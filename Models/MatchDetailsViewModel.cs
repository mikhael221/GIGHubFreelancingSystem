using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class MatchDetailsViewModel
    {
        public MentorshipMatch Match { get; set; }
        public UserAccount Partner { get; set; }
        public string PartnerRole { get; set; }
        public List<UserSkill> SharedSkills { get; set; } = new();
        public bool IsCurrentUserMentor { get; set; }
    }
}
