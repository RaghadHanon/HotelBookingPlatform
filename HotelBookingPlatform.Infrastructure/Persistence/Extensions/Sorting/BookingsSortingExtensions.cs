using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class BookingSortingExtensions
{
    public static Expression<Func<Booking, object>> GetSortingCriterion(
        this GetBookingsQueryParametersDto request)
    {
        return request.SortColumn switch
        {
            BookingSortColumn.NumberOfChildren => b => b.NumberOfChildren,
            BookingSortColumn.NumberOfAdults => b => b.NumberOfAdults,
            BookingSortColumn.CheckInDate => b => b.CheckInDate,
            BookingSortColumn.CheckOutDate => b => b.CheckOutDate,
            BookingSortColumn.Price => b => b.Price,
            _ => b => b.Id
        };
    }
}