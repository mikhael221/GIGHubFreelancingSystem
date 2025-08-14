using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Freelancing.Controllers
{
    [Authorize(Roles = "Freelancer")]
    public class ManageProjectFreelancerController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public ManageProjectFreelancerController(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        // GET: ManageProjectFreelancer
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            // Get ongoing projects where freelancer's bid was accepted
            var ongoingProjects = await dbContext.Projects
                .Where(p => p.AcceptedBidId != null && 
                           p.Biddings.Any(b => b.UserId == userId && b.IsAccepted))
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
                var client = project.User;
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
                    
                    ClientId = client.Id,
                    ClientName = $"{client.FirstName} {client.LastName}",
                    ClientEmail = client.Email,
                    ClientPhoto = client.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                    
                    FreelancerId = freelancer.Id,
                    FreelancerName = $"{freelancer.FirstName} {freelancer.LastName}",
                    FreelancerEmail = freelancer.Email,
                    FreelancerPhoto = freelancer.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                    
                    AcceptedBidAmount = project.AcceptedBid.Budget,
                    AcceptedBidDelivery = project.AcceptedBid.Delivery,
                    AcceptedBidProposal = project.AcceptedBid.Proposal,
                    BiddingAcceptedDate = project.AcceptedBid.BiddingAcceptedDate,
                    
                    ProjectStatus = "Active",
                    ProjectRequiredSkills = project.ProjectSkills?.Select(ps => ps.UserSkill).ToList() ?? new List<UserSkill>(),
                    FreelancerSkills = freelancerSkills
                };

                ongoingProjectViewModels.Add(ongoingProjectViewModel);
            }

            var viewModel = new OngoingProjectListViewModel
            {
                OngoingProjects = ongoingProjectViewModels,
                TotalActiveProjects = ongoingProjectViewModels.Count,
                UserRole = "Freelancer"
            };

            return View(viewModel);
        }

        // GET: ManageProjectFreelancer/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized();
            }

            var project = await dbContext.Projects
                .Where(p => p.Id == id && p.AcceptedBidId != null && 
                           p.Biddings.Any(b => b.UserId == userId && b.IsAccepted))
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

            var client = project.User;
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
                
                ClientId = client.Id,
                ClientName = $"{client.FirstName} {client.LastName}",
                ClientEmail = client.Email,
                                    ClientPhoto = client.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                
                FreelancerId = freelancer.Id,
                FreelancerName = $"{freelancer.FirstName} {freelancer.LastName}",
                FreelancerEmail = freelancer.Email,
                                    FreelancerPhoto = freelancer.Photo ?? "https://ik.imagekit.io/6txj3mofs/GIGHub%20(11).png?updatedAt=1750552804497",
                
                AcceptedBidAmount = project.AcceptedBid.Budget,
                AcceptedBidDelivery = project.AcceptedBid.Delivery,
                AcceptedBidProposal = project.AcceptedBid.Proposal,
                BiddingAcceptedDate = project.AcceptedBid.BiddingAcceptedDate,
                
                ProjectStatus = "Active",
                ProjectRequiredSkills = project.ProjectSkills?.Select(ps => ps.UserSkill).ToList() ?? new List<UserSkill>(),
                FreelancerSkills = freelancerSkills
            };

            return View(viewModel);
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
