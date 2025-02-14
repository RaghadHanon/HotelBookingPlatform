using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class DiscountProfile : Profile
{
    public DiscountProfile()
    {
        CreateMap<Discount, DiscountOutputDto>()
            // calculated at service
            .ForMember(dest => dest.OriginalPrice, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountedPrice, opt => opt.Ignore());

        CreateMap<Discount, DiscountForRoomOutputDto>();
    }
}
