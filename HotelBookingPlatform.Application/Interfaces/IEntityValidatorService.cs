using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces;
public interface IEntityValidatorService
{
    Task<Amenity> ValidateAmenityAsync(Guid amenityId);
    Task<Booking> ValidateBookingAsync(Guid bookingId);
    Task<City> ValidateCityAsync(Guid cityId);
    Task<Discount> ValidateDiscountAsync(Guid roomId, Guid discountId);
    Task<Guest> ValidateGuestAsync(Guid guestId);
    Task<Hotel> ValidateHotelAsync(Guid hotelId);
    Task<Review> ValidateReviewAsync(Hotel hotel, Guid reviewId);
    Task<Room> ValidateRoomAsync(Guid roomId);
}