using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Guest;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class GuestProfile : Profile
{
    public GuestProfile()
    {
        CreateMap<Guest, GuestOutputDto>()
            .ForMember(dest => dest.NumberOfBookings, opt => opt.MapFrom(src => src.Bookings.Count));
    }
}
