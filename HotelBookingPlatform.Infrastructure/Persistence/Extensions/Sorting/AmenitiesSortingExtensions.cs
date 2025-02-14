using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
public static class AmenitiesSortingExtensions
{
    public static Expression<Func<Amenity, object>> GetSortingCriterion(
        this GetAmenitiesQueryParametersDto request)
    {
        return request.SortColumn switch
        {
            AmenitySortColumn.Name => a => a.Name,
            AmenitySortColumn.Description => a => a.Description!,
            _ => a => a.Id
        };
    }
}
