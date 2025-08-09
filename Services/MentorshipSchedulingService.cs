using Freelancing.Data;
using Freelancing.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Freelancing.Services
{
    public interface IMentorshipSchedulingService
    {
        Task<List<MentorshipSession>> GetSessionsAsync(Guid matchId);
        Task<MentorshipSession?> GetSessionAsync(Guid sessionId);
        Task<(bool ok, string? error, MentorshipSession? session)> CreateSessionAsync(Guid matchId, Guid createdByUserId, DateTime startUtc, string? title, string? notes, string? timeZone);
        Task<(bool ok, string? error)> AcceptAsync(Guid sessionId, Guid userId);
        Task<(bool ok, string? error)> DeclineAsync(Guid sessionId, Guid userId);
        Task<(bool ok, string? error)> CancelAsync(Guid sessionId, Guid userId);
        Task<(bool ok, string? error)> RescheduleAsync(Guid sessionId, Guid userId, DateTime newStartUtc, string? notes);
    }

    public class MentorshipSchedulingService : IMentorshipSchedulingService
    {
        private readonly ApplicationDbContext _context;

        public MentorshipSchedulingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MentorshipSession>> GetSessionsAsync(Guid matchId)
        {
            return await _context.Set<MentorshipSession>()
                .Where(s => s.MentorshipMatchId == matchId)
                .OrderByDescending(s => s.ScheduledStartUtc)
                .ToListAsync();
        }

        public async Task<MentorshipSession?> GetSessionAsync(Guid sessionId)
        {
            return await _context.Set<MentorshipSession>().FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<(bool ok, string? error, MentorshipSession? session)> CreateSessionAsync(
            Guid matchId,
            Guid createdByUserId,
            DateTime startUtc,
            string? title,
            string? notes,
            string? timeZone)
        {
            // Validate start is not in the past (local time semantics)
            if (startUtc < DateTime.Now)
            {
                return (false, "Start date and time must be now or later", null);
            }

            var match = await _context.MentorshipMatches.FirstOrDefaultAsync(m => m.Id == matchId && m.Status == "Active");
            if (match == null)
            {
                return (false, "Mentorship match not found or not active", null);
            }

            if (match.MentorId != createdByUserId && match.MenteeId != createdByUserId)
            {
                return (false, "Not authorized for this mentorship match", null);
            }

            // Overlap check for both mentor and mentee against Confirmed sessions
            var participantIds = new[] { match.MentorId, match.MenteeId };
            var overlapping = await _context.Set<MentorshipSession>()
                .Where(s => s.Status == "Confirmed")
                .Where(s => participantIds.Contains(s.MentorshipMatch.MentorId) || participantIds.Contains(s.MentorshipMatch.MenteeId))
                .AnyAsync();

            if (overlapping)
            {
                return (false, "Time overlaps with an existing confirmed session", null);
            }

            var entity = new MentorshipSession
            {
                Id = Guid.NewGuid(),
                MentorshipMatchId = matchId,
                CreatedByUserId = createdByUserId,
                // Store as provided (treat as local)
                ScheduledStartUtc = startUtc,
                Title = title,
                Notes = notes,
                TimeZone = timeZone,
                Status = "Proposed",
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Set<MentorshipSession>().Add(entity);
            await _context.SaveChangesAsync();

            return (true, null, entity);
        }

        public async Task<(bool ok, string? error)> AcceptAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.Set<MentorshipSession>()
                .Include(s => s.MentorshipMatch)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return (false, "Session not found");

            var match = session.MentorshipMatch;
            if (match.Status != "Active") return (false, "Match not active");
            if (match.MentorId != userId && match.MenteeId != userId) return (false, "Not authorized");
            if (session.Status != "Proposed") return (false, "Only proposed sessions can be accepted");
            if (session.CreatedByUserId == userId) return (false, "You cannot accept your own proposal");

            // Re-run overlap for current times
            var participantIds = new[] { match.MentorId, match.MenteeId };
            var overlapping = await _context.Set<MentorshipSession>()
                .Where(s => s.Id != session.Id && s.Status == "Confirmed")
                .Where(s => participantIds.Contains(s.MentorshipMatch.MentorId) || participantIds.Contains(s.MentorshipMatch.MenteeId))
                .AnyAsync();
            if (overlapping) return (false, "Time overlaps with existing confirmed session");

            session.Status = "Confirmed";
            session.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> DeclineAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.Set<MentorshipSession>()
                .Include(s => s.MentorshipMatch)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return (false, "Session not found");
            var match = session.MentorshipMatch;
            if (match.MentorId != userId && match.MenteeId != userId) return (false, "Not authorized");
            if (session.Status != "Proposed") return (false, "Only proposed sessions can be declined");
            if (session.CreatedByUserId == userId) return (false, "You cannot decline your own proposal");

            session.Status = "Cancelled";
            session.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> CancelAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.Set<MentorshipSession>()
                .Include(s => s.MentorshipMatch)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return (false, "Session not found");
            var match = session.MentorshipMatch;
            if (match.MentorId != userId && match.MenteeId != userId) return (false, "Not authorized");
            if (session.Status != "Confirmed") return (false, "Only confirmed sessions can be cancelled");

            session.Status = "Cancelled";
            session.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error)> RescheduleAsync(Guid sessionId, Guid userId, DateTime newStartUtc, string? notes)
        {
            if (newStartUtc < DateTime.Now) return (false, "Start date and time must be now or later");

            var session = await _context.Set<MentorshipSession>()
                .Include(s => s.MentorshipMatch)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return (false, "Session not found");

            var match = session.MentorshipMatch;
            if (match.MentorId != userId && match.MenteeId != userId) return (false, "Not authorized");
            if (session.Status == "Completed") return (false, "Cannot reschedule a completed session");

            // Check overlapping against other confirmed sessions
            var participantIds = new[] { match.MentorId, match.MenteeId };
            var overlapping = await _context.Set<MentorshipSession>()
                .Where(s => s.Id != session.Id && s.Status == "Confirmed")
                .Where(s => participantIds.Contains(s.MentorshipMatch.MentorId) || participantIds.Contains(s.MentorshipMatch.MenteeId))
                .AnyAsync();
            if (overlapping) return (false, "Time overlaps with existing confirmed session");

            // Keep as provided (local semantics)
            session.ScheduledStartUtc = newStartUtc;
            session.Status = session.Status == "Proposed" ? "Proposed" : "Confirmed"; // keep confirmed if already accepted
            session.Notes = notes ?? session.Notes;
            session.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}


