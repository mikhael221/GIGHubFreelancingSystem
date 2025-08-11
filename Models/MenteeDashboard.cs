namespace Freelancing.Models
{
    public class MenteeDashboard
    {
        public List<MentorshipRequestViewModel> RequestsSent { get; set; } = new List<MentorshipRequestViewModel>();
        public List<MentorshipSessionItem> UpcomingSessions { get; set; } = new List<MentorshipSessionItem>();
        public List<MentorshipSessionItem> ProposedSessions { get; set; } = new List<MentorshipSessionItem>();
        public int CompletedGoalsCount { get; set; } = 0;
        public int TotalGoalsCount { get; set; } = 7; // Total number of goals in the system
        public double ProgressPercentage => TotalGoalsCount > 0 ? (double)CompletedGoalsCount / TotalGoalsCount * 100 : 0;
    }

    public class MentorshipRequestViewModel
    {
        public Guid Id { get; set; }
        public string MentorName { get; set; }
        public string? MentorPhoto { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}
