using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class MentorshipMatchingViewModel
    {
        public List<UserAccount> PotentialMatches { get; set; } = new();
        public List<MentorshipMatch> ExistingMatches { get; set; } = new();
        public string UserRole { get; set; } // "Mentor" or "Mentee"
        public int TotalSkills { get; set; }
        public List<UserSkill> UserSkills { get; set; } = new();
        public bool HasSkills { get; set; }
    }
    public class MatchDetailsViewModel
    {
        public MentorshipMatch Match { get; set; }
        public UserAccount Partner { get; set; }
        public string PartnerRole { get; set; }
        public List<UserSkill> SharedSkills { get; set; } = new();
        public bool IsCurrentUserMentor { get; set; }
    }

    public class CreateMatchRequest
    {
        public Guid PartnerId { get; set; }
        public string Notes { get; set; }
    }

    public class MentorshipMatchListViewModel
    {
        public List<MentorshipMatch> ActiveMatches { get; set; } = new();
        public List<MentorshipMatch> CompletedMatches { get; set; } = new();
        public List<MentorshipMatch> CancelledMatches { get; set; } = new();
        public int TotalActiveMatches { get; set; }
        public string UserRole { get; set; }
    }
}
