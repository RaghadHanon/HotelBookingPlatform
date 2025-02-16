using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Enums.SortingColumns;

namespace HotelBookingPlatform.Application.DTOs.Booking;

public class GetBookingsQueryParametersDto : QueryParameters<BookingSortColumn>
{
}
