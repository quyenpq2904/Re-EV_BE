using System.ComponentModel.DataAnnotations;

namespace ReEV.Service.Marketplace.DTOs
{
    public class AddFavouriteRequestDTO
    {
        [Required]
        public string listing_id { get; set; } = null!;
    }
}

