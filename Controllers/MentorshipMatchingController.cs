using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Freelancing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        // Main matching page - shows potential matches and existing matches
        public async Task<IActionResult> AvailableMentors()
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
                if (mentorship.Role.ToLower() == "mentee")
                {
                    potentialMatches = await _matchingService.FindPotentialMentorsAsync(userId);
                }
            }

            var viewModel = new MentorshipMatchingViewModel
            {
                PotentialMatches = potentialMatches,
                UserRole = mentorship.Role,
                TotalSkills = userSkills.Count,
                UserSkills = userSkills,
                HasSkills = userSkills.Any()
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> CreateRequest(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // Get the potential mentor/partner
            var partner = await _context.UserAccounts
                .Include(ua => ua.UserAccountSkills)
                .ThenInclude(uas => uas.UserSkill)
                .FirstOrDefaultAsync(ua => ua.Id == id);

            if (partner == null)
                return NotFound();

            // Check if a request already exists (including declined ones)
            var existingMatch = await _context.MentorshipMatches
                .FirstOrDefaultAsync(mm => mm.MenteeId == userId &&
                                          mm.MentorId == id &&
                                          (mm.Status == "Pending" || mm.Status == "Active" || mm.Status == "Declined"));

            // Check if mentee can re-request after a declined request
            bool canReRequest = false;
            DateTime? nextRequestDate = null;

            if (existingMatch != null && existingMatch.Status == "Declined")
            {
                var daysSinceDecline = (DateTime.UtcNow - existingMatch.DeclinedDate.GetValueOrDefault()).TotalDays;
                canReRequest = daysSinceDecline >= 7; // Allow re-request after 7 days

                if (!canReRequest)
                {
                    nextRequestDate = existingMatch.DeclinedDate.GetValueOrDefault().AddDays(7);
                }
            }

            // Get current user's skills
            var userSkills = await _context.UserAccountSkills
                .Where(uas => uas.UserAccountId == userId)
                .Include(uas => uas.UserSkill)
                .Select(uas => uas.UserSkill)
                .ToListAsync();

            // Get potential matches/prospects (optional - for showing alternatives)
            var prospects = await _context.UserAccounts
                .Where(ua => ua.Id != userId && ua.Id != id)
                .Include(ua => ua.UserAccountSkills)
                .ThenInclude(uas => uas.UserSkill)
                .Take(5)
                .ToListAsync();

            var model = new CreateMatchRequest
            {
                PartnerId = id,
                UserSkills = userSkills,
                Prospect = prospects,
                Partner = partner
            };

            // Set ViewBag properties based on existing request status
            if (existingMatch != null)
            {
                ViewBag.ExistingRequest = true;
                ViewBag.RequestStatus = existingMatch.Status;
                ViewBag.RequestDate = existingMatch.MatchedDate;
                ViewBag.RequestNotes = existingMatch.Notes;
                ViewBag.CanReRequest = canReRequest;
                ViewBag.PartnerId = id; // For ViewMentorship link

                // Set appropriate message based on status
                if (existingMatch.Status == "Pending")
                {
                    ViewBag.StatusMessage = "You have already sent a mentorship request to this mentor. Please wait for their response.";
                    ViewBag.StatusClass = "text-yellow-800 border-yellow-300 bg-yellow-50";
                }
                else if (existingMatch.Status == "Active")
                {
                    ViewBag.StatusMessage = "You already have an active mentorship with this mentor.";
                    ViewBag.StatusClass = "text-green-800 border-green-300 bg-green-50";
                }
                else if (existingMatch.Status == "Declined")
                {
                    if (canReRequest)
                    {
                        ViewBag.StatusMessage = "Your previous request was declined, but you can now send a new request.";
                        ViewBag.StatusClass = "text-blue-800 border-blue-300 bg-blue-50";
                        ViewBag.ExistingRequest = false; // Allow form to show
                    }
                    else
                    {
                        var daysRemaining = Math.Ceiling(7 - (DateTime.UtcNow - existingMatch.DeclinedDate.GetValueOrDefault()).TotalDays);
                        var hoursRemaining = Math.Ceiling((nextRequestDate.Value - DateTime.UtcNow).TotalHours);

                        // More precise messaging based on time remaining
                        if (daysRemaining > 1)
                        {
                            ViewBag.StatusMessage = $"Your request was declined. You can send a new request in {daysRemaining} days.";
                        }
                        else if (hoursRemaining > 1)
                        {
                            ViewBag.StatusMessage = $"Your request was declined. You can send a new request in {hoursRemaining} hours.";
                        }
                        else
                        {
                            ViewBag.StatusMessage = "Your request was declined. You can send a new request very soon.";
                        }

                        ViewBag.StatusClass = "text-red-800 border-red-300 bg-red-50";
                        ViewBag.DaysRemaining = (int)daysRemaining;
                        ViewBag.NextRequestDate = nextRequestDate?.ToString("MMM dd, yyyy 'at' h:mm tt");
                    }
                }
            }

            return View(model);
        }

        // Enhanced POST method with better validation and messaging
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(CreateMatchRequest model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the partner and user skills if validation fails
                await ReloadCreateRequestModel(model);
                return View(model);
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // Check for existing matches
            var existingMatch = await _context.MentorshipMatches
                .Include(mm => mm.Mentor) // Include mentor for better messaging
                .FirstOrDefaultAsync(mm => mm.MenteeId == userId &&
                                          mm.MentorId == model.PartnerId);

            // Handle different scenarios
            if (existingMatch != null)
            {
                // If pending or active, don't allow new request
                if (existingMatch.Status == "Pending" || existingMatch.Status == "Active")
                {
                    string message = existingMatch.Status == "Pending"
                        ? "You already have a pending request with this mentor."
                        : "You already have an active mentorship with this mentor.";
                    TempData["Error"] = message;
                    await ReloadCreateRequestModel(model);
                    return View(model);
                }

                // If declined, check if a week has passed
                if (existingMatch.Status == "Declined")
                {
                    var daysSinceDecline = (DateTime.UtcNow - existingMatch.DeclinedDate.GetValueOrDefault()).TotalDays;

                    if (daysSinceDecline < 7)
                    {
                        var daysRemaining = Math.Ceiling(7 - daysSinceDecline);
                        TempData["Error"] = $"You can send a new request in {daysRemaining} day(s).";
                        await ReloadCreateRequestModel(model);
                        return View(model);
                    }

                    // Update existing declined request to pending with new details
                    existingMatch.Status = "Pending";
                    existingMatch.MatchedDate = DateTime.UtcNow; // Update request date
                    existingMatch.Notes = model.Notes; // Update with new message
                    existingMatch.DeclinedDate = null; // Clear declined date

                    await _context.SaveChangesAsync();

                    // Set success message for re-request
                    TempData["Success"] = $"Your new mentorship request has been sent to {existingMatch.Mentor.FirstName} {existingMatch.Mentor.LastName}!";
                    await ReloadCreateRequestModel(model);
                    ViewBag.RequestSent = true;
                    ViewBag.SentMessage = model.Notes;
                    ViewBag.CanReRequest = false; // Reset flag
                    return View(model);
                }
            }

            // Create new request (no existing match found)
            // Get the mentor's PeerMentorship record
            var mentorMentorship = await _context.PeerMentorships
                .FirstOrDefaultAsync(pm => pm.UserId == model.PartnerId);

            // Get or create the mentee's PeerMentorship record
            var menteeMentorship = await _context.PeerMentorships
                .FirstOrDefaultAsync(pm => pm.UserId == userId);

            if (menteeMentorship == null)
            {
                menteeMentorship = new PeerMentorship
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    // Set other required properties based on your PeerMentorship entity
                };
                _context.PeerMentorships.Add(menteeMentorship);
                await _context.SaveChangesAsync();
            }

            // Create the mentorship match with pending status
            var mentorshipMatch = new MentorshipMatch
            {
                Id = Guid.NewGuid(),
                MentorId = model.PartnerId,
                MenteeId = userId,
                MentorMentorshipId = mentorMentorship?.Id ?? Guid.Empty,
                MenteeMentorshipId = menteeMentorship.Id,
                MatchedDate = DateTime.UtcNow,
                Status = "Pending",
                Notes = model.Notes
            };

            _context.MentorshipMatches.Add(mentorshipMatch);
            await _context.SaveChangesAsync();

            // Get mentor name for success message
            var mentor = await _context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.Id == model.PartnerId);

            // Reload the model and show confirmation
            await ReloadCreateRequestModel(model);
            ViewBag.RequestSent = true;
            ViewBag.SentMessage = model.Notes;
            TempData["Success"] = $"Mentorship request sent successfully to {mentor?.FirstName} {mentor?.LastName}! You'll be notified when they respond.";

            return View(model);
        }

        private async Task ReloadCreateRequestModel(CreateMatchRequest model)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                model.Partner = await _context.UserAccounts
                    .Include(ua => ua.UserAccountSkills)
                    .ThenInclude(uas => uas.UserSkill)
                    .FirstOrDefaultAsync(ua => ua.Id == model.PartnerId);

                model.UserSkills = await _context.UserAccountSkills
                    .Where(uas => uas.UserAccountId == userId)
                    .Include(uas => uas.UserSkill)
                    .Select(uas => uas.UserSkill)
                    .ToListAsync();
            }
        }
        public async Task<IActionResult> MenteeDashboard()
        {
            var currentUserId = GetCurrentUserId(); // Implement this method based on your auth system

            var dashboardModel = new MenteeDashboard();

            // Get mentorship requests sent by this mentee
            dashboardModel.RequestsSent = await _context.MentorshipMatches
                .Where(mm => mm.MenteeId == currentUserId)
                .Include(mm => mm.Mentor)
                .Select(mm => new MentorshipRequestViewModel
                {
                    Id = mm.Id,
                    MentorName = $"{mm.Mentor.FirstName} {mm.Mentor.LastName}",
                    Status = mm.Status,
                    RequestDate = mm.MatchedDate,
                    StartDate = mm.StartDate,
                    Notes = mm.Notes
                })
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(dashboardModel);
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        public async Task<IActionResult> MentorDashboard()
        {
            var currentUserId = GetUserId();
            var dashboardModel = new MentorDashboard();

            // Get mentorship requests RECEIVED by this mentor
            dashboardModel.RequestsReceived = await _context.MentorshipMatches
                .Where(mm => mm.MentorId == currentUserId) // Current user is the mentor receiving requests
                .Include(mm => mm.Mentee) // Include mentee who sent the request
                .Select(mm => new MentorshipReceivedViewModel
                {
                    Id = mm.Id,
                    MenteeName = $"{mm.Mentee.FirstName} {mm.Mentee.LastName}", // Show mentee name
                    Status = mm.Status,
                    RequestDate = mm.MatchedDate,
                    StartDate = mm.StartDate,
                    Notes = mm.Notes
                })
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(dashboardModel);
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        [HttpGet]
        public async Task<IActionResult> PendingRequests()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var pendingRequests = await _context.MentorshipMatches
                .Where(mm => mm.MentorId == userId && mm.Status == "Pending")
                .Include(mm => mm.Mentee)
                .ThenInclude(m => m.UserAccountSkills)
                .ThenInclude(uas => uas.UserSkill)
                .OrderByDescending(mm => mm.MatchedDate)
                .ToListAsync();

            return View(pendingRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PendingRequests(Guid matchId, string response)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var mentorshipMatch = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .FirstOrDefaultAsync(mm => mm.Id == matchId && mm.MentorId == userId);

            if (mentorshipMatch == null)
                return NotFound();

            if (mentorshipMatch.Status != "Pending")
            {
                TempData["Error"] = "This request has already been responded to.";
                return RedirectToAction("PendingRequests");
            }

            if (response.ToLower() == "accept")
            {
                mentorshipMatch.Status = "Active";
                mentorshipMatch.StartDate = DateTime.UtcNow;
                // Clear any previous declined date since this is now accepted
                mentorshipMatch.DeclinedDate = null;

                TempData["Success"] = $"You've accepted the mentorship request from {mentorshipMatch.Mentee.FirstName} {mentorshipMatch.Mentee.LastName}!";
            }
            else if (response.ToLower() == "decline")
            {
                mentorshipMatch.Status = "Declined";
                mentorshipMatch.DeclinedDate = DateTime.UtcNow; // Set the declined date for re-request functionality

                TempData["Info"] = $"You've declined the mentorship request from {mentorshipMatch.Mentee.FirstName} {mentorshipMatch.Mentee.LastName}.";
            }
            else
            {
                TempData["Error"] = "Invalid response.";
                return RedirectToAction("PendingRequests");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("PendingRequests");
        }
    }
}