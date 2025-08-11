namespace Freelancing.Models
{
    public class MentorDashboard
    {
        public List<MentorshipReceivedViewModel> RequestsReceived { get; set; } = new List<MentorshipReceivedViewModel>();
        public List<MentorshipSessionItem> UpcomingSessions { get; set; } = new List<MentorshipSessionItem>();
        public List<MentorshipSessionItem> ProposedSessions { get; set; } = new List<MentorshipSessionItem>();
        
        // Review statistics
        public double AverageRating { get; set; } = 0.0;
        public int TotalReviews { get; set; } = 0;
        public int FourPlusStarReviews { get; set; } = 0;
        public int WouldRecommendCount { get; set; } = 0;
    }
    public class MentorshipReceivedViewModel
    {
        public Guid Id { get; set; }
        public string MenteeName { get; set; }
        public string? MenteePhoto { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}
