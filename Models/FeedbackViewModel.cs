using System.ComponentModel.DataAnnotations;

namespace Freelancing.Models
{
    public class FeedbackViewModel
    {
        public Guid AcceptBidId { get; set; }
        public Guid ProjectId { get; set; }
        public string FreelancerName { get; set; } = string.Empty;
        public string? FreelancerPhoto { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhoto { get; set; }
        public string? ProjectName { get; set; }

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please indicate if you would recommend this freelancer?")]
        public bool WouldRecommend { get; set; }

        [StringLength(2000, ErrorMessage = "Comments cannot exceed 2000 characters")]
        public string? Comments { get; set; }
    }
}
