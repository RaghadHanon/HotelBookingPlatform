using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
public interface IAmenityRepository
{
    Task<Amenity> AddAmenityAsync(Amenity amenity);
    Task<bool> AmenityExistsAsync(Guid id);
    Task<bool> DeleteAmenityAsync(Guid id);
    Task<PaginatedResult<Amenity>> GetAllAmenitiesAsync(GetAmenitiesQueryParametersDto request);
    Task<Amenity?> GetAmenityAsync(Guid id);
    Task<bool> SaveChangesAsync();
}
