using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Freelancing.Services;
using System;
using Microsoft.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;

namespace Freelancing.Controllers
{
    // Handles freelancer-specific functionalities such as viewing projects, bidding on projects, and managing bids.
    [Authorize(Roles = "Freelancer")]
    public class FreelancerController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly INotificationService notificationService;
        
        public FreelancerController(ApplicationDbContext context, INotificationService notificationService)
        {
            this.dbContext = context;
            this.notificationService = notificationService;
        }

        // Helper method to generate unique filename while preserving original name
        private string GenerateUniqueFileName(string originalFileName, string uploadsFolder)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var fileExtension = Path.GetExtension(originalFileName);
            var uniqueFileName = originalFileName;
            var counter = 1;

            // Keep trying until we find a unique filename
            while (System.IO.File.Exists(Path.Combine(uploadsFolder, uniqueFileName)))
            {
                uniqueFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
                counter++;
            }

            return uniqueFileName;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Displays the freelancer dashboard with statistics and a list of biddings for the logged-in user.
        public async Task<IActionResult> Dashboard(Guid projectId)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            int totalAccepted = dbContext.Biddings.Count(p => p.UserId == userId && p.IsAccepted != false);
            ViewBag.TotalAccepted = totalAccepted;

            var biddings = await dbContext.Biddings
                .Include(b => b.Project)
                .ThenInclude(p => p.User)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var projects = await dbContext.Projects
                .Include(p => p.Biddings)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);


            var viewModel = new FreelancerDashboard
            {
                Biddings = biddings,
                Project = projects
            };

            return View(viewModel);
        }
        // Displays the feed of all projects available for bidding.
        [HttpGet]
        public async Task<IActionResult> Feed()
        {
            var project = await dbContext.Projects
                .Include(p => p.User)
                .Include(p => p.ProjectSkills)
                .ThenInclude(ps => ps.UserSkill)
                .ToListAsync();
            return View(project);
        }
        // Displays the details of a specific project, including its bids and the user who posted it.
        [HttpGet]
        public async Task<IActionResult> Project(Guid Id)
        {
            var projects = await dbContext.Projects
                .Include(p => p.Biddings)
                .ThenInclude(b => b.User)
                .Include(p => p.ProjectSkills)
                .ThenInclude(ps => ps.UserSkill)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (projects == null)
                return NotFound();
            return View(projects);
        }
        // Allows a freelancer to place a bid on a project. If the freelancer has already placed a bid, it redirects them with a message.
        [HttpGet]
        public async Task<IActionResult> Bid(Guid Id)
        {
            var project = await dbContext.Projects.FindAsync(Id);
            if (project == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            bool alreadyBid = await dbContext.Biddings.AnyAsync(b => b.UserId == userId && b.ProjectId == project.Id);
            if (alreadyBid)
            {
                TempData["Message"] = "You have already placed a bid on this project.";
                return RedirectToAction("Project", new { id = project.Id });
            }

            var viewModel = new ViewProjectandBidding
            {
                Project = project,
                Bidding = new AddBidding()
            };
            return View(viewModel);
        }
        // Handles the submission of a bid on a project.
        [HttpPost]
        public async Task<IActionResult> Bid(ViewProjectandBidding viewModel)
        {
            var project = await dbContext.Projects.FindAsync(viewModel.Project.Id);
            if (project == null)
                return NotFound();

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            bool alreadyBid = await dbContext.Biddings.AnyAsync(b => b.UserId == userId && b.ProjectId == project.Id);
            if (alreadyBid)
            {
                viewModel.Project = project;
                return View(viewModel);
            }

            // Handle file uploads for previous works
            var uploadedFilePaths = new List<string>();
            
            // Process newly uploaded files
            if (viewModel.Bidding.PreviousWorksFiles != null && viewModel.Bidding.PreviousWorksFiles.Any())
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".pdf", ".doc", ".docx", ".txt", ".zip", ".mp4", ".mov", ".avi" };
                const int maxFileSize = 10 * 1024 * 1024; // 10MB

                foreach (var file in viewModel.Bidding.PreviousWorksFiles)
                {
                    if (file.Length > 0)
                    {
                        // Validate file type
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("PreviousWorksFiles", $"File {file.FileName} is not a valid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
                            viewModel.Project = project;
                            return View(viewModel);
                        }

                        // Validate file size
                        if (file.Length > maxFileSize)
                        {
                            ModelState.AddModelError("PreviousWorksFiles", $"File {file.FileName} is too large. File size must be less than 10MB.");
                            viewModel.Project = project;
                            return View(viewModel);
                        }

                        // Generate unique filename while preserving original name
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "previous-works");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = GenerateUniqueFileName(file.FileName, uploadsFolder);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        uploadedFilePaths.Add($"/uploads/previous-works/{fileName}");
                    }
                }
            }
            


            var bidding = new Bidding
            {
                UserId = userId,
                ProjectId = project.Id,
                Budget = viewModel.Bidding.Budget,
                Delivery = viewModel.Bidding.Delivery,
                Proposal = viewModel.Bidding.Proposal,
                PreviousWorksPaths = uploadedFilePaths.Any() ? JsonSerializer.Serialize(uploadedFilePaths) : null,
                RepositoryLinks = !string.IsNullOrEmpty(viewModel.Bidding.RepositoryLinks) ? viewModel.Bidding.RepositoryLinks : null
            };

            dbContext.Biddings.Add(bidding);
            await dbContext.SaveChangesAsync();

            // Get the freelancer's information for the notification
            var freelancer = await dbContext.UserAccounts.FindAsync(userId);
            
            // Create notification for the project owner
            var notificationTitle = "New Bid Received";
            var notificationMessage = $"You received a new bid from {freelancer?.FirstName} {freelancer?.LastName} on your project '{project.ProjectName}'";
            var notificationIconSvg = "<svg fill=\"currentColor\" height=\"20px\" width=\"20px\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 512 512\"><g><g><path d=\"M475.542,203.546c-15.705-15.707-38.776-18.531-57.022-9.796L296.42,71.648c8.866-18.614,5.615-41.609-9.775-56.999 c-19.528-19.531-51.307-19.531-70.837,0L144.97,85.486c-19.529,19.529-19.529,51.307,0,70.836 c15.351,15.353,38.31,18.678,56.999,9.775l25.645,25.645L14.902,404.454c-19.575,19.574-19.578,51.259,0,70.836 c19.575,19.576,51.259,19.579,70.837,0l212.712-212.711l25.642,25.641c-8.868,18.615-5.617,41.609,9.774,57 c9.46,9.46,22.039,14.672,35.419,14.672s25.957-5.21,35.418-14.672l70.837-70.837 C495.072,254.853,495.072,223.077,475.542,203.546z M192.196,132.71c-6.51,6.509-17.103,6.507-23.613,0 c-6.509-6.511-6.509-17.102,0-23.612l70.837-70.837c6.509-6.509,17.1-6.512,23.612,0c6.51,6.51,6.51,17.102,0.001,23.612 L192.196,132.71z M62.127,451.676c-6.526,6.525-17.086,6.526-23.612,0c-6.525-6.525-6.526-17.087,0-23.612l212.712-212.712 l23.612,23.613L62.127,451.676z M227.614,144.516l11.805-11.807l35.419-35.419L392.9,215.353l-47.224,47.225L227.614,144.516z M451.931,250.772l-70.837,70.837c-6.526,6.526-17.086,6.526-23.612,0c-6.51-6.51-6.51-17.103,0-23.613l70.838-70.837 c6.524-6.526,17.086-6.525,23.611,0C458.457,233.684,458.457,244.245,451.931,250.772z\"></path></g></g><g><g><path d=\"M461.691,411.822H328.12c-27.619,0-50.089,22.47-50.089,50.089v33.393c0,9.221,7.476,16.696,16.696,16.696h200.357 c9.221,0,16.696-7.476,16.696-16.696v-33.393C511.781,434.292,489.311,411.822,461.691,411.822z M478.388,478.607H311.424v-16.696 c0-9.206,7.49-16.696,16.696-16.696h133.571c9.206,0,16.696,7.49,16.696,16.696V478.607z\"></path></g></g></g></svg>";
            var relatedUrl = $"/Client/ManageBid/{project.Id}";
            
            await notificationService.CreateNotificationAsync(
                project.UserId, 
                notificationTitle, 
                notificationMessage, 
                "bid", 
                notificationIconSvg, 
                relatedUrl
            );

            ModelState.Clear();
            ViewBag.Message = "Bidded successfully!";

            return RedirectToAction("Project", new { id = project.Id });
        }
        // Allows a freelancer to edit an existing bid on a project.
        [HttpGet]
        public async Task<IActionResult> EditBid(Guid id)
        {
            var bidding = await dbContext.Biddings
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bidding == null)
                return NotFound();

            var viewModel = new ViewProjectandBidding
            {
                Project = bidding.Project,
                Bidding = new AddBidding
                {
                    UserId = bidding.UserId,
                    ProjectId = bidding.ProjectId,
                    Budget = bidding.Budget,
                    Delivery = bidding.Delivery,
                    Proposal = bidding.Proposal,
                    PreviousWorksPaths = bidding.PreviousWorksPaths,
                    RepositoryLinks = bidding.RepositoryLinks
                }
            };
            return View(viewModel);
        }
        // Handles the submission of an edited bid on a project. It allows saving changes or deleting the bid.
        [HttpPost]
        public async Task<IActionResult> EditBid(Guid id, ViewProjectandBidding viewModel, string action, string removedFiles)
        {
            var bidding = await dbContext.Biddings.FindAsync(id);
            if (bidding == null)
                return NotFound();

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId) || bidding.UserId != userId)
                return Unauthorized();

            if (action == "save")
            {
                bidding.Budget = viewModel.Bidding.Budget;
                bidding.Delivery = viewModel.Bidding.Delivery;
                bidding.Proposal = viewModel.Bidding.Proposal;
                bidding.RepositoryLinks = !string.IsNullOrEmpty(viewModel.Bidding.RepositoryLinks) ? viewModel.Bidding.RepositoryLinks : null;

                // Handle file uploads for previous works
                var existingFilePaths = new List<string>();
                if (!string.IsNullOrEmpty(bidding.PreviousWorksPaths))
                {
                    try
                    {
                        existingFilePaths = JsonSerializer.Deserialize<List<string>>(bidding.PreviousWorksPaths) ?? new List<string>();
                    }
                    catch
                    {
                        existingFilePaths = new List<string>();
                    }
                }

                // Handle file removal
                var filesToRemove = new List<string>();
                if (!string.IsNullOrEmpty(removedFiles))
                {
                    try
                    {
                        filesToRemove = JsonSerializer.Deserialize<List<string>>(removedFiles) ?? new List<string>();
                    }
                    catch
                    {
                        filesToRemove = new List<string>();
                    }
                }

                // Remove files from existing paths and delete from disk
                foreach (var filePath in filesToRemove)
                {
                    if (existingFilePaths.Contains(filePath))
                    {
                        existingFilePaths.Remove(filePath);
                        
                        // Delete the physical file
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            try
                            {
                                System.IO.File.Delete(fullPath);
                            }
                            catch (Exception ex)
                            {
                                // Log the error but don't fail the operation
                                Console.WriteLine($"Error deleting file {fullPath}: {ex.Message}");
                            }
                        }
                    }
                }

                var uploadedFilePaths = new List<string>();
                
                if (viewModel.Bidding.PreviousWorksFiles != null && viewModel.Bidding.PreviousWorksFiles.Any())
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".pdf", ".doc", ".docx", ".txt", ".zip", ".mp4", ".mov", ".avi" };
                    const int maxFileSize = 10 * 1024 * 1024; // 10MB

                    foreach (var file in viewModel.Bidding.PreviousWorksFiles)
                    {
                        if (file.Length > 0)
                        {
                            // Validate file type
                            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("PreviousWorksFiles", $"File {file.FileName} is not a valid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
                                viewModel.Project = bidding.Project;
                                return View(viewModel);
                            }

                            // Validate file size
                            if (file.Length > maxFileSize)
                            {
                                ModelState.AddModelError("PreviousWorksFiles", $"File {file.FileName} is too large. File size must be less than 10MB.");
                                viewModel.Project = bidding.Project;
                                return View(viewModel);
                            }

                            // Generate unique filename while preserving original name
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "previous-works");

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var fileName = GenerateUniqueFileName(file.FileName, uploadsFolder);
                            var filePath = Path.Combine(uploadsFolder, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            uploadedFilePaths.Add($"/uploads/previous-works/{fileName}");
                        }
                    }
                }

                // Combine existing and new file paths
                var allFilePaths = existingFilePaths.Concat(uploadedFilePaths).ToList();
                bidding.PreviousWorksPaths = allFilePaths.Any() ? JsonSerializer.Serialize(allFilePaths) : null;

                await dbContext.SaveChangesAsync();
                ViewBag.Message = "Bid edited successfully!";

                var updatedBidding = await dbContext.Biddings
                    .Include(b => b.Project)
                    .FirstOrDefaultAsync(b => b.Id == id);

                viewModel.Project = updatedBidding.Project;
                viewModel.Bidding.PreviousWorksPaths = updatedBidding.PreviousWorksPaths;
                viewModel.Bidding.RepositoryLinks = updatedBidding.RepositoryLinks;
                return View(viewModel);
            }
            else if (action == "delete")
            {
                // Delete all associated files before removing the bidding
                if (!string.IsNullOrEmpty(bidding.PreviousWorksPaths))
                {
                    try
                    {
                        var filePaths = JsonSerializer.Deserialize<List<string>>(bidding.PreviousWorksPaths) ?? new List<string>();
                        foreach (var filePath in filePaths)
                        {
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error deleting file {fullPath}: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continue with deletion even if file cleanup fails
                    }
                }

                dbContext.Biddings.Remove(bidding);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", "Freelancer");
        }
        [HttpGet]
        public async Task<IActionResult> EditAccount()
        {
            // Get the user ID from the claims
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // Fetch the user account details from the database
            var userAccount = await dbContext.UserAccounts.FindAsync(userId);
            if (userAccount == null)
                return NotFound();

            var savedSkills = await dbContext.UserAccountSkills
                .Where(uas => uas.UserAccountId == userId)
                .Include(uas => uas.UserSkill)
                .Select(uas => uas.UserSkill)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var user = await dbContext.UserAccounts
                .FirstOrDefaultAsync(u => u.Id == userId);

            // Create view model
            var viewModel = new EditAccount
            {
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                Email = userAccount.Email,
                UserName = userAccount.UserName,
                Photo = userAccount.Photo,
                SavedSkills = savedSkills,
                TotalSkillsCount = savedSkills.Count
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(EditAccount viewModel, IFormFile? PhotoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Get user ID
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // Fetch user account
            var userAccount = await dbContext.UserAccounts.FindAsync(userId);
            if (userAccount == null)
                return NotFound();

            // Check for existing username/email
            var existingUserWithUsername = await dbContext.UserAccounts
                .FirstOrDefaultAsync(u => u.UserName == viewModel.UserName && u.Id != userId);
            var existingUserWithEmail = await dbContext.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == viewModel.Email && u.Id != userId);

            if (existingUserWithEmail != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(viewModel);
            }

            if (existingUserWithUsername != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return View(viewModel);
            }

            // Track if any changes were made
            bool hasChanges = false;
            bool nameChanged = false;
            bool photoChanged = false;

            // Check and update user account fields only if they changed
            if (userAccount.FirstName != viewModel.FirstName)
            {
                userAccount.FirstName = viewModel.FirstName;
                hasChanges = true;
                nameChanged = true;
            }

            if (userAccount.LastName != viewModel.LastName)
            {
                userAccount.LastName = viewModel.LastName;
                hasChanges = true;
                nameChanged = true;
            }

            if (userAccount.Email != viewModel.Email)
            {
                userAccount.Email = viewModel.Email;
                hasChanges = true;
            }

            if (userAccount.UserName != viewModel.UserName)
            {
                userAccount.UserName = viewModel.UserName;
                hasChanges = true;
            }

            // Handle photo upload
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                // Validate the uploaded file type and size
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(PhotoFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PhotoFile", "Please upload a valid image file (jpg, jpeg, png, gif).");
                    return View(viewModel);
                }

                if (PhotoFile.Length > 10 * 1024 * 1024) // 10 MB limit
                {
                    ModelState.AddModelError("PhotoFile", "The image file size should not exceed 10 MB.");
                    return View(viewModel);
                }

                // Generate a unique file name while preserving original name
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = GenerateUniqueFileName(PhotoFile.FileName, uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Delete old photo if exists
                if (!string.IsNullOrEmpty(userAccount.Photo))
                {
                    var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", userAccount.Photo.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                        catch
                        {
                        }
                    }
                }

                // Save the uploaded photo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PhotoFile.CopyToAsync(stream);
                }

                userAccount.Photo = $"/uploads/profiles/{fileName}";
                viewModel.Photo = userAccount.Photo;
                hasChanges = true;
                photoChanged = true;
            }

            // Only save if there were actual changes
            if (hasChanges)
            {
                dbContext.Entry(userAccount).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await dbContext.SaveChangesAsync();

                // Refresh the authentication cookie with updated claims if name, email, username, or photo changed
                if (nameChanged || photoChanged)
                {
                    await RefreshUserClaims(userAccount);
                }

                ViewBag.Message = "Account updated successfully!";
            }
            else
            {
                ViewBag.Message = "No changes were detected.";
            }

            var finalViewModel = await PopulateEditAccountViewModel(userId, userAccount);
            return View(finalViewModel);
        }
        private async Task<EditAccount> PopulateEditAccountViewModel(Guid userId, UserAccount userAccount)
        {
            var savedSkills = await dbContext.UserAccountSkills
                .Where(uas => uas.UserAccountId == userId)
                .Include(uas => uas.UserSkill)
                .Select(uas => uas.UserSkill)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return new EditAccount
            {
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                Email = userAccount.Email,
                UserName = userAccount.UserName,
                Photo = userAccount.Photo,
                SavedSkills = savedSkills,
                TotalSkillsCount = savedSkills.Count
            };
        }

        // Updated method to refresh claims
        private async Task RefreshUserClaims(UserAccount userAccount)
        {
            var identity = (ClaimsIdentity)User.Identity;

            // Update FullName claim
            var existingFullNameClaim = identity.FindFirst("FullName");
            if (existingFullNameClaim != null)
            {
                identity.RemoveClaim(existingFullNameClaim);
            }
            var fullName = $"{userAccount.FirstName} {userAccount.LastName}";
            identity.AddClaim(new Claim("FullName", fullName));

            // Update Email claim
            var existingEmailClaim = identity.FindFirst(ClaimTypes.Email);
            if (existingEmailClaim != null)
            {
                identity.RemoveClaim(existingEmailClaim);
            }
            identity.AddClaim(new Claim(ClaimTypes.Email, userAccount.Email));

            // Update Username claim
            var existingUsernameClaim = identity.FindFirst(ClaimTypes.Name);
            if (existingUsernameClaim != null)
            {
                identity.RemoveClaim(existingUsernameClaim);
            }
            identity.AddClaim(new Claim(ClaimTypes.Name, userAccount.UserName));

            // Update Photo claim
            var existingPhotoClaim = identity.FindFirst("Photo");
            if (existingPhotoClaim != null)
            {
                identity.RemoveClaim(existingPhotoClaim);
            }
            identity.AddClaim(new Claim("Photo", userAccount.Photo ?? string.Empty));

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
