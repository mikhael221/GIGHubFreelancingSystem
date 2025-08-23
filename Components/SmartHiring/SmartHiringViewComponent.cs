using Microsoft.AspNetCore.Mvc;
using Freelancing.Services;
using Freelancing.Models;

namespace Freelancing.Components.SmartHiring
{
    public class SmartHiringViewComponent : ViewComponent
    {
        private readonly ISmartHiringService _smartHiringService;

        public SmartHiringViewComponent(ISmartHiringService smartHiringService)
        {
            _smartHiringService = smartHiringService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid projectId, bool showTopOnly = false)
        {
            try
            {
                var recommendations = await _smartHiringService.GetBestFreelancersAsync(projectId);
                var insights = await _smartHiringService.GetProjectInsightsAsync(projectId);

                var model = new SmartHiringComponentViewModel
                {
                    ProjectId = projectId,
                    Recommendations = showTopOnly ? recommendations.Take(3).ToList() : recommendations,
                    ProjectInsights = insights,
                    ShowTopOnly = showTopOnly
                };

                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }
    }

    public class SmartHiringComponentViewModel
    {
        public Guid ProjectId { get; set; }
        public List<SmartHiringPrediction> Recommendations { get; set; } = new();
        public SmartHiringInsights ProjectInsights { get; set; } = new();
        public bool ShowTopOnly { get; set; }
    }
}



