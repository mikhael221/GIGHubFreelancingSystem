namespace Freelancing.Models
{
    public class MentorshipSessionItem
    {
        public Guid SessionId { get; set; }
        public Guid MatchId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public DateTime StartUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsCreatedByCurrentUser { get; set; }
    }
}



