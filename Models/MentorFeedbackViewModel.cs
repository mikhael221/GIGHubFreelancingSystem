using System.ComponentModel.DataAnnotations;

namespace Freelancing.Models
{
    public class MentorFeedbackViewModel
    {
        public Guid MatchId { get; set; }
        public string MentorName { get; set; } = string.Empty;
        public string? MentorPhoto { get; set; }
        public string MenteeName { get; set; } = string.Empty;
        public DateTime MatchStartDate { get; set; }
        public DateTime? MatchEndDate { get; set; }

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please indicate if you would recommend this mentor")]
        public bool WouldRecommend { get; set; }

        [StringLength(2000, ErrorMessage = "Comments cannot exceed 2000 characters")]
        public string? Comments { get; set; }

        [StringLength(500, ErrorMessage = "Strengths cannot exceed 500 characters")]
        public string? Strengths { get; set; }

        [StringLength(500, ErrorMessage = "Areas for improvement cannot exceed 500 characters")]
        public string? AreasForImprovement { get; set; }


    }
}
