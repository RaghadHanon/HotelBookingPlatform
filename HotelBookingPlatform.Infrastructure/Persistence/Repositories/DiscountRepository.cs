using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using HotelBookingPlatform.Infrastructure.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public DiscountRepository(HotelBookingPlatformDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Discount> AddDiscountAsync(Room room, Discount discount)
    {
        var entry = await _dbContext.Discounts.AddAsync(discount);
        room.Discounts.Add(discount);

        return entry.Entity;
    }

    public async Task<bool> DeleteDiscountAsync(Guid roomId, Guid discountId)
    {
        var discount = await _dbContext
                             .Discounts
                             .FirstOrDefaultAsync(d => d.RoomId == roomId && d.Id == discountId);
        if (discount is null)
        {
            return false;
        }

        _dbContext.Discounts.Remove(discount);

        return true;
    }

    public async Task<Discount?> GetDiscountAsync(Guid roomId, Guid discountId)
    {
        return await _dbContext.Discounts
            .Include(d => d.Room)
            .FirstOrDefaultAsync(d => d.RoomId == roomId && d.Id == discountId);
    }

    public async Task<PaginatedResult<Discount>> GetDiscountsForRoomAsync(Guid roomId, GetDiscountsQueryParametersDto request)
    {
        var query= _dbContext.Discounts
            .Where(d => d.RoomId == roomId)
            .Include(d=> d.Room)
            .AsQueryable();

        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        var paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        var result = await query
                          .AsNoTracking()
                          .ToListAsync();

        return new PaginatedResult<Discount>(result, paginationMetadata);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }
}

