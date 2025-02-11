using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;

public interface ICityRepository
{
    Task<City> AddCityAsync(City city);
    Task<CityImage> AddCityImageAsync(City city, CityImage cityImage);
    Task<bool> CityExistsAsync(Guid id);
    Task<bool> DeleteCityAsync(Guid id);
    Task<PaginatedResult<City>> GetAllCitiesAsync(GetCitiesQueryParametersDto request);
    Task<City?> GetCityAsync(Guid id);
    Task<City?> GetCityByNameAsync(string name);
    Task<IEnumerable<City>> GetMostVisitedCitiesAsync(int count = 5);
    Task<bool> SaveChangesAsync();
}
