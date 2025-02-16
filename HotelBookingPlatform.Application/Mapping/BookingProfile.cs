using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingOutputDto>()
            .ForMember(dest => dest.ConfirmationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RoomNumbers, opt => opt.MapFrom(src => src.BookingRooms.Select(r => r.Room.RoomNumber)))
            .ForMember(dest => dest.GuestFullName, opt => opt.MapFrom(src => src.Guest.FullName))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));

        CreateMap<Booking, BookingForGuestOutputDto>()
            .ForMember(dest => dest.ConfirmationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RoomNumbers, opt => opt.MapFrom(src => src.BookingRooms.Select(r => r.Room.RoomNumber)))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));

        CreateMap<Booking, InvoiceDto>()
            .ForMember(dest => dest.ConfirmationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.GuestId, opt => opt.MapFrom(src => src.GuestId))
            .ForMember(dest => dest.GuestFullName, opt => opt.MapFrom(src => src.Guest.FullName))
            .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate))
            .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => src.CheckOutDate))
            .ForMember(dest => dest.NumberOfAdults, opt => opt.MapFrom(src => src.NumberOfAdults))
            .ForMember(dest => dest.NumberOfChildren, opt => opt.MapFrom(src => src.NumberOfChildren))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.BookingRooms.Select(r => r.Room)))
            .ForMember(dest => dest.Hotel, opt => opt.MapFrom(src => src.Hotel))
            .ForMember(dest => dest.TotalPriceAfterDiscount, opt => opt.Ignore());
    }
}
