using Freelancing.Models.Entities;

namespace Freelancing.Models
{
    public class AddBidding
    {
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public int Budget { get; set; }
        public string Delivery { get; set; }
        public string Proposal { get; set; }
    }
}
