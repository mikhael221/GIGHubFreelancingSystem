namespace Freelancing.Models.Entities
{
    public class Bidding
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserAccount User { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
        public int Budget { get; set; }
        public string Delivery { get; set; }
        public string Proposal { get; set; }
        public bool IsAccepted { get; set; }
    }
}
