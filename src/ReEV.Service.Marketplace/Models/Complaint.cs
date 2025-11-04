namespace ReEV.Service.Marketplace.Models
{
    public sealed class Complaint : Entity
    {
        public Guid ReporterId { get; set; }
        public Guid? ComplainedUserId { get; set; } = null;
        public Guid? OrderId { get; set; } = null;
        public Guid? ListingId {  get; set; } = null;
        public string Reason { get; set; }
        public ComplaintStatus Status { get; set; } = ComplaintStatus.PENDING;
        public string? Note { get; set; }
        public User Reporter { get; set; } = null!;
        public User? ComplainedUser { get; set; } = null;
        public Listing? Listing { get; set; } = null;

        private Complaint(Guid reporterId, Guid? complainedUserId, Guid? orderId, Guid? listingId,
            string reason)
        {
            ReporterId = reporterId;
            ComplainedUserId = complainedUserId;
            OrderId = orderId;
            ListingId = listingId;
            Reason = reason;
        }

        public static Complaint CreateComplaintUser(Guid reportedId, Guid? complainedUserId, string reason)
        {
            return new Complaint(reportedId, complainedUserId, null, null, reason);
        }
        public static Complaint CreateComplaintOrder(Guid reportedId, Guid? orderId, string reason)
        {
            return new Complaint(reportedId, null, orderId, null, reason);
        }
        public static Complaint CreateComplaintListing(Guid reportedId, Guid? listingId, string reason)
        {
            return new Complaint(reportedId, null, null, listingId, reason);
        }
        public static Complaint CreateOtherComplaint(Guid reportedId, string reason)
        {
            return new Complaint(reportedId, null, null, null, reason);
        }
    }
}
