using System.ComponentModel.DataAnnotations;

namespace Freelancing.Models
{
    public class MentorshipRegistration
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
