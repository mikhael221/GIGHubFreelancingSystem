namespace Freelancing.Models
{
    public class MenteeDashboard
    {
        public List<MentorshipRequestViewModel> RequestsSent { get; set; } = new List<MentorshipRequestViewModel>();
        public List<MentorshipSessionItem> UpcomingSessions { get; set; } = new List<MentorshipSessionItem>();
        public List<MentorshipSessionItem> ProposedSessions { get; set; } = new List<MentorshipSessionItem>();
    }

    public class MentorshipRequestViewModel
    {
        public Guid Id { get; set; }
        public string MentorName { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Notes { get; set; }
    }
}
