namespace Freelancing.Models
{
    public class MentorDashboard
    {
        public List<MentorshipReceivedViewModel> RequestsReceived { get; set; } = new List<MentorshipReceivedViewModel>();
    }
    public class MentorshipReceivedViewModel
    {
        public Guid Id { get; set; }
        public string MenteeName { get; set; }
        public string Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Notes { get; set; }
    }
}
