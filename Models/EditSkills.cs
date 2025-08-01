using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class EditSkills
    {
        public List<UserSkill> UserSkills { get; set; } = new();
        public string SearchTerm { get; set; }
        public List<Guid> SelectedSkillIds { get; set; } = new List<Guid>();
    }
}
