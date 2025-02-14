using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Validators.Bookings;

public class BookingServiceValidator
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<BookingServiceValidator> _logger;

    public BookingServiceValidator(IRoomRepository roomRepository, ILogger<BookingServiceValidator> logger)
    {
        _roomRepository = roomRepository;
        _logger = logger;
    }

    public async Task ValidateRooms(CreateBookingDto request, List<Room> rooms, Hotel hotel)
    {
        foreach (Room room in rooms)
        {
            if (room == null)
            {
                throw new NotFoundException(nameof(Rooms));
            }

            ValidateRoomInHotel(room, hotel.Id);
            await ValidateRoomAvailability(room.Id, DateOnly.FromDateTime(request.CheckInDate), DateOnly.FromDateTime(request.CheckOutDate));
        }

        ValidateRoomsCapacity(request, rooms.Sum(r => r.AdultsCapacity), rooms.Sum(r => r.ChildrenCapacity));
    }

    private static void ValidateRoomInHotel(Room room, Guid hotelId)
    {
        if (room.HotelId != hotelId)
        {
            throw new BadRequestException(string.Format(ServicesErrorMessages.RoomDoesNotBelongToHotel, room.Id, hotelId));
        }
    }

    private async Task ValidateRoomAvailability(Guid roomId, DateOnly checkInDate, DateOnly checkOutDate)
    {
        _logger.LogDebug(ApplicationLogMessages.RoomAvailabilityCheck, roomId, checkInDate, checkOutDate);
        bool isRoomAvailable = await _roomRepository.IsAvailableAsync(roomId, checkInDate, checkOutDate);
        if (!isRoomAvailable)
        {
            throw new UnavailableRoomException(roomId, checkInDate, checkOutDate);
        }

        _logger.LogDebug(ApplicationLogMessages.RoomAvailabilitySuccess, roomId, checkInDate, checkOutDate);
    }

    private void ValidateRoomsCapacity(CreateBookingDto request, int totalAdultsCapacity, int totalChildrenCapacity)
    {
        if (totalAdultsCapacity < request.NumberOfAdults)
        {
            throw new InvalidNumberOfGuestsException(string.Format(ServicesErrorMessages.InsufficientAdultsCapacity, totalAdultsCapacity, request.NumberOfAdults));
        }

        if (totalChildrenCapacity < request.NumberOfChildren)
        {
            throw new InvalidNumberOfGuestsException(string.Format(ServicesErrorMessages.InsufficientChildrenCapacity, totalChildrenCapacity, request.NumberOfChildren));
        }

        _logger.LogDebug(ApplicationLogMessages.RoomsCapacityValidation, request);
    }
}
