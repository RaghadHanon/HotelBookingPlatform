using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class BookingRoomProfile : Profile
{
    public BookingRoomProfile()
    {
        CreateMap<BookingRoom, RoomWithinInvoiceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Room.Id))
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
            .ForMember(dest => dest.AdultsCapacity, opt => opt.MapFrom(src => src.Room.AdultsCapacity))
            .ForMember(dest => dest.ChildrenCapacity, opt => opt.MapFrom(src => src.Room.ChildrenCapacity))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.Room.RoomType))
            .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.Room.Price))
            .ForMember(dest => dest.PricePerNightAfterDiscount, opt => opt.Ignore()) // Needs calculation based on Discount applied
            .ForMember(dest => dest.NumberOfNights, opt => opt.Ignore()) // Needs calculation based on check-in/out dates
            .ForMember(dest => dest.TotalRoomPrice, opt => opt.Ignore()) // Needs dynamic calculation
            .ForMember(dest => dest.TotalRoomPriceAfterDiscount, opt => opt.Ignore()); // Needs dynamic calculation
    }
}
