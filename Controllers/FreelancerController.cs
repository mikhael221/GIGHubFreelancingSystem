using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Freelancing.Data;
using Freelancing.Models;
using Freelancing.Models.Entities;
using System;

namespace Freelancing.Controllers
{
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
        public async Task<IActionResult> Dashboard()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var biddings = await dbContext.Biddings
                .Include(b => b.Project)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var model = new FreelancerDashboard
            {
                Biddings = biddings
            };

            return View(model);
        }

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
                TempData["BidError"] = "You have already placed a bid on this project.";
                return RedirectToAction("Project", new { id = project.Id });
            }

            var viewModel = new ViewProjectandBidding
            {
                Project = project,
                Bidding = new AddBidding()
            };
            return View(viewModel);
        }
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

            return RedirectToAction("Project", new { id = project.Id });
        }
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
            }
            else if (action == "delete")
            {
                dbContext.Biddings.Remove(bidding);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
    }
}
