using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public ReviewRepository(HotelBookingPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Review> AddReviewAsync(Hotel hotel, Review review)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Review> entry = await _dbContext.Reviews.AddAsync(review);
        hotel.Reviews.Add(review);

        return entry.Entity;
    }

    public async Task<Review?> GetReviewAsync(Hotel hotel, Guid reviewId)
    {
        return await _dbContext.Reviews
             .Include(r => r.Hotel)
             .Include(r => r.Guest)
               .ThenInclude(G => G.Bookings)
             .FirstOrDefaultAsync(r => r.Id == reviewId && r.HotelId == hotel.Id);
    }

    public async Task<bool> DeleteReviewAsync(Guid hotelId, Guid reviewId)
    {
        Review? review = await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.HotelId == hotelId);
        if (review is null)
        {
            return false;
        }

        _dbContext.Reviews.Remove(review);

        return true;
    }

    public async Task<PaginatedResult<Review>> GetHotelReviewsAsync
        (Hotel hotel, GetHotelReviewsQueryParameters request)
    {
        IQueryable<Review> query = _dbContext.Reviews
                        .Include(r => r.Hotel)
                        .Include(r => r.Guest)
                           .ThenInclude(G => G.Bookings)
                        .Where(r => r.HotelId == hotel.Id);

        query = SearchInTitleOrDescription(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<Review> result = await query
                          .AsNoTracking()
                          .ToListAsync();

        return new PaginatedResult<Review>(result, paginationMetadata);
    }

    private static IQueryable<Review> SearchInTitleOrDescription
        (IQueryable<Review> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(r => (!string.IsNullOrEmpty(r.Title) && r.Title.StartsWith(searchTerm))
                             || r.Description.StartsWith(searchTerm));
        }

        return query;
    }

    public async Task<double> GetHotelAverageRatingAsync(Hotel hotel)
    {
        bool hasReviews = await HasHotelReviewsAsync(hotel);
        if (!hasReviews)
        {
            return 0;
        }

        return await _dbContext.Reviews
            .Where(r => r.HotelId == hotel.Id)
            .AverageAsync(r => r.Rating);
    }

    private Task<bool> HasHotelReviewsAsync(Hotel hotel)
    {
        return _dbContext.Reviews.AnyAsync(r => r.HotelId == hotel.Id);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }
}
