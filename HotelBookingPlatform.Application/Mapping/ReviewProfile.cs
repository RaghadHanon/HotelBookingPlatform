using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<CreateOrUpdateReviewDto, Review>();
        CreateMap<Review, ReviewOutputDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));
    }
}
