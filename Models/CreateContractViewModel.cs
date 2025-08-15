using System.ComponentModel.DataAnnotations;

namespace Freelancing.Models
{
    public class CreateContractViewModel
    {
        public Guid ProjectId { get; set; }
        public Guid BiddingId { get; set; }
        
        // Project Information (read-only for display)
        public string ProjectName { get; set; } = "";
        public string ProjectDescription { get; set; } = "";
        public string ProjectBudget { get; set; } = "";
        public string ProjectCategory { get; set; } = "";
        
        // Client and Freelancer Info (read-only for display)
        public string ClientName { get; set; } = "";
        public string ClientEmail { get; set; } = "";
        public string FreelancerName { get; set; } = "";
        public string FreelancerEmail { get; set; } = "";
        
        // Accepted Bid Info (read-only for display)
        public int AgreedAmount { get; set; }
        public string DeliveryTimeline { get; set; } = "";
        public string Proposal { get; set; } = "";
        
        // Contract Template Selection
        [Required(ErrorMessage = "Please select a contract template")]
        public Guid? SelectedTemplateId { get; set; }
        public List<ContractTemplateOption> AvailableTemplates { get; set; } = new List<ContractTemplateOption>();
        
        // Custom Contract Content (if not using template)
        public string? CustomContractContent { get; set; }
        
        // Payment Terms Configuration
        [Range(0, 100, ErrorMessage = "Upfront percentage must be between 0 and 100")]
        public int UpfrontPercentage { get; set; } = 30;
        
        [Range(0, 100, ErrorMessage = "Final percentage must be between 0 and 100")]
        public int FinalPercentage { get; set; } = 70;
        
        public List<CreateMilestoneViewModel> Milestones { get; set; } = new List<CreateMilestoneViewModel>();
        
        // Revision Policy
        [Range(0, 10, ErrorMessage = "Free revisions must be between 0 and 10")]
        public int FreeRevisions { get; set; } = 3;
        
        [Range(0, 1000, ErrorMessage = "Additional revision cost must be reasonable")]
        public decimal AdditionalRevisionCost { get; set; } = 50;
        
        public string RevisionScope { get; set; } = "Minor changes and adjustments within the original scope";
        
        // Timeline Configuration
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);
        
        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; } = DateTime.Today.AddDays(30);
        
        // Deliverable Requirements
        public List<string> DeliverableRequirements { get; set; } = new List<string>();
        public string NewDeliverableRequirement { get; set; } = "";
        
        // Additional Terms
        public string? AdditionalTerms { get; set; }
        
        // Validation
        public bool IsValid => 
            Deadline > StartDate &&
            !string.IsNullOrEmpty(ProjectName);
    }

    public class CreateMilestoneViewModel
    {
        [Required(ErrorMessage = "Milestone name is required")]
        public string Name { get; set; } = "";
        
        public string Description { get; set; } = "";
        
        [Range(1, 100, ErrorMessage = "Milestone percentage must be between 1 and 100")]
        public int Percentage { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }

    public class ContractTemplateOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string? PreviewImagePath { get; set; }
    }
}
