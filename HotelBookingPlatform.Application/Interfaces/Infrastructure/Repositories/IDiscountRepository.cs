using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;

public interface IDiscountRepository
{
    Task<Discount> AddDiscountAsync(Room room, Discount discount);
    Task<bool> DeleteDiscountAsync(Guid roomId, Guid discountId);
    Task<Discount?> GetDiscountAsync(Guid roomId, Guid discountId);
    Task<PaginatedResult<Discount>> GetDiscountsForRoomAsync(Guid roomId, GetDiscountsQueryParametersDto request);
    Task<bool> SaveChangesAsync();
}
