namespace Freelancing.Models.Entities
{
    public class Project
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string Budget { get; set; }
        public string Category { get; set; }
    }
}
