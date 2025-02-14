using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class CitySortingCriteriaExtensions
{
    public static Expression<Func<City, object>> GetSortingCriterion(
        this GetCitiesQueryParametersDto request)
    {
        return request.SortColumn switch
        {
            CitySortColumn.CreationDate => c => c.CreationDate,
            CitySortColumn.LastModified => c => c.LastModified,
            CitySortColumn.Name => c => c.Name,
            CitySortColumn.Country => c => c.Country,
            CitySortColumn.PostOffice => c => c.PostOffice,
            CitySortColumn.NumbersOfHotels => c => c.Hotels.Count,
            _ => c => c.Id
        };
    }
}