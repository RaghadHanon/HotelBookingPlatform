using HotelBookingPlatform.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions;

public static class PaginationExtensions
{
    public static async Task<PaginationMetadata> GetPaginationMetadataAsync<T>(
        this IQueryable<T> query, int pageNumber, int pageSize)
    {
        int queryRecordsCount = await query.CountAsync();
        return new PaginationMetadata(pageNumber, pageSize, queryRecordsCount);
    }

    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}

