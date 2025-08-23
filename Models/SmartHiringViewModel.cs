using Freelancing.Models.Entities;
using Freelancing.Services;

namespace Freelancing.Models
{
    public class SmartHiringViewModel
    {
        public Project Project { get; set; } = new();
        public List<SmartHiringPrediction> Recommendations { get; set; } = new();
        public List<SmartHiringPrediction> TopRecommendations { get; set; } = new();
        public SmartHiringInsights ProjectInsights { get; set; } = new();
        
        // Additional properties for the view
        public string ProjectRequiredSkillsJson => 
            System.Text.Json.JsonSerializer.Serialize(Project.ProjectSkills?.Select(ps => ps.UserSkill.Name).ToList() ?? new List<string>());
    }

    public class BidderDetailsViewModel
    {
        public Project Project { get; set; } = new();
        public Bidding Bid { get; set; } = new();
        public UserAccount Freelancer { get; set; } = new();
        public SmartHiringPrediction Prediction { get; set; } = new();
        public List<FreelancerFeedback> PastFeedbacks { get; set; } = new();
        public List<UserSkill> FreelancerSkills { get; set; } = new();
        public int CompletedProjectsCount { get; set; }
        
        // Calculated properties
        public float AverageRating => PastFeedbacks.Any() ? (float)PastFeedbacks.Average(f => f.Rating) : 0;
        public float RecommendationRate => PastFeedbacks.Any() ? 
            (float)PastFeedbacks.Count(f => f.WouldRecommend) / PastFeedbacks.Count : 0;
        
        public List<UserSkill> MatchingSkills => 
            Project.ProjectSkills?.Where(ps => FreelancerSkills.Any(fs => fs.Id == ps.UserSkillId))
                .Select(ps => ps.UserSkill).ToList() ?? new List<UserSkill>();
                
        public List<UserSkill> MissingSkills => 
            Project.ProjectSkills?.Where(ps => !FreelancerSkills.Any(fs => fs.Id == ps.UserSkillId))
                .Select(ps => ps.UserSkill).ToList() ?? new List<UserSkill>();
    }

    public class SmartHiringInsightsSummary
    {
        public int TotalProjects { get; set; }
        public int ProjectsWithRecommendations { get; set; }
        public float AverageMatchScore { get; set; }
        public int TotalFreelancersAnalyzed { get; set; }
        public string MostRecommendedSkill { get; set; } = string.Empty;
        public string TopPerformingCategory { get; set; } = string.Empty;
    }
}



