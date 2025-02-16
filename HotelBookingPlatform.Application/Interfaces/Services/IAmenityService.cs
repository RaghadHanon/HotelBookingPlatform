using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;

namespace HotelBookingPlatform.Application.Interfaces.Services;
public interface IAmenityService
{
    Task<AmenityOutputDto> CreateAmenity(CreateAndUpdateAmenityDto request);
    Task<bool> DeleteAmenityAsync(Guid id);
    Task<PaginatedResult<AmenityOutputDto>> GetAllAmenitiesAsync(GetAmenitiesQueryParametersDto request);
    Task<AmenityOutputDto> GetAmenity(Guid id);
    Task<bool> UpdateAmenity(Guid id, CreateAndUpdateAmenityDto request);
}
