using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Marketplace.DTOs
{
    public class UpdateListingDTO
    {
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }
        public string[]? Images { get; set; }
        public int BatteryPercentage { get; set; }
        public int YearOfManufacture { get; set; }
        public Condition Condition { get; set; }
    }
}
