using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class ReviewSortingCriteriaExtensions
{
    public static Expression<Func<Review, object>> GetSortingCriterion(
        this GetHotelReviewsQueryParameters request)
    {
        return request.SortColumn switch
        {
            ReviewSortColumn.CreationDate => r => r.CreationDate,
            ReviewSortColumn.LastModified => r => r.LastModified,
            ReviewSortColumn.Rating => r => r.Rating,
            _ => r => r.Id
        };
    }
}

