using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
public static class DiscountsSortingExtensions
{
    public static Expression<Func<Discount, object>> GetSortingCriterion(
        this GetDiscountsQueryParametersDto request)
    {
        return request.SortColumn switch
        {
            DiscountSortColumn.OriginalPrice => d => d.OriginalPrice,
            DiscountSortColumn.DiscountedPrice => d => d.DiscountedPrice,
            DiscountSortColumn.StartDate => d => d.StartDate,
            DiscountSortColumn.EndDate => d => d.EndDate,
            DiscountSortColumn.Percentage => d => d.Percentage,
            _ => d => d.Id
        };
    }
}