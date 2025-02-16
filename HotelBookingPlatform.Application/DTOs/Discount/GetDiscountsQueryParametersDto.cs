using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Enums.SortingColumns;

namespace HotelBookingPlatform.Application.DTOs.Discount;

public class GetDiscountsQueryParametersDto : QueryParameters<DiscountSortColumn>
{
    private new string? SearchTerm { get; set; }
}
