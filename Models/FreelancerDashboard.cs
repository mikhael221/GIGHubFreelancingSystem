using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class FreelancerDashboard
    {
        public List<Bidding> Biddings { get; set; } = new();
        public Project? Project { get; set; }
    }
}
