using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class HotelSearchResultsSortingCriteriaExtensions
{
    public static Expression<Func<Hotel, object>> GetSearchResultsSortingCriterion(
        this HotelSearchAndFilterParametersDto request)
    {
        return request.SortColumn switch
        {
            HotelSearchSortColumn.Name => h => h.Name,
            HotelSearchSortColumn.Price => h => h.Rooms.Min(r => r.Price),
            HotelSearchSortColumn.Rating => h => h.Reviews.Average(r => r.Rating),
            HotelSearchSortColumn.StarRating => h => h.StarRate,
            _ => h => h.Id
        };
    }
}