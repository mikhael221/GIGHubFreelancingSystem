using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class MentorshipMatchingViewModel
    {
        public List<UserAccount> PotentialMatches { get; set; } = new();
        /*public List<MentorshipMatch> ExistingMatches { get; set; } = new();*/
        public string UserRole { get; set; } // "Mentor" or "Mentee"
        public int TotalSkills { get; set; }
        public List<UserSkill> UserSkills { get; set; } = new();
        public bool HasSkills { get; set; }
    }
}
