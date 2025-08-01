using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using System;
using Microsoft.CodeAnalysis;
using System.Security.Claims;

namespace Freelancing.Controllers
{
    // Handles freelancer-specific functionalities such as viewing projects, bidding on projects, and managing bids.
    [Authorize(Roles = "Freelancer")]
    public class FreelancerController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public FreelancerController(ApplicationDbContext context)
        {
            this.dbContext = context;
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

            var bidding = new Bidding
            {
                UserId = userId,
                ProjectId = project.Id,
                Budget = viewModel.Bidding.Budget,
                Delivery = viewModel.Bidding.Delivery,
                Proposal = viewModel.Bidding.Proposal,
            };

            dbContext.Biddings.Add(bidding);
            await dbContext.SaveChangesAsync();

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
                    Proposal = bidding.Proposal
                }
            };
            return View(viewModel);
        }
        // Handles the submission of an edited bid on a project. It allows saving changes or deleting the bid.
        [HttpPost]
        public async Task<IActionResult> EditBid(Guid id, ViewProjectandBidding viewModel, string action)
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

                await dbContext.SaveChangesAsync();
                ViewBag.Message = "Bid edited successfully!";

                var updatedBidding = await dbContext.Biddings
                    .Include(b => b.Project)
                    .FirstOrDefaultAsync(b => b.Id == id);

                viewModel.Project = updatedBidding.Project;
                return View(viewModel);
            }
            else if (action == "delete")
            {
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

                // Generate a unique file name and save the uploaded photo
                var fileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

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

            return View(viewModel);
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
