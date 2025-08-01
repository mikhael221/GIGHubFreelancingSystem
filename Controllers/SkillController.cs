using Freelancing.Data;
using Freelancing.Models.Entities;
using Freelancing.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Freelancing.Controllers
{
    public class SkillController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public SkillController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> EditSkills(string searchTerm)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            // Get all available skills based on search term
            var availableSkills = await dbContext.UserSkills
                .Where(s => string.IsNullOrEmpty(searchTerm) || s.Name.Contains(searchTerm))
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Get currently selected skills for the user
            var selectedSkillIds = await dbContext.UserAccountSkills
                .Where(uas => uas.UserAccountId == userId)
                .Select(uas => uas.UserSkillId)
                .ToListAsync();

            var viewModel = new EditSkills
            {
                UserSkills = availableSkills,
                SelectedSkillIds = selectedSkillIds,
                SearchTerm = searchTerm ?? string.Empty
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditSkills(EditSkills viewModel)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            try
            {
                // Remove existing skills for the user
                var existingUserSkills = await dbContext.UserAccountSkills
                    .Where(uas => uas.UserAccountId == userId)
                    .ToListAsync();

                dbContext.UserAccountSkills.RemoveRange(existingUserSkills);

                // Add selected skills
                if (viewModel.SelectedSkillIds != null && viewModel.SelectedSkillIds.Any())
                {
                    var newUserSkills = viewModel.SelectedSkillIds.Select(skillId => new UserAccountSkill
                    {
                        UserAccountId = userId,
                        UserSkillId = skillId
                    }).ToList();

                    await dbContext.UserAccountSkills.AddRangeAsync(newUserSkills);
                }

                await dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Skills updated successfully!";
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while updating skills.";
                // You might want to log the exception here
            }

            return RedirectToAction("EditSkills");
        }
    }
}
