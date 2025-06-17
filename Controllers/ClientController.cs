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
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public ClientController(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        public IActionResult Dashboard()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                int totalProjects = dbContext.Projects.Count(p => p.UserId == userId);
                ViewBag.TotalProjects = totalProjects;

                var projects = dbContext.Projects
                    .Where(p => p.UserId == userId)
                    .ToList();

                var model = new AddProject
                {
                    Projects = projects
                };
                return View(model);
            }
            return View(new AddProject());
        }
        [HttpGet]
        public async Task <IActionResult> Feed()
        {
            var project = await dbContext.Projects.ToListAsync();
            return View(project);
        }
        [HttpGet]
        public IActionResult Post()
        {
            return View();
        }
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
                    ViewBag.Message = "Project added successfully!";

                    return View(new AddProject());
                }
                else
                {
                    ModelState.AddModelError("", "Unable to determine the logged-in user.");
                }
            }
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> EditPost(Guid Id)
        {
            var project = await dbContext.Projects.FindAsync(Id);

            return View(project);
        }
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
            }
            else if (action == "delete")
            {
                dbContext.Projects.Remove(project);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard", "Client");
        }
    }
}
