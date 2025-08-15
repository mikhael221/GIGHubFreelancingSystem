using System.ComponentModel.DataAnnotations;

namespace Freelancing.Models
{
    public class SubmitDeliverableViewModel
    {
        public Guid ContractId { get; set; }
        
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        
        // File upload properties
        public IFormFileCollection? SubmittedFiles { get; set; }
        public string? AllSelectedFiles { get; set; } // Hidden field for JS file handling
        
        // Repository links
        public string? RepositoryLinks { get; set; } // JSON array of links
    }
}
