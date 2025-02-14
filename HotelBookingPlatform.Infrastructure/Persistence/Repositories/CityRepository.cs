using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;

public class CityRepository : ICityRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public CityRepository(HotelBookingPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CityExistsAsync(Guid id)
    {
        return await _dbContext.Cities.AnyAsync(c => c.Id == id);
    }

    public async Task<City> AddCityAsync(City city)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<City> entry = await _dbContext.Cities.AddAsync(city);

        return entry.Entity;
    }

    public async Task<CityImage> AddCityImageAsync(City city, CityImage cityImage)
    {
        await _dbContext.CityImages.AddAsync(cityImage);
        city.Images.Add(cityImage);

        return cityImage;
    }

    public async Task<bool> DeleteCityAsync(Guid id)
    {
        City? city = await _dbContext.Cities.FindAsync(id);
        if (city is null)
        {
            return false;
        }

        _dbContext.Cities.Remove(city);

        return true;
    }

    public async Task<City?> GetCityAsync(Guid id)
    {
        return await _dbContext.Cities
                               .Include(c => c.Hotels)
                                  .ThenInclude(h => h.Location)
                               .Include(c => c.Images)
                               .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<City?> GetCityByNameAsync(string name)
    {
        return await _dbContext.Cities.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<IEnumerable<City>> GetMostVisitedCitiesAsync(int count = 5)
    {
        List<Guid> cityIds = await GetCityIdsOfMostVisitedCities(count);
        List<City> cities = await _dbContext.Cities
            .Where(c => cityIds.Contains(c.Id))
            .Include(c => c.Images)
            .AsNoTracking()
            .ToListAsync();

        return cities;
    }

    private async Task<List<Guid>> GetCityIdsOfMostVisitedCities(int count)
    {
        return await _dbContext.Bookings
            .GroupBy(b => b.Hotel.CityId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }

    public async Task<PaginatedResult<City>> GetAllCitiesAsync(GetCitiesQueryParametersDto request)
    {
        IQueryable<City> query = _dbContext.Cities
                    .Include(c => c.Hotels)
                    .Include(c => c.Images)
                    .AsQueryable();
        query = SearchCitiesByNameOrCountry(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<City> result = await query
                           .AsNoTracking()
                           .ToListAsync();

        return new PaginatedResult<City>(result, paginationMetadata);
    }

    private static IQueryable<City> SearchCitiesByNameOrCountry(IQueryable<City> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.StartsWith(searchTerm) || c.Country.StartsWith(searchTerm));
        }

        return query;
    }
}
