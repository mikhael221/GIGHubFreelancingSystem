using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Freelancing.Controllers
{
    // Handles client-specific functionalities such as managing projects, viewing bids, and accepting bids.
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public ClientController(ApplicationDbContext context)
        {
            this.dbContext = context;
        }
        // Displays the client dashboard with project statistics and a list of projects.
        public async Task<IActionResult> Dashboard()
        {
            // Get the user ID from the claims to filter projects by the logged-in user.
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();
            // Fetch projects associated with the logged-in user, including accepted bids.
            var projects = await dbContext.Projects
                .Include(p => p.AcceptedBid)
                .Where(p => p.UserId == userId)
                .ToListAsync();
            // Create a view model to hold project statistics and the list of projects.
            var viewModel = new ClientDashboard
            {
                Projects = projects,
                TotalProjects = projects.Count,
                OpenProjects = projects.Count(p => !p.AcceptedBidId.HasValue),
                ClosedProjects = projects.Count(p => p.AcceptedBidId.HasValue)
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
        // Displays the form to create a new project.
        [HttpGet]
        public IActionResult Post()
        {
            return View();
        }
        // Handles the submission of the project creation form, validating the input and saving the project to the database.
        [HttpPost]
        public async Task<IActionResult> Post(AddProject viewModel)
        {
            if (ModelState.IsValid)
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    var project = new Project
                    {
                        UserId = userId,
                        ProjectName = viewModel.ProjectName,
                        ProjectDescription = viewModel.ProjectDescription,
                        Budget = viewModel.Budget,
                        Category = viewModel.Category,
                    };
                    await dbContext.Projects.AddAsync(project);
                    await dbContext.SaveChangesAsync();

                    ModelState.Clear();
                    ViewBag.Message = "Project posted successfully!";

                    return View(new AddProject());
                }
                else
                {
                    ModelState.AddModelError("", "Unable to determine the logged-in user.");
                }
            }
            return View(viewModel);
        }
        // Displays the form to edit an existing project.
        [HttpGet]
        public async Task<IActionResult> EditPost(Guid Id)
        {
            var project = await dbContext.Projects.FindAsync(Id);

            return View(project);
        }
        // Handles the submission of the project editing form, allowing users to save changes or delete the project.
        [HttpPost]
        public async Task<IActionResult> EditPost(Project viewModel, string action)
        {
            var project = await dbContext.Projects.FindAsync(viewModel.Id);

            if (project == null)
                return RedirectToAction("Dashboard", "Client");

            if (action == "save")
            {
                project.ProjectName = viewModel.ProjectName;
                project.ProjectDescription = viewModel.ProjectDescription;
                project.Budget = viewModel.Budget;
                project.Category = viewModel.Category;

                await dbContext.SaveChangesAsync();

                ModelState.Clear();
                ViewBag.Message = "Project edited successfully!";
            }
            else if (action == "delete")
            {
                dbContext.Projects.Remove(project);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("Dashboard", "Client");
            }

            return View(project);
        }
        // Displays the bids for a specific project and allows the client to manage them.
        [HttpGet]
        public async Task<IActionResult> ManageBid(Guid Id)
        {
            var projects = await dbContext.Projects
                .Include(p => p.Biddings)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (projects == null)
                return NotFound();
            return View(projects);
        }
        // Accepts a bid for a specific project, marking it as the accepted bid and updating the project accordingly.
        [HttpPost]
        public async Task<IActionResult> AcceptBid(Guid projectId, Guid bidId)
        {
            var project = await dbContext.Projects
                .Include(p => p.Biddings)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                return NotFound();

            var bidToAccept = project.Biddings.FirstOrDefault(b => b.Id == bidId);
            if (bidToAccept == null)
                return BadRequest("Invalid bid ID");

            foreach (var bid in project.Biddings)
            {
                bid.IsAccepted = (bid.Id == bidId);
            }

            project.AcceptedBidId = bidId;

            await dbContext.SaveChangesAsync();
            return RedirectToAction("ManageBid", new { id = projectId });
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

            // Create view model
            var viewModel = new EditAccount
            {
                FirstName = userAccount.FirstName,
                LastName = userAccount.LastName,
                Email = userAccount.Email,
                UserName = userAccount.UserName,
                Photo = userAccount.Photo
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
