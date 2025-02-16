using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Guest;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GuestService> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IEntityValidatorService _entityValidatorService;

    public GuestService(
        IGuestRepository guestRepository,
        IMapper mapper,
        ICurrentUser currentUser,
        ILogger<GuestService> logger,
        IEntityValidatorService entityValidatorService)
    {
        _guestRepository = guestRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _logger = logger;
        _entityValidatorService = entityValidatorService;
    }

    public async Task<GuestOutputDto?> GetGuestAsync(Guid guestId)
    {
        var guest = await _entityValidatorService.ValidateGuestAsync(guestId);
        var mapped = _mapper.Map<GuestOutputDto>(guest);

        return mapped;
    }

    public async Task<PaginatedResult<BookingForGuestOutputDto>> GetAllBookingsForGuestAsync(Guid guestId, GetBookingsQueryParametersDto request)
    {
        var guest = await _entityValidatorService.ValidateGuestAsync(guestId);
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
        var guest = await _entityValidatorService.ValidateGuestAsync(guestId);
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

        var currentGuestDto = await GetGuestFromCurrentUser();
        _logger.LogDebug(ApplicationLogMessages.FetchingRecentBookings, currentGuestDto.Guest.Id);
        var recentBookings = await _guestRepository.GetRecentBookingsInDifferentHotelsAsync(currentGuestDto.Guest.Id, count);
        _logger.LogDebug(ApplicationLogMessages.MappingBookingsToOutputModel);
        var mapped = _mapper.Map<IEnumerable<RecentlyVisitedHotelOutputDto>>(recentBookings);
        _logger.LogInformation(ApplicationLogMessages.GetRecentlyVisitedHotelsForCurrentUserCompleted, count, currentGuestDto.Guest.Id);

        return mapped;
    }

    public async Task<CurrentGuestDto> GetGuestFromCurrentUser()
    {
        _logger.LogDebug(ApplicationLogMessages.GettingUserIdFromCurrentUser);
        var userId = _currentUser.Id;
        _logger.LogDebug(ApplicationLogMessages.FetchingGuestByUserId, userId);
        var guest = await _guestRepository.GetGuestByUserIdAsync(userId)
            ?? throw new NotFoundException(nameof(Guest), userId);

        return new CurrentGuestDto { Guest = guest, UserId = userId, Email = _currentUser.Email };
    }
}
