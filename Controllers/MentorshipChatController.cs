using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Freelancing.Data;
using Freelancing.Models.Entities;
using Freelancing.Models;

namespace Freelancing.Controllers
{
    [Authorize]
    public class MentorshipChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] AllowedFileTypes = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".avi" };

        public MentorshipChatController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid matchId)
        {
            var userId = GetCurrentUserId();

            // Verify user has access to this mentorship match
            var match = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .FirstOrDefaultAsync(mm => mm.Id == matchId &&
                                          (mm.MentorId == userId || mm.MenteeId == userId) &&
                                          mm.Status == "Active");

            if (match == null)
            {
                TempData["Error"] = "Access denied or mentorship not found";
                return RedirectToAction("AvailableMentors", "MentorshipMatching");
            }

            // Get chat messages
            var messages = await _context.MentorshipChatMessages
                .Where(mcm => mcm.MentorshipMatchId == matchId)
                .Include(mcm => mcm.Sender)
                .OrderBy(mcm => mcm.SentAt)
                .Select(mcm => new ChatMessageViewModel
                {
                    Id = mcm.Id,
                    SenderId = mcm.SenderId,
                    SenderName = $"{mcm.Sender.FirstName} {mcm.Sender.LastName}",
                    Message = mcm.Message,
                    MessageType = mcm.MessageType,
                    FileUrl = mcm.FileUrl,
                    FileType = mcm.FileType,
                    FileSize = mcm.FileSize,
                    SentAt = mcm.SentAt,
                    IsRead = mcm.IsRead,
                    IsCurrentUser = mcm.SenderId == userId
                })
                .ToListAsync();

            var partner = match.MentorId == userId ? match.Mentee : match.Mentor;

            var viewModel = new MentorshipChatViewModel
            {
                MatchId = matchId,
                Partner = partner,
                CurrentUserId = userId,
                Messages = messages,
                IsCurrentUserMentor = match.MentorId == userId
            };

            // Mark messages as read
            await MarkMessagesAsRead(matchId, userId);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(Guid matchId, IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Verify access
                var hasAccess = await _context.MentorshipMatches
                    .AnyAsync(mm => mm.Id == matchId &&
                                   (mm.MentorId == userId || mm.MenteeId == userId) &&
                                   mm.Status == "Active");

                if (!hasAccess)
                {
                    return Json(new { success = false, message = "Access denied" });
                }

                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "No file selected" });
                }

                if (file.Length > MaxFileSize)
                {
                    return Json(new { success = false, message = $"File size exceeds {MaxFileSize / 1024 / 1024}MB limit" });
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedFileTypes.Contains(extension))
                {
                    return Json(new
                    {
                        success = false,
                        message = $"File type '{extension}' not allowed. Allowed types: {string.Join(", ", AllowedFileTypes)}"
                    });
                }

                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "mentorship-chat");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique filename to prevent conflicts
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create file URL (make sure this matches your app's URL structure)
                var fileUrl = $"/uploads/mentorship-chat/{uniqueFileName}";

                // Log successful upload
                Console.WriteLine($"File uploaded successfully: {file.FileName} -> {fileUrl}");

                return Json(new
                {
                    success = true,
                    fileName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length,
                    fileType = file.ContentType ?? "application/octet-stream"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Upload failed: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            var userId = GetCurrentUserId();

            var message = await _context.MentorshipChatMessages
                .Include(mcm => mcm.MentorshipMatch)
                .FirstOrDefaultAsync(mcm => mcm.Id == messageId && mcm.SenderId == userId);

            if (message == null)
            {
                return Json(new { success = false, message = "Message not found or access denied" });
            }

            // Soft delete
            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(Guid matchId, int page = 1, int pageSize = 50)
        {
            var userId = GetCurrentUserId();

            // Verify access
            var hasAccess = await _context.MentorshipMatches
                .AnyAsync(mm => mm.Id == matchId &&
                               (mm.MentorId == userId || mm.MenteeId == userId) &&
                               mm.Status == "Active");

            if (!hasAccess)
            {
                return Json(new { success = false, message = "Access denied" });
            }

            var messages = await _context.MentorshipChatMessages
                .Where(mcm => mcm.MentorshipMatchId == matchId && !mcm.IsDeleted)
                .Include(mcm => mcm.Sender)
                .OrderByDescending(mcm => mcm.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(mcm => new
                {
                    Id = mcm.Id,
                    SenderId = mcm.SenderId,
                    SenderName = $"{mcm.Sender.FirstName} {mcm.Sender.LastName}",
                    Message = mcm.Message,
                    MessageType = mcm.MessageType,
                    FileUrl = mcm.FileUrl,
                    FileType = mcm.FileType,
                    FileSize = mcm.FileSize,
                    SentAt = mcm.SentAt,
                    IsRead = mcm.IsRead,
                    IsCurrentUser = mcm.SenderId == userId
                })
                .ToListAsync();

            return Json(new { success = true, messages = messages.OrderBy(m => m.SentAt) });
        }

        [HttpGet]
        public async Task<IActionResult> VideoCall(Guid matchId)
        {
            var userId = GetCurrentUserId();

            var match = await _context.MentorshipMatches
                .Include(mm => mm.Mentor)
                .Include(mm => mm.Mentee)
                .FirstOrDefaultAsync(mm => mm.Id == matchId &&
                                          (mm.MentorId == userId || mm.MenteeId == userId) &&
                                          mm.Status == "Active");

            if (match == null)
            {
                TempData["Error"] = "Access denied or mentorship not found";
                return RedirectToAction("Index", new { matchId });
            }

            var partner = match.MentorId == userId ? match.Mentee : match.Mentor;

            var viewModel = new VideoCallViewModel
            {
                MatchId = matchId,
                Partner = partner,
                CurrentUserId = userId,
                IsCurrentUserMentor = match.MentorId == userId
            };

            return View(viewModel);
        }

        private async Task MarkMessagesAsRead(Guid matchId, Guid userId)
        {
            var unreadMessages = await _context.MentorshipChatMessages
                .Where(mcm => mcm.MentorshipMatchId == matchId &&
                             mcm.SenderId != userId && !mcm.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}