using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class FeaturedDealProfile : Profile
{

    public FeaturedDealProfile()
    {
        HotelImage defaultHotelImage = new HotelImage
        {
            ImageUrl = "C:\\Users\\user\\source\\repos\\HotelBookingSystem\\HotelBookingSystem.Api\\wwwroot\\images\\common\\defaultHotel.jpg",
            AlternativeText = "Default hotel image"
        };

        CreateMap<Room, FeaturedDealOutputDto>()
    .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Hotel.Id))
    .ForMember(dest => dest.HotelImage, opt => opt.MapFrom(src =>
                        src.Hotel.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail"))
                        ?? src.Hotel.Images.FirstOrDefault()
                        ?? defaultHotelImage
                        ))
    .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
    .ForMember(dest => dest.StarRate, opt => opt.MapFrom(src => src.Hotel.StarRate))
    .ForPath(dest => dest.Location.Latitude, opt => opt.MapFrom(src => src.Hotel.Location.Latitude))
    .ForPath(dest => dest.Location.Longitude, opt => opt.MapFrom(src => src.Hotel.Location.Longitude))
    .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Hotel.Street))
    .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.Hotel.City.Name))
    .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Hotel.City.Country))
    .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.Price))
    .ForMember(dest => dest.DiscountedPrice, opt => opt.MapFrom(src => src.Discounts.First().DiscountedPrice))
    .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => src.Discounts.First().Percentage));

    }
}
