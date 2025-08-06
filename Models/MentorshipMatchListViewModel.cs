using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class MentorshipMatchListViewModel
    {
        public List<MentorshipMatch> ActiveMatches { get; set; } = new();
        public List<MentorshipMatch> CompletedMatches { get; set; } = new();
        public List<MentorshipMatch> CancelledMatches { get; set; } = new();
        public int TotalActiveMatches { get; set; }
        public string UserRole { get; set; }
    }
}
