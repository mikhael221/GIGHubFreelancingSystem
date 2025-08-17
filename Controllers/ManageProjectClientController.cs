using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using Freelancing.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Freelancing.Controllers
{
    [Authorize(Roles = "Client")]
    public class ManageProjectClientController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IContractService contractService;

        public ManageProjectClientController(ApplicationDbContext context, IContractService contractService)
        {
            this.dbContext = context;
            this.contractService = contractService;
        }

        // GET: ManageProjectClient
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            // Get ongoing projects (projects with accepted bids)
            var ongoingProjects = await dbContext.Projects
                .Where(p => p.UserId == userId && p.AcceptedBidId != null)
                .Include(p => p.User) // Client
                .Include(p => p.AcceptedBid)
                    .ThenInclude(ab => ab.User) // Freelancer
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.UserSkill)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var ongoingProjectViewModels = new List<OngoingProjectViewModel>();

            foreach (var project in ongoingProjects)
            {
                var freelancer = project.AcceptedBid.User;
                
                // Get freelancer skills
                var freelancerSkills = await dbContext.UserAccountSkills
                    .Where(uas => uas.UserAccountId == freelancer.Id)
                    .Include(uas => uas.UserSkill)
                    .Select(uas => uas.UserSkill)
                    .ToListAsync();

                var ongoingProjectViewModel = new OngoingProjectViewModel
                {
                    ProjectId = project.Id,
                    ProjectName = project.ProjectName,
                    ProjectDescription = project.ProjectDescription,
                    ProjectBudget = project.Budget,
                    ProjectCategory = project.Category,
                    ProjectCreatedAt = project.CreatedAt,
                    ProjectImageUrls = ParseJsonArray(project.ImagePaths),
                    
                    ClientId = project.User.Id,
                    ClientName = $"{project.User.FirstName} {project.User.LastName}",
                    ClientEmail = project.User.Email,
                    ClientPhoto = project.User.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                    
                    FreelancerId = freelancer.Id,
                    FreelancerName = $"{freelancer.FirstName} {freelancer.LastName}",
                    FreelancerEmail = freelancer.Email,
                    FreelancerPhoto = freelancer.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                    
                    AcceptedBidAmount = project.AcceptedBid.Budget,
                    AcceptedBidDelivery = project.AcceptedBid.Delivery,
                                    AcceptedBidProposal = project.AcceptedBid.Proposal,
                BiddingAcceptedDate = project.AcceptedBid.BiddingAcceptedDate,
                
                ProjectStatus = project.AcceptedBidId.HasValue ? "Active" : "Open",
                    ProjectRequiredSkills = project.ProjectSkills?.Select(ps => ps.UserSkill).ToList() ?? new List<UserSkill>(),
                    FreelancerSkills = freelancerSkills
                };

                ongoingProjectViewModels.Add(ongoingProjectViewModel);
            }

            var viewModel = new OngoingProjectListViewModel
            {
                OngoingProjects = ongoingProjectViewModels,
                TotalActiveProjects = ongoingProjectViewModels.Count,
                UserRole = "Client"
            };

            return View(viewModel);
        }

        // GET: ManageProjectClient/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var project = await dbContext.Projects
                .Where(p => p.Id == id && p.UserId == userId && p.AcceptedBidId != null)
                .Include(p => p.User)
                .Include(p => p.AcceptedBid)
                    .ThenInclude(ab => ab.User)
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.UserSkill)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            var freelancer = project.AcceptedBid.User;
            
            // Get freelancer skills
            var freelancerSkills = await dbContext.UserAccountSkills
                .Where(uas => uas.UserAccountId == freelancer.Id)
                .Include(uas => uas.UserSkill)
                .Select(uas => uas.UserSkill)
                .ToListAsync();

            var viewModel = new OngoingProjectViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.ProjectName,
                ProjectDescription = project.ProjectDescription,
                ProjectBudget = project.Budget,
                ProjectCategory = project.Category,
                ProjectCreatedAt = project.CreatedAt,
                ProjectImageUrls = ParseJsonArray(project.ImagePaths),
                
                ClientId = project.User.Id,
                ClientName = $"{project.User.FirstName} {project.User.LastName}",
                ClientEmail = project.User.Email,
                ClientUsername = project.User.UserName,
                ClientPhoto = project.User.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                
                FreelancerId = freelancer.Id,
                FreelancerName = $"{freelancer.FirstName} {freelancer.LastName}",
                FreelancerEmail = freelancer.Email,
                FreelancerUsername = freelancer.UserName,
                FreelancerPhoto = freelancer.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                
                AcceptedBidAmount = project.AcceptedBid.Budget,
                AcceptedBidDelivery = project.AcceptedBid.Delivery,
                AcceptedBidProposal = project.AcceptedBid.Proposal,
                BiddingAcceptedDate = project.AcceptedBid.BiddingAcceptedDate,
                
                ProjectStatus = project.Status,
                ProjectRequiredSkills = project.ProjectSkills?.Select(ps => ps.UserSkill).ToList() ?? new List<UserSkill>(),
                FreelancerSkills = freelancerSkills
            };

            // Get contract information
            var contract = await contractService.GetContractByProjectIdAsync(id);
            if (contract != null)
            {
                ViewBag.ContractId = contract.Id;
                ViewBag.ContractStatus = contract.Status;
                
                // Check if current user (client) needs to sign
                var needsSignature = !contract.ClientSignedAt.HasValue && 
                                   (contract.Status == "Draft" || contract.Status == "AwaitingClient");
                ViewBag.NeedsSignature = needsSignature;
                
                // Add status information for better UX
                ViewBag.ContractCreatedAt = contract.CreatedAt;
                ViewBag.ClientHasSigned = contract.ClientSignedAt.HasValue;
                ViewBag.FreelancerHasSigned = contract.FreelancerSignedAt.HasValue;
            }
            else
            {
                ViewBag.ContractId = null;
                ViewBag.ContractStatus = null;
                ViewBag.NeedsSignature = false;
                ViewBag.ClientHasSigned = false;
                ViewBag.FreelancerHasSigned = false;
            }

            // Pass through any success/error messages from contract operations
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(viewModel);
        }

        // POST: ManageProjectClient/MarkAsCompleted/5
        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var project = await dbContext.Projects
                .Where(p => p.Id == id && p.UserId == userId && p.AcceptedBidId != null)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            project.Status = "Completed";
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: ManageProjectFreelancer/MarkAsCompleted/5
        [HttpPost]
        public async Task<IActionResult> MarkAsCompletedFreelancer(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var project = await dbContext.Projects
                .Where(p => p.Id == id && p.AcceptedBidId != null && 
                           p.Biddings.Any(b => b.UserId == userId && b.IsAccepted))
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            project.Status = "Completed";
            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helper method to parse JSON arrays stored as strings
        private List<string> ParseJsonArray(string? jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
