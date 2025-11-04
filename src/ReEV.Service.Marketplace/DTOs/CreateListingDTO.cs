using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Marketplace.DTOs
{
    public class CreateListingDTO
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }

        public float? BiddingIncrements { get; set; }

        public string[]? Images { get; set; }
        [Required]
        public string Brand { get; set; } = null!;

        [Required]
        public string Model { get; set; } = null!;

        [Range(0, 100)]
        public int BatteryPercentage { get; set; }

        [Range(1900, 2100)]
        public int YearOfManufacture { get; set; }

        [Required]
        public Condition Condition { get; set; }

        [Required]
        public ListingType ListingType { get; set; }

        // Đối với AUCTION loại listing:
        public DateTimeOffset? AuctionStartTime { get; set; }
        public DateTimeOffset? AuctionEndTime { get; set; }
    }
}
