using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ReEV.Common.Enums;

namespace ReEV.Service.Marketplace.DTOs
{
    public class UpdateListingDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }
        
        [MaxLength(5000)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue)]
        public float? Price { get; set; }
        
        // File types và sizes đã được validate (jpg, jpeg, png, webp, max 5MB)
        // TODO: Upload ảnh lên storage và lấy URLs (bước 3, 4, 5)
        // Nhận ảnh từ FormData với name = "images"
        public IFormFile[]? Images { get; set; }
        
        [Range(0, 100)]
        public int? BatteryPercentage { get; set; }
        
        [Range(1900, 2100)]
        public int? YearOfManufacture { get; set; }
        
        public Condition? Condition { get; set; }
    }
}
