using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class AmenityProfile : Profile
{
    public AmenityProfile()
    {
        CreateMap<CreateAndUpdateAmenityDto, Amenity>();
        CreateMap<Amenity, CreateAndUpdateAmenityDto>();
        CreateMap<Amenity, AmenityOutputDto>();
    }
}
