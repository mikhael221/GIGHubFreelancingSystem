namespace Freelancing.Models.Entities
{
    public class Goal
    {
        public Guid Id { get; set; }
        public string GoalName { get; set; }
        public string GoalDescription { get; set; }
        public int Order { get; set; } // Order/sequence of the goal
        public bool IsActive { get; set; } = true; // Whether this goal is active in the system
        public string? IconSvg { get; set; } // Custom SVG icon for the goal
    }
}
