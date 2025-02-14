using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;

public interface IHotelRepository
{
    Task<Hotel> AddHotelAsync(Hotel hotel);
    Task<HotelImage> AddHotelImageAsync(Hotel hotel, HotelImage hotelImage);
    Amenity AddAmenityToHotelAsync(Hotel hotel, Amenity amenity);
    Task<PaginatedResult<Amenity>> GetAmenitiesForHotelAsync(Guid id, GetAmenitiesQueryParametersDto request);
    Task<Location> AddHotelLocationAsync(Hotel hotel, Location location);
    Task<bool> DeleteHotelAsync(Guid id);
    Task<PaginatedResult<Hotel>> GetAllHotelsAsync(GetHotelsQueryParametersDto request);
    Task<Hotel?> GetHotelAsync(Guid id);
    Task<Hotel?> GetHotelByNameAsync(string Name);
    Task<bool> HotelExistsAsync(Guid id);
    Task<bool> SaveChangesAsync();
    Task<PaginatedResult<Hotel>> SearchAndFilterHotelsAsync(HotelSearchAndFilterParametersDto request);
}
