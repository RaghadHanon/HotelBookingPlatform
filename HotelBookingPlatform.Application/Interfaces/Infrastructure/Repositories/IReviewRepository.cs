using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;

public interface IReviewRepository
{
    Task<Review> AddReviewAsync(Hotel hotel, Review review);
    Task<bool> DeleteReviewAsync(Guid id, Guid reviewId);
    Task<double> GetHotelAverageRatingAsync(Hotel hotel);
    Task<PaginatedResult<Review>> GetHotelReviewsAsync(Hotel hotel, GetHotelReviewsQueryParameters request);
    Task<Review?> GetReviewAsync(Hotel hotel, Guid reviewId);
    Task<bool> SaveChangesAsync();
}