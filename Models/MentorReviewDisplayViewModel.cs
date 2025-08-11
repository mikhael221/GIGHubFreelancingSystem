using System.ComponentModel.DataAnnotations;

namespace Freelancing.Models
{
    public class MentorReviewDisplayViewModel
    {
        public Guid Id { get; set; }
        public string MenteeName { get; set; } = string.Empty;
        public string? MenteePhoto { get; set; }
        public int Rating { get; set; }
        public bool WouldRecommend { get; set; }
        public string? Comments { get; set; }
        public string? Strengths { get; set; }
        public string? AreasForImprovement { get; set; }
        public DateTime CreatedAt { get; set; }

        public string MentorName { get; set; } = string.Empty;
        public DateTime MatchStartDate { get; set; }
        public DateTime? MatchEndDate { get; set; }

        // Helper properties for display
        public string DisplayName => MenteeName;
        public string RatingText => Rating switch
        {
            1 => "Poor - 1/5",
            2 => "Fair - 2/5",
            3 => "Good - 3/5",
            4 => "Very Good - 4/5",
            5 => "Excellent - 5/5",
            _ => "Not rated"
        };
        public string RecommendationText => WouldRecommend ? "Yes, would recommend" : "No, would not recommend";
        public string FormattedDate => CreatedAt.ToString("MMM dd, yyyy");
        public string MentorshipPeriod => $"{MatchStartDate.ToString("MMM dd, yyyy")} - {(MatchEndDate?.ToString("MMM dd, yyyy") ?? "Present")}";
    }
}
