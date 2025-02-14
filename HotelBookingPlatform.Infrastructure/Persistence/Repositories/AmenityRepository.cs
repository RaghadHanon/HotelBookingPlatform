using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;
public class AmenityRepository : IAmenityRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public AmenityRepository(HotelBookingPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AmenityExistsAsync(Guid id)
    {
        return await _dbContext.Amenities
            .AnyAsync(h => h.Id == id);
    }

    public async Task<Amenity> AddAmenityAsync(Amenity amenity)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Amenity> entry = await _dbContext.Amenities.AddAsync(amenity);

        return entry.Entity;
    }

    public async Task<bool> DeleteAmenityAsync(Guid id)
    {
        Amenity? amenity = await _dbContext.Amenities.FindAsync(id);
        if (amenity is null)
        {
            return false;
        }

        _dbContext.Amenities.Remove(amenity);

        return true;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }

    public async Task<Amenity?> GetAmenityAsync(Guid id)
    {
        return await _dbContext.Amenities.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<PaginatedResult<Amenity>> GetAllAmenitiesAsync(GetAmenitiesQueryParametersDto request)
    {
        IQueryable<Amenity> query = _dbContext.Amenities.AsQueryable();
        query = SearchAmenitiesByNameOrDescription(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<Amenity> result = await query
                          .AsNoTracking()
                          .ToListAsync();

        return new PaginatedResult<Amenity>(result, paginationMetadata);
    }

    private IQueryable<Amenity> SearchAmenitiesByNameOrDescription(IQueryable<Amenity> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => a.Name.StartsWith(searchTerm) || (a.Description != null && a.Description.StartsWith(searchTerm)));
        }

        return query;
    }
}

