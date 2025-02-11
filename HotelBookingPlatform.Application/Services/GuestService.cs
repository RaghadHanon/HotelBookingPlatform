using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Guest;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GuestService> _logger;
    private readonly ICurrentUser _currentUser;

    public GuestService(
        IGuestRepository guestRepository,
        IBookingRepository bookingRepository,
        IHotelRepository hotelRepository,
        IMapper mapper,
        ICurrentUser currentUser,
        ILogger<GuestService> logger)
    {
        _guestRepository = guestRepository;
        _bookingRepository = bookingRepository;
        _hotelRepository = hotelRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<GuestOutputDto?> GetGuestAsync(Guid guestId)
    {
        var guest = await _guestRepository.GetGuestAsync(guestId);
        if (guest == null)
        {
            throw new NotFoundException(nameof(Guest), guestId);
        }
        var mapped = _mapper.Map<GuestOutputDto>(guest);

        return mapped;
    }

    public async Task<PaginatedResult<BookingForGuestOutputDto>> GetAllBookingsForGuestAsync(Guid guestId, GetBookingsQueryParametersDto request)
    {
        if (!await _guestRepository.GuestExistsAsync(guestId))
        {
            throw new NotFoundException(nameof(Guest), guestId);
        }

        var paginatedResult = await _guestRepository.GetAllBookingsForGuestAsync(guestId, request);
        var mapped = _mapper.Map<IEnumerable<BookingForGuestOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<BookingForGuestOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<IEnumerable<RecentlyVisitedHotelOutputDto>> GetRecentlyVisitedHotelsAsync(Guid guestId, int count = 5)
    {
        _logger.LogInformation(ApplicationLogMessages.GetRecentlyVisitedHotelsStarted, guestId, count);
        if (count <= 0 || count > 100)
        {
            throw new BadRequestException(string.Format(ServicesErrorMessages.InvalidHotelCount, count));
        }

        _logger.LogDebug(ApplicationLogMessages.CheckingGuestExistence, guestId);
        if (!await _guestRepository.GuestExistsAsync(guestId))
        {
            throw new NotFoundException(nameof(Guest), guestId);
        }

        _logger.LogDebug(ApplicationLogMessages.FetchingRecentBookings, guestId);
        var recentBookings = await _guestRepository.GetRecentBookingsInDifferentHotelsAsync(guestId, count);
        _logger.LogDebug(ApplicationLogMessages.MappingBookingsToOutputModel);
        var mapped = _mapper.Map<IEnumerable<RecentlyVisitedHotelOutputDto>>(recentBookings);

        return mapped;
    }

    public async Task<IEnumerable<RecentlyVisitedHotelOutputDto>> GetRecentlyVisitedHotelsForCurrentUserAsync(int count = 5)
    {
        _logger.LogInformation(ApplicationLogMessages.GetRecentlyVisitedHotelsForCurrentUserStarted, count);
        if (count <= 0 || count > 100)
        {
            throw new BadRequestException(string.Format(ServicesErrorMessages.InvalidHotelCount, count));
        }

        var guest = await GetGuestFromCurrentUser();
        _logger.LogDebug(ApplicationLogMessages.FetchingRecentBookings, guest.Id);
        var recentBookings = await _guestRepository.GetRecentBookingsInDifferentHotelsAsync(guest.Id, count);
        _logger.LogDebug(ApplicationLogMessages.MappingBookingsToOutputModel);
        var mapped = _mapper.Map<IEnumerable<RecentlyVisitedHotelOutputDto>>(recentBookings);
        _logger.LogInformation(ApplicationLogMessages.GetRecentlyVisitedHotelsForCurrentUserCompleted, count, guest.Id);

        return mapped;
    }

    private async Task<Guest> GetGuestFromCurrentUser()
    {
        _logger.LogDebug(ApplicationLogMessages.GettingUserIdFromCurrentUser);
        var userId = _currentUser.Id;
        _logger.LogDebug(ApplicationLogMessages.FetchingGuestByUserId, userId);
        var guest = await _guestRepository.GetGuestByUserIdAsync(userId)
            ?? throw new NotFoundException(nameof(Guest), userId);

        return guest;
    }
}
