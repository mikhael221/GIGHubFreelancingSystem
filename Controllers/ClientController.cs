using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            var project = await dbContext.Projects.ToListAsync();
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

    }
}
