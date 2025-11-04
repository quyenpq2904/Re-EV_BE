using AutoMapper;
using ReEV.Service.Marketplace.DTOs;
using ReEV.Service.Marketplace.Models;

namespace ReEV.Service.Marketplace.Mappings
{
    public class ListingProfile : Profile
    {
        public ListingProfile()
        {
            CreateMap<Listing, ListingDTO>();
        }
    }
}
