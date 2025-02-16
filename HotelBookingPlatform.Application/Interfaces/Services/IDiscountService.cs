using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.DTOs.Hotel;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IDiscountService
{
    Task<PaginatedResult<DiscountForRoomOutputDto>> GetDiscountsForRoomAsync(Guid roomId, GetDiscountsQueryParametersDto request);
    Task<DiscountOutputDto> CreateDiscountAsync(Guid roomId, CreateDiscountDto command);
    Task<bool> DeleteDiscountAsync(Guid roomId, Guid discountId);
    Task<DiscountOutputDto?> GetDiscountAsync(Guid roomId, Guid discountId);
    Task<IEnumerable<FeaturedDealOutputDto>> GetFeaturedDealsAsync(int deals = 5);
}
