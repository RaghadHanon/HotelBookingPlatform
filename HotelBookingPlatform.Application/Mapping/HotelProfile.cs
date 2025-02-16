using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.DTOs.Location;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class HotelProfile : Profile
{
    public HotelProfile()
    {
        CreateMap<LocationDto, Location>().ReverseMap();

        CreateMap<CreateLocationDto, Location>().ReverseMap();

        CreateMap<CreateHotelDto, Hotel>();

        CreateMap<UpdateHotelDto, Hotel>();

        CreateMap<Hotel, HotelOutputDto>()
            .ForMember(dest => dest.RoomsNumber, opt => opt.MapFrom(src => src.Rooms.Count))
            .ForMember(dest => dest.HotelImage, opt => opt.MapFrom(src =>
                src.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail")) ?? src.Images.FirstOrDefault()));

        CreateMap<HotelImage, HotelImageOutputDto>();

        CreateMap<Booking, RecentlyVisitedHotelOutputDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.Hotel.City.Name))
            .ForMember(dest => dest.StarRating, opt => opt.MapFrom(src => src.Hotel.StarRate))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Hotel.Street))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Hotel.Location))
            .ForMember(dest => dest.HotelImage, opt => opt.MapFrom(src =>
                src.Hotel.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail")) ?? src.Hotel.Images.FirstOrDefault()));

        CreateMap<Hotel, HotelWithFullDataOutputDto>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name));

        CreateMap<Hotel, HotelSearchResultOutputDto>()
            .ForMember(dest => dest.HotelImage, opt => opt.MapFrom(src =>
                src.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail")) ?? src.Images.FirstOrDefault()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                src.Rooms.Any() ? src.Rooms.Min(r => r.Price) : 0))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

        CreateMap<Hotel, HotelWithinInvoiceDto>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name));
    }
}
