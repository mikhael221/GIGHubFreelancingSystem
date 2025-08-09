using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models.Entities;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Sessions(Guid matchId)
        {
            var userId = GetCurrentUserId();
            var match = await _context.MentorshipMatches
                .FirstOrDefaultAsync(mm => mm.Id == matchId && (mm.MentorId == userId || mm.MenteeId == userId) && mm.Status == "Active");
            if (match == null)
            {
                TempData["Error"] = "Access denied or mentorship not active";
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
                .AnyAsync(mm => mm.Id == matchId && (mm.MentorId == userId || mm.MenteeId == userId) && mm.Status == "Active");
            if (!auth)
            {
                TempData["Error"] = "Access denied or mentorship not active";
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

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
