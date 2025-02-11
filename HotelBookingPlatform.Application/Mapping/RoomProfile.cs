using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<CreateRoomDto, Room>();

        CreateMap<Room, RoomOutputDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));

        CreateMap<Room, RoomWithFullDataOutputDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));

        CreateMap<UpdateRoomDto, Room>();

        CreateMap<RoomImage, RoomImageOutputDto>();

        CreateMap<Room, RoomWithImageOutputDto>()
            .ForMember(dest => dest.RoomImage, opt => opt.MapFrom(src =>
                                src.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail"))
                                ?? src.Images.FirstOrDefault()
                                )
                      );

        CreateMap<Room, RoomWithinInvoiceDto>()
            .ForMember(dest => dest.PricePerNight, opt => opt.Ignore())
            .ForMember(dest => dest.PricePerNightAfterDiscount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRoomPrice, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRoomPriceAfterDiscount, opt => opt.Ignore())
            .ForMember(dest => dest.NumberOfNights, opt => opt.Ignore());
    }
}
