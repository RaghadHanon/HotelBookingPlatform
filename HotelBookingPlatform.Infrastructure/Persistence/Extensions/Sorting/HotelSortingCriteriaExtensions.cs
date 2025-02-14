using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class HotelSortingExtensions
{
    public static Expression<Func<Hotel, object>> GetSortingCriterion(
        this GetHotelsQueryParametersDto request)
    {
        return request.SortColumn switch
        {
            HotelSortColumn.CreationDate => h => h.CreationDate,
            HotelSortColumn.LastModified => h => h.LastModified,
            HotelSortColumn.Name => h => h.Name,
            HotelSortColumn.Owner => h => h.Owner,
            HotelSortColumn.StarRate => h => h.StarRate,
            HotelSortColumn.RoomsNumber => h => h.Rooms.Count,
            _ => h => h.Id
        };
    }
}

