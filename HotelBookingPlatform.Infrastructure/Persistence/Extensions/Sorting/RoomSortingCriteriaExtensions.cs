using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Domain.Models;
using System.Linq.Expressions;

namespace HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;

public static class RoomSortingExtensions
{
    public static Expression<Func<Room, object>> GetSortingCriterion(
        this GetRoomsQueryParametersDto request)
    {
        return request.SortColumn switch
        {
            RoomSortColumn.CreationDate => r => r.CreationDate,
            RoomSortColumn.LastModified => r => r.LastModified,
            RoomSortColumn.RoomNumber => r => r.RoomNumber,
            RoomSortColumn.AdultsCapacity => r => r.AdultsCapacity,
            RoomSortColumn.ChildrenCapacity => r => r.ChildrenCapacity,
            RoomSortColumn.HotelName => r => r.Hotel.Name,
            _ => r => r.Id
        };
    }
}