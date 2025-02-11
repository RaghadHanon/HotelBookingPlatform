using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IHotelService
{
    Task<HotelOutputDto> CreateHotelAsync(CreateHotelDto request);
    Task<bool> DeleteHotelAsync(Guid id);
    Task<PaginatedResult<HotelOutputDto>> GetAllHotelsAsync(GetHotelsQueryParametersDto request);
    Task<HotelWithFullDataOutputDto?> GetHotelAsync(Guid id);
    Task<PaginatedResult<HotelSearchResultOutputDto>> SearchAndFilterHotelsAsync(HotelSearchAndFilterParametersDto request);
    Task<bool> UpdateHotelAsync(Guid id, UpdateHotelDto request);
    Task<bool> UploadImageAsync(Guid hotelId, IFormFile file, string basePath, string? alternativeText, bool? thumbnail = false);
    Task<bool> AddAmenityToHotelAsync(Guid hotelId, Guid anenityId);
    Task<PaginatedResult<AmenityOutputDto>> GetAmenitiesForHotelAsync(Guid hotelId, GetAmenitiesQueryParametersDto request);
}
