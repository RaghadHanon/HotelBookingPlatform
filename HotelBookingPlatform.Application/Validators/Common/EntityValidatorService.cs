using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Domain.Models;

public class EntityValidatorService : IEntityValidatorService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IAmenityRepository _amenityRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IDiscountRepository _discountRepository;

    public EntityValidatorService(
        IHotelRepository hotelRepository,
        IRoomRepository roomRepository,
        IGuestRepository guestRepository,
        ICityRepository cityRepository,
        IAmenityRepository amenityRepository,
        IBookingRepository bookingRepository,
        IReviewRepository reviewRepository,
        IDiscountRepository discountRepository)
    {
        _hotelRepository = hotelRepository;
        _roomRepository = roomRepository;
        _guestRepository = guestRepository;
        _cityRepository = cityRepository;
        _amenityRepository = amenityRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _discountRepository = discountRepository;
    }

    public async Task<Hotel> ValidateHotelAsync(Guid hotelId)
    {
        var hotel = await _hotelRepository.GetHotelAsync(hotelId);
        return hotel ?? throw new NotFoundException(nameof(Hotel), hotelId);
    }

    public async Task<Room> ValidateRoomAsync(Guid roomId)
    {
        var room = await _roomRepository.GetRoomAsync(roomId);
        return room ?? throw new NotFoundException(nameof(Room), roomId);
    }

    public async Task<Guest> ValidateGuestAsync(Guid guestId)
    {
        var guest = await _guestRepository.GetGuestAsync(guestId);
        return guest ?? throw new NotFoundException(nameof(Guest), guestId);
    }

    public async Task<City> ValidateCityAsync(Guid cityId)
    {
        var city = await _cityRepository.GetCityAsync(cityId);
        return city ?? throw new NotFoundException(nameof(City), cityId);
    }

    public async Task<Amenity> ValidateAmenityAsync(Guid amenityId)
    {
        var amenity = await _amenityRepository.GetAmenityAsync(amenityId);
        return amenity ?? throw new NotFoundException(nameof(Amenity), amenityId);
    }

    public async Task<Booking> ValidateBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetBookingAsync(bookingId);
        return booking ?? throw new NotFoundException(nameof(Booking), bookingId);
    }

    public async Task<Review> ValidateReviewAsync(Hotel hotel, Guid reviewId)
    {
        var review = await _reviewRepository.GetReviewAsync(hotel, reviewId);
        return review ?? throw new NotFoundException(nameof(Review), reviewId);
    }

    public async Task<Discount> ValidateDiscountAsync(Guid roomId, Guid discountId)
    {
        var discount = await _discountRepository.GetDiscountAsync(roomId, discountId);
        return discount ?? throw new NotFoundException(nameof(Discount), discountId);
    }
}
