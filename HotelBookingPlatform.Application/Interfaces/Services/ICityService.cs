using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface ICityService
{
    Task<bool> CityExistsAsync(Guid id);
    Task<CityOutputDto> CreateCityAsync(CreateCityDto request);
    Task<bool> DeleteCityAsync(Guid id);
    Task<PaginatedResult<CityOutputDto>> GetAllCitiesAsync(GetCitiesQueryParametersDto request);
    Task<CityOutputWithHotelsAndImagesDto?> GetCityAsync(Guid id);
    Task<IEnumerable<CityAsTrendingDestinationOutputDto>> MostVisitedCitiesAsync(int count = 5);
    Task<bool> UpdateCityAsync(Guid id, UpdateCityDto request);
    Task<bool> UploadImageAsync(Guid cityId, IFormFile file, string basePath, string? alternativeText, bool? thumbnail = false);
}
