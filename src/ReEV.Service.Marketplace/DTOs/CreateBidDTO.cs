using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Marketplace.DTOs
{
    public class CreateBidDTO
    {
        [Required]
        public string listing_id { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public float bid_ammount { get; set; }
    }
}

