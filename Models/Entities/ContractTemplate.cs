namespace Freelancing.Models.Entities
{
    public class ContractTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // "Web Development", "Mobile App", "Design", etc.
        public string TemplateContent { get; set; } // HTML template with placeholders
        
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
        public DateTime? LastModifiedAt { get; set; }
        
        // Template metadata
        public string? PreviewImagePath { get; set; }
        public int UsageCount { get; set; } = 0;
        public string? TemplateVersion { get; set; } = "1.0";
    }
}

