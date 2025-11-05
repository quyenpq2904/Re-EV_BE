using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Marketplace.DTOs
{
    public class CreateAuctionListingDTO
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public float BiddingIncrements { get; set; }

        // File types và sizes đã được validate (jpg, jpeg, png, webp, max 5MB)
        // TODO: Upload ảnh lên storage và lấy URLs (bước 3, 4, 5)
        // Nhận ảnh từ FormData với name = "images"
        public IFormFile[]? Images { get; set; }

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
        public DateTimeOffset AuctionStartTime { get; set; }

        [Required]
        public DateTimeOffset AuctionEndTime { get; set; }
    }
}

