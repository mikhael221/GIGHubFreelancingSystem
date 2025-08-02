using System.Security.Claims;
using Freelancing.Data;
using Freelancing.Models.Entities;
using Freelancing.Models;
using Freelancing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Freelancing.Controllers
{
    [Authorize]
    public class MentorshipMatchingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMentorshipMatchingService _matchingService;

        public MentorshipMatchingController(ApplicationDbContext context, IMentorshipMatchingService matchingService)
        {
            _context = context;
            _matchingService = matchingService;
        }
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // Check if user is registered in mentorship program
            var mentorship = await _context.PeerMentorships
                .FirstOrDefaultAsync(pm => pm.UserId == userId);

            if (mentorship == null)
            {
                TempData["ErrorMessage"] = "You must be registered in the peer mentorship program to access matching.";
                return RedirectToAction("Registration", "PeerMentorship");
            }

            // Get user's skills
            var userSkills = await _context.UserAccountSkills
                .Where(uas => uas.UserAccountId == userId)
                .Include(uas => uas.UserSkill)
                .Select(uas => uas.UserSkill)
                .ToListAsync();

            List<UserAccount> potentialMatches = new();

            if (userSkills.Any())
            {
                // Find potential matches based on role
                if (mentorship.Role.ToLower() == "Mentor")
                {
                    potentialMatches = await _matchingService.FindPotentialMenteesAsync(userId);
                }
                else if (mentorship.Role.ToLower() == "Mentee")
                {
                    potentialMatches = await _matchingService.FindPotentialMentorsAsync(userId);
                }
            }

            // Get existing matches
            var existingMatches = await _matchingService.GetUserMatchesAsync(userId);

            var viewModel = new MentorshipMatchingViewModel
            {
                PotentialMatches = potentialMatches,
                ExistingMatches = existingMatches,
                UserRole = mentorship.Role,
                TotalSkills = userSkills.Count,
                UserSkills = userSkills,
                HasSkills = userSkills.Any()
            };

            return View(viewModel);
        }
        public async Task<IActionResult> MatchDetails(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var match = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .ThenInclude(m => m.UserAccountSkills)
                .ThenInclude(uas => uas.UserSkill)
                .Include(mm => mm.Mentee)
                .ThenInclude(m => m.UserAccountSkills)
                .ThenInclude(uas => uas.UserSkill)
                .Include(mm => mm.MentorMentorship)
                .Include(mm => mm.MenteeMentorship)
                .FirstOrDefaultAsync(mm => mm.Id == id);

            if (match == null)
                return NotFound();

            // Ensure user is part of this match
            if (match.MentorId != userId && match.MenteeId != userId)
                return Forbid();

            bool isCurrentUserMentor = match.MentorId == userId;
            var partner = isCurrentUserMentor ? match.Mentee : match.Mentor;
            var partnerRole = isCurrentUserMentor ? "Mentee" : "Mentor";

            // Get shared skills
            var sharedSkills = partner.UserAccountSkills
                .Select(uas => uas.UserSkill)
                .ToList();

            var viewModel = new MatchDetailsViewModel
            {
                Match = match,
                Partner = partner,
                PartnerRole = partnerRole,
                SharedSkills = sharedSkills,
                IsCurrentUserMentor = isCurrentUserMentor
            };

            return View(viewModel);
        }

        // Create a new match
        [HttpPost]
        public async Task<IActionResult> CreateMatch(Guid partnerId, string notes = "")
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out Guid userId))
                {
                    TempData["ErrorMessage"] = "Unable to identify current user.";
                    return RedirectToAction("Index");
                }

                // Debug: Log the IDs
                Console.WriteLine($"Current User ID: {userId}");
                Console.WriteLine($"Partner ID: {partnerId}");
                Console.WriteLine($"Notes: {notes}");

                // Get current user's mentorship info
                var currentUserMentorship = await _context.PeerMentorships
                    .FirstOrDefaultAsync(pm => pm.UserId == userId);

                if (currentUserMentorship == null)
                {
                    TempData["ErrorMessage"] = "You are not registered in the mentorship program.";
                    return RedirectToAction("Index");
                }

                // Get partner's mentorship info
                var partnerMentorship = await _context.PeerMentorships
                    .FirstOrDefaultAsync(pm => pm.UserId == partnerId);

                if (partnerMentorship == null)
                {
                    TempData["ErrorMessage"] = "Partner is not registered in the mentorship program.";
                    return RedirectToAction("Index");
                }

                // Debug: Log the roles
                Console.WriteLine($"Current User Role: {currentUserMentorship.Role}");
                Console.WriteLine($"Partner Role: {partnerMentorship.Role}");

                // Determine mentor and mentee IDs
                Guid mentorId, menteeId, mentorMentorshipId, menteeMentorshipId;

                if (currentUserMentorship.Role.ToLower() == "Mentor" && partnerMentorship.Role.ToLower() == "Mentee")
                {
                    mentorId = userId;
                    menteeId = partnerId;
                    mentorMentorshipId = currentUserMentorship.Id;
                    menteeMentorshipId = partnerMentorship.Id;
                }
                else if (currentUserMentorship.Role.ToLower() == "Mentee" && partnerMentorship.Role.ToLower() == "Mentor")
                {
                    mentorId = partnerId;
                    menteeId = userId;
                    mentorMentorshipId = partnerMentorship.Id;
                    menteeMentorshipId = currentUserMentorship.Id;
                }
                else
                {
                    TempData["ErrorMessage"] = $"Cannot create match between {currentUserMentorship.Role} and {partnerMentorship.Role}.";
                    return RedirectToAction("Index");
                }

                // Debug: Log final IDs
                Console.WriteLine($"Final Mentor ID: {mentorId}");
                Console.WriteLine($"Final Mentee ID: {menteeId}");

                // Check if match already exists
                var existingMatch = await _context.MentorshipMatches
                    .FirstOrDefaultAsync(mm => mm.MentorId == mentorId && mm.MenteeId == menteeId);

                if (existingMatch != null)
                {
                    TempData["ErrorMessage"] = "A match between these users already exists.";
                    return RedirectToAction("Index");
                }

                // Verify skill compatibility
                var mentorSkills = await _context.UserAccountSkills
                    .Where(uas => uas.UserAccountId == mentorId)
                    .Select(uas => uas.UserSkillId)
                    .OrderBy(id => id)
                    .ToListAsync();

                var menteeSkills = await _context.UserAccountSkills
                    .Where(uas => uas.UserAccountId == menteeId)
                    .Select(uas => uas.UserSkillId)
                    .OrderBy(id => id)
                    .ToListAsync();

                // Debug: Log skills
                Console.WriteLine($"Mentor Skills Count: {mentorSkills.Count}");
                Console.WriteLine($"Mentee Skills Count: {menteeSkills.Count}");

                if (!mentorSkills.SequenceEqual(menteeSkills))
                {
                    TempData["ErrorMessage"] = "Skills do not match 100%. Cannot create match.";
                    return RedirectToAction("Index");
                }

                // Create the match
                var match = new MentorshipMatch
                {
                    Id = Guid.NewGuid(),
                    MentorId = mentorId,
                    MenteeId = menteeId,
                    MentorMentorshipId = mentorMentorshipId,
                    MenteeMentorshipId = menteeMentorshipId,
                    MatchedDate = DateTime.UtcNow,
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    Notes = notes
                };

                // Debug: Log the match object
                Console.WriteLine($"Match ID: {match.Id}");
                Console.WriteLine($"Status: {match.Status}");
                Console.WriteLine($"MatchedDate: {match.MatchedDate}");

                // Add to context and save
                _context.MentorshipMatches.Add(match);

                // Debug: Check if entity is being tracked
                var entry = _context.Entry(match);
                Console.WriteLine($"Entity State: {entry.State}");

                var changesSaved = await _context.SaveChangesAsync();

                // Debug: Log changes saved
                Console.WriteLine($"Changes Saved: {changesSaved}");

                if (changesSaved > 0)
                {
                    TempData["SuccessMessage"] = "Match created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "No changes were saved to the database.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the full exception
                Console.WriteLine($"Exception in CreateMatch: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Update match status
        [HttpPost]
        public async Task<IActionResult> UpdateMatchStatus(Guid matchId, string status, string notes = "")
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var match = await _context.MentorshipMatches
                .FirstOrDefaultAsync(mm => mm.Id == matchId);

            if (match == null)
                return NotFound();

            // Ensure user is part of this match
            if (match.MentorId != userId && match.MenteeId != userId)
                return Forbid();

            // Validate status
            var validStatuses = new[] { "Active", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
                return BadRequest("Invalid status");

            match.Status = status;
            if (!string.IsNullOrEmpty(notes))
                match.Notes = notes;

            if (status == "Completed" || status == "Cancelled")
                match.EndDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Match status updated to {status}.";
            return RedirectToAction("MatchDetails", new { id = matchId });
        }

        // Get matches list with filtering
        public async Task<IActionResult> MyMatches(string status = "all")
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var mentorship = await _context.PeerMentorships
                .FirstOrDefaultAsync(pm => pm.UserId == userId);

            if (mentorship == null)
                return RedirectToAction("Registration", "PeerMentorship");

            var allMatches = await _matchingService.GetUserMatchesAsync(userId);

            var viewModel = new MentorshipMatchListViewModel
            {
                ActiveMatches = allMatches.Where(m => m.Status == "Active").ToList(),
                CompletedMatches = allMatches.Where(m => m.Status == "Completed").ToList(),
                CancelledMatches = allMatches.Where(m => m.Status == "Cancelled").ToList(),
                TotalActiveMatches = allMatches.Count(m => m.Status == "Active"),
                UserRole = mentorship.Role
            };

            return View(viewModel);
        }

        // AJAX endpoint to check skill compatibility
        [HttpGet]
        public async Task<IActionResult> CheckCompatibility(Guid partnerId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Json(new { compatible = false, message = "Unauthorized" });

            // Get both users' skills
            var userSkills = await _context.UserAccountSkills
                .Where(uas => uas.UserAccountId == userId)
                .Select(uas => uas.UserSkillId)
                .OrderBy(id => id)
                .ToListAsync();

            var partnerSkills = await _context.UserAccountSkills
                .Where(uas => uas.UserAccountId == partnerId)
                .Select(uas => uas.UserSkillId)
                .OrderBy(id => id)
                .ToListAsync();

            var compatible = userSkills.SequenceEqual(partnerSkills);
            var message = compatible ? "100% skill match found!" : "Skills do not match completely.";

            return Json(new { compatible, message, userSkillCount = userSkills.Count, partnerSkillCount = partnerSkills.Count });
        }
    }
}
