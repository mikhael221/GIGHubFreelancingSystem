using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models.Entities;
using Freelancing.Models;
using Freelancing.Services;
using System.Security.Claims;

namespace Freelancing.Controllers
{
    [Authorize]
    public class MentorshipManageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMentorshipSchedulingService _schedulingService;

        public MentorshipManageController(ApplicationDbContext context, IMentorshipSchedulingService schedulingService)
        {
            _context = context;
            _schedulingService = schedulingService;
        }

        [HttpGet]
        public async Task<IActionResult> Goals(Guid matchId)
        {
            var userId = GetCurrentUserId();
            var match = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .FirstOrDefaultAsync(mm => mm.Id == matchId && (mm.MentorId == userId || mm.MenteeId == userId) && (mm.Status == "Active" || mm.Status == "Completed"));
            
            if (match == null)
            {
                TempData["Error"] = "Access denied or mentorship not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }

            var isCurrentUserMentor = match.MentorId == userId;
            var partnerName = isCurrentUserMentor 
                ? $"{match.Mentee.FirstName} {match.Mentee.LastName}"
                : $"{match.Mentor.FirstName} {match.Mentor.LastName}";

            // Get all active goals ordered by their sequence
            var goals = await _context.Goals
                .Where(g => g.IsActive)
                .OrderBy(g => g.Order)
                .ToListAsync();

            // Get completion status for each goal
            var goalCompletions = await _context.MentorshipGoalCompletions
                .Where(mgc => mgc.MentorshipMatchId == matchId)
                .ToListAsync();

            var goalViewModels = new List<GoalItemViewModel>();
            var completedGoalsCount = 0;

            for (int i = 0; i < goals.Count; i++)
            {
                var goal = goals[i];
                var completions = goalCompletions.Where(mgc => mgc.GoalId == goal.Id).ToList();
                
                var isCompletedByMentor = completions.Any(c => c.CompletedByUserId == match.MentorId);
                var isCompletedByMentee = completions.Any(c => c.CompletedByUserId == match.MenteeId);
                var isFullyCompleted = isCompletedByMentor && isCompletedByMentee;

                if (isFullyCompleted)
                {
                    completedGoalsCount++;
                }

                // Determine if current user can mark this goal as done
                var canMarkAsDone = false;
                if (isCurrentUserMentor)
                {
                    canMarkAsDone = !isCompletedByMentor;
                }
                else
                {
                    canMarkAsDone = !isCompletedByMentee;
                }

                // Determine if the mark as done button should be shown
                // Only show if previous goal is completed (except for the first goal)
                var showMarkAsDoneButton = false;
                if (i == 0) // First goal
                {
                    showMarkAsDoneButton = canMarkAsDone;
                }
                else // Subsequent goals
                {
                    var previousGoal = goals[i - 1];
                    var previousCompletions = goalCompletions.Where(mgc => mgc.GoalId == previousGoal.Id).ToList();
                    var previousCompletedByMentor = previousCompletions.Any(c => c.CompletedByUserId == match.MentorId);
                    var previousCompletedByMentee = previousCompletions.Any(c => c.CompletedByUserId == match.MenteeId);
                    var previousFullyCompleted = previousCompletedByMentor && previousCompletedByMentee;
                    
                    showMarkAsDoneButton = previousFullyCompleted && canMarkAsDone;
                }

                // Determine completion status text
                string completedBy = "";
                if (isFullyCompleted)
                {
                    completedBy = "Both";
                }
                else if (isCompletedByMentor)
                {
                    completedBy = "Mentor";
                }
                else if (isCompletedByMentee)
                {
                    completedBy = "Mentee";
                }

                var goalViewModel = new GoalItemViewModel
                {
                    GoalId = goal.Id,
                    GoalName = goal.GoalName,
                    GoalDescription = goal.GoalDescription,
                    Order = goal.Order,
                    IsCompletedByMentor = isCompletedByMentor,
                    IsCompletedByMentee = isCompletedByMentee,
                    CanMarkAsDone = canMarkAsDone,
                    ShowMarkAsDoneButton = showMarkAsDoneButton,
                    CompletedBy = completedBy,
                    CompletedAt = completions.Any() ? completions.Max(c => c.CompletedAt) : null,
                    IconSvg = goal.IconSvg
                };

                goalViewModels.Add(goalViewModel);
            }

            var viewModel = new GoalViewModel
            {
                MatchId = matchId,
                PartnerName = partnerName,
                IsCurrentUserMentor = isCurrentUserMentor,
                Goals = goalViewModels,
                TotalGoals = goals.Count,
                CompletedGoals = completedGoalsCount
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Sessions(Guid matchId)
        {
            var userId = GetCurrentUserId();
            var match = await _context.MentorshipMatches
                .FirstOrDefaultAsync(mm => mm.Id == matchId && (mm.MentorId == userId || mm.MenteeId == userId) && (mm.Status == "Active" || mm.Status == "Completed"));
            if (match == null)
            {
                TempData["Error"] = "Access denied or mentorship not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }

            var sessions = await _schedulingService.GetSessionsAsync(matchId);
            ViewBag.MatchId = matchId;
            ViewBag.CurrentUserId = userId;
            return View(sessions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession(Guid matchId, DateTime startUtc, string? title, string? notes, string? timeZone = null, int tzOffsetMinutes = 0)
        {
            var userId = GetCurrentUserId();
            var auth = await _context.MentorshipMatches
                .AnyAsync(mm => mm.Id == matchId && (mm.MentorId == userId || mm.MenteeId == userId) && (mm.Status == "Active" || mm.Status == "Completed"));
            if (!auth)
            {
                TempData["Error"] = "Access denied or mentorship not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }

            // Per requirement: treat inputs as local and store/display consistently (no UTC conversion)
            var result = await _schedulingService.CreateSessionAsync(matchId, userId, startUtc, title, notes, timeZone);
            if (!result.ok)
            {
                TempData["Error"] = result.error;
            }
            else
            {
                TempData["Success"] = "Session proposed";
            }
            return RedirectToAction("Sessions", new { matchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptSession(Guid sessionId, string? returnUrl)
        {
            var userId = GetCurrentUserId();
            var session = await _schedulingService.GetSessionAsync(sessionId);
            if (session == null)
            {
                TempData["Error"] = "Session not found";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }

            var result = await _schedulingService.AcceptAsync(sessionId, userId);
            TempData[result.ok ? "Success" : "Error"] = result.ok ? "Session confirmed" : result.error;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Sessions", new { matchId = session.MentorshipMatchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineSession(Guid sessionId, string? returnUrl)
        {
            var userId = GetCurrentUserId();
            var session = await _schedulingService.GetSessionAsync(sessionId);
            if (session == null)
            {
                TempData["Error"] = "Session not found";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }
            var result = await _schedulingService.DeclineAsync(sessionId, userId);
            TempData[result.ok ? "Success" : "Error"] = result.ok ? "Session declined" : result.error;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Sessions", new { matchId = session.MentorshipMatchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelSession(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            var session = await _schedulingService.GetSessionAsync(sessionId);
            if (session == null)
            {
                TempData["Error"] = "Session not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }
            var result = await _schedulingService.CancelAsync(sessionId, userId);
            TempData[result.ok ? "Success" : "Error"] = result.ok ? "Session cancelled" : result.error;
            return RedirectToAction("Sessions", new { matchId = session.MentorshipMatchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleSession(Guid sessionId, DateTime startUtc, string? notes)
        {
            var userId = GetCurrentUserId();
            var session = await _schedulingService.GetSessionAsync(sessionId);
            if (session == null)
            {
                TempData["Error"] = "Session not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }
            var result = await _schedulingService.RescheduleAsync(sessionId, userId, startUtc, notes);
            TempData[result.ok ? "Success" : "Error"] = result.ok ? "Session rescheduled" : result.error;
            return RedirectToAction("Sessions", new { matchId = session.MentorshipMatchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkGoalAsDone(Guid matchId, Guid goalId)
        {
            var userId = GetCurrentUserId();
            var match = await _context.MentorshipMatches
                .FirstOrDefaultAsync(mm => mm.Id == matchId && (mm.MentorId == userId || mm.MenteeId == userId) && (mm.Status == "Active" || mm.Status == "Completed"));
            
            if (match == null)
            {
                TempData["Error"] = "Access denied or mentorship not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }

            var isCurrentUserMentor = match.MentorId == userId;
            var completionType = isCurrentUserMentor ? "Mentor" : "Mentee";

            // Check if goal already completed by this user
            var existingCompletion = await _context.MentorshipGoalCompletions
                .FirstOrDefaultAsync(mgc => mgc.MentorshipMatchId == matchId && 
                                          mgc.GoalId == goalId && 
                                          mgc.CompletedByUserId == userId);

            if (existingCompletion != null)
            {
                TempData["Error"] = "You have already marked this goal as done";
                return RedirectToAction("Goals", new { matchId });
            }

            // Verify that the goal exists and is active
            var goal = await _context.Goals
                .FirstOrDefaultAsync(g => g.Id == goalId && g.IsActive);

            if (goal == null)
            {
                TempData["Error"] = "Goal not found";
                return RedirectToAction("Goals", new { matchId });
            }

            // Check if previous goal is completed (except for the first goal)
            if (goal.Order > 1)
            {
                var previousGoal = await _context.Goals
                    .FirstOrDefaultAsync(g => g.Order == goal.Order - 1 && g.IsActive);

                if (previousGoal != null)
                {
                    var previousCompletions = await _context.MentorshipGoalCompletions
                        .Where(mgc => mgc.MentorshipMatchId == matchId && mgc.GoalId == previousGoal.Id)
                        .ToListAsync();

                    var previousCompletedByMentor = previousCompletions.Any(c => c.CompletedByUserId == match.MentorId);
                    var previousCompletedByMentee = previousCompletions.Any(c => c.CompletedByUserId == match.MenteeId);
                    var previousFullyCompleted = previousCompletedByMentor && previousCompletedByMentee;

                    if (!previousFullyCompleted)
                    {
                        TempData["Error"] = "Previous goal must be completed before marking this goal as done";
                        return RedirectToAction("Goals", new { matchId });
                    }
                }
            }

            // Create the completion record
            var completion = new MentorshipGoalCompletion
            {
                MentorshipMatchId = matchId,
                GoalId = goalId,
                CompletedByUserId = userId,
                CompletedAt = DateTime.Now,
                CompletionType = completionType
            };

            _context.MentorshipGoalCompletions.Add(completion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Goal marked as done successfully";
            return RedirectToAction("Goals", new { matchId });
        }

        [HttpGet]
        public async Task<IActionResult> Feedback(Guid matchId)
        {
            var userId = GetCurrentUserId();
            var match = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .FirstOrDefaultAsync(mm => mm.Id == matchId && mm.MenteeId == userId && mm.Status == "Completed");
            
            if (match == null)
            {
                TempData["Error"] = "Access denied or completed mentorship not found";
                return RedirectToAction("MenteeDashboard", "MentorshipMatching");
            }

            // Check if review already exists
            var existingReview = await _context.MentorReviews
                .FirstOrDefaultAsync(mr => mr.MentorshipMatchId == matchId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already submitted a review for this mentorship";
                return RedirectToAction("MenteeDashboard", "MentorshipMatching");
            }

            var viewModel = new MentorFeedbackViewModel
            {
                MatchId = matchId,
                MentorName = $"{match.Mentor.FirstName} {match.Mentor.LastName}",
                MentorPhoto = match.Mentor.Photo,
                MenteeName = $"{match.Mentee.FirstName} {match.Mentee.LastName}",
                MatchStartDate = match.StartDate ?? match.MatchedDate,
                MatchEndDate = match.EndDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Feedback(MentorFeedbackViewModel model)
        {
            var userId = GetCurrentUserId();
            var match = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .FirstOrDefaultAsync(mm => mm.Id == model.MatchId && mm.MenteeId == userId && mm.Status == "Completed");
            
            if (match == null)
            {
                TempData["Error"] = "Access denied or completed mentorship not found";
                return RedirectToAction("MenteeDashboard", "MentorshipMatching");
            }

            // Check if review already exists
            var existingReview = await _context.MentorReviews
                .FirstOrDefaultAsync(mr => mr.MentorshipMatchId == model.MatchId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already submitted a review for this mentorship";
                return RedirectToAction("MenteeDashboard", "MentorshipMatching");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate the view model with mentor info
                model.MentorName = $"{match.Mentor.FirstName} {match.Mentor.LastName}";
                model.MentorPhoto = match.Mentor.Photo;
                model.MenteeName = $"{match.Mentee.FirstName} {match.Mentee.LastName}";
                model.MatchStartDate = match.StartDate ?? match.MatchedDate;
                model.MatchEndDate = match.EndDate;
                return View(model);
            }

            var review = new MentorReview
            {
                MentorshipMatchId = model.MatchId,
                MentorId = match.MentorId,
                MenteeId = userId,
                Rating = model.Rating,
                WouldRecommend = model.WouldRecommend,
                Comments = model.Comments,
                Strengths = model.Strengths,
                AreasForImprovement = model.AreasForImprovement,

                CreatedAt = DateTime.Now
            };

            _context.MentorReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback! Your review has been submitted successfully.";
            return RedirectToAction("MenteeDashboard", "MentorshipMatching");
        }

        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            var userId = GetCurrentUserId();
            
            var reviews = await _context.MentorReviews
                .Include(mr => mr.Mentee)
                .Include(mr => mr.MentorshipMatch)
                .Where(mr => mr.MentorId == userId)
                .OrderByDescending(mr => mr.CreatedAt)
                .Select(mr => new MentorReviewDisplayViewModel
                {
                    Id = mr.Id,
                    MenteeName = $"{mr.Mentee.FirstName} {mr.Mentee.LastName}",
                    MenteePhoto = mr.Mentee.Photo,
                    Rating = mr.Rating,
                    WouldRecommend = mr.WouldRecommend,
                    Comments = mr.Comments,
                    Strengths = mr.Strengths,
                    AreasForImprovement = mr.AreasForImprovement,
                    CreatedAt = mr.CreatedAt,

                    MentorName = $"{mr.Mentor.FirstName} {mr.Mentor.LastName}",
                    MatchStartDate = mr.MentorshipMatch.StartDate ?? mr.MentorshipMatch.MatchedDate,
                    MatchEndDate = mr.MentorshipMatch.EndDate
                })
                .ToListAsync();

            return View(reviews);
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
