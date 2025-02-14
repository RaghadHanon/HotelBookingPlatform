using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Review;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IReviewService
{
    Task<ReviewOutputDto> AddReviewAsync(Guid hotelId, CreateOrUpdateReviewDto request);
    Task<bool> DeleteReviewAsync(Guid hotelId, Guid reviewId);
    Task<bool> UpdateReviewAsync(Guid hotelId, Guid reviewId, CreateOrUpdateReviewDto request);
    Task<ReviewOutputDto> GetReviewAsync(Guid id, Guid reviewId);
    Task<PaginatedResult<ReviewOutputDto>> GetHotelReviewsAsync(Guid id, GetHotelReviewsQueryParameters request);
    Task<double> GetHotelAverageRatingAsync(Guid id);


}
