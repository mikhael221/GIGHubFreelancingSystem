namespace Freelancing.Models.Entities
{
    public class ProjectSkill
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
        public Guid UserSkillId { get; set; }
        public UserSkill UserSkill { get; set; }
    }
}
