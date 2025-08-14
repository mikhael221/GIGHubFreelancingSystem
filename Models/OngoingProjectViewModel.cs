using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class OngoingProjectViewModel
    {
        // Project Information
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectBudget { get; set; }
        public string ProjectCategory { get; set; }
        public DateTime ProjectCreatedAt { get; set; }
        public List<string> ProjectImageUrls { get; set; } = new();
        
        // Client Information
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhoto { get; set; }
        
        // Freelancer Information  
        public Guid FreelancerId { get; set; }
        public string FreelancerName { get; set; }
        public string FreelancerEmail { get; set; }
        public string FreelancerPhoto { get; set; }
        
        // Accepted Bid Information
        public int AcceptedBidAmount { get; set; }
        public string AcceptedBidDelivery { get; set; }
        public string AcceptedBidProposal { get; set; }
        public DateTime? BiddingAcceptedDate { get; set; }
        
        // Project Status
        public string ProjectStatus { get; set; } = "Active";
        
        // Additional Details
        public List<UserSkill> ProjectRequiredSkills { get; set; } = new();
        public List<UserSkill> FreelancerSkills { get; set; } = new();
    }
    
    public class OngoingProjectListViewModel
    {
        public List<OngoingProjectViewModel> OngoingProjects { get; set; } = new();
        public int TotalActiveProjects { get; set; }
        public string UserRole { get; set; } // "Client" or "Freelancer"
    }
}
