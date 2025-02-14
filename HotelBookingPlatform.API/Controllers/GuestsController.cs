using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Guest;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Handles Guests-related operations such as retrieving, creating, and deleting guests.
/// </summary>

[ApiVersion("1.0")]
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly ILogger<GuestsController> _logger;

    public GuestsController(IGuestService guestService, ILogger<GuestsController> logger)
    {
        _guestService = guestService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves details of a guest by their ID.
    /// </summary>
    /// <param name="guestId">The ID of the guest to retrieve.</param>
    /// <returns>The guest details.</returns>
    /// <response code="200">Returns the guest details.</response>
    /// <response code="404">If the guest is not found.</response>
    [HttpGet("{guestId}")]
    public async Task<ActionResult<GuestOutputDto>> GetGuest(Guid guestId)
    {
        _logger.LogInformation(APILogMessages.GetGuestStarted, guestId);
        GuestOutputDto? guest = await _guestService.GetGuestAsync(guestId);
        _logger.LogInformation(APILogMessages.GetGuestCompleted, guestId);

        return Ok(guest);
    }

    /// <summary>
    /// Retrieves all bookings for a specific guest.
    /// </summary>
    /// <param name="guestId">The ID of the guest.</param>
    /// <param name="request"></param>
    /// <returns>A collection of bookings for the guest.</returns>
    [HttpGet("{guestId}/bookings")]
    public async Task<ActionResult<IEnumerable<BookingForGuestOutputDto>>> GetBookingsForGuest(Guid guestId, [FromQuery] GetBookingsQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.GetBookingsStarted, request);
        Application.DTOs.Common.PaginatedResult<BookingForGuestOutputDto> paginatedResult = await _guestService.GetAllBookingsForGuestAsync(guestId, request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetBookingsForGuest), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetBookingsCompleted, request);

        return Ok(paginatedResult.Data);
    }

    /// <summary>
    /// Retrieves a collection of unique recently visited hotels for a guest, presenting essential details.
    /// </summary>
    /// <param name="guestId">The ID of the guest for whom recently visited hotels are to be retrieved.</param>
    /// <param name="count">The maximum number of unique recently visited hotels to retrieve. Default is 5.</param>
    /// <returns>
    /// A collection of <see cref="RecentlyVisitedHotelOutputDto"/> objects, each representing an hotel the user recently visited
    /// </returns>
    /// <remarks>
    /// The resulting collection provides essential information about the last N different hotels the guest visited, such as hotel name, city name, star rating, and price.
    /// 
    /// Sample request:
    /// 
    ///     GET guests/{guestId}/recently-visited-hotels?count=3
    ///     
    /// </remarks>
    /// <response code="200">Returns the last 5 different hotels the guest visited</response>>
    /// <response code="404">If the guest is not found</response>
    [AllowAnonymous]
    [HttpGet("{guestId}/recently-visited-hotels")]
    public async Task<ActionResult<IEnumerable<RecentlyVisitedHotelOutputDto>>> GetRecentlyVisitedHotels(Guid guestId, int count = 5)
    {
        _logger.LogInformation(APILogMessages.GetRecentlyVisitedHotelsStarted, guestId, count);
        IEnumerable<RecentlyVisitedHotelOutputDto> hotels = await _guestService.GetRecentlyVisitedHotelsAsync(guestId, count);
        _logger.LogInformation(APILogMessages.GetRecentlyVisitedHotelsCompleted, guestId, count);

        return Ok(hotels);
    }

    /// <summary>
    /// Retrieves a collection of unique recently visited hotels for a the current authorized guest, presenting essential details.
    /// </summary>
    /// <param name="count">The maximum number of unique recently visited hotels to retrieve. Default is 5.</param>
    /// <remarks>
    /// The resulting collection provides essential information about the last N different hotels the guest visited, such as hotel name, city name, star rating, and price.
    /// 
    /// Sample request:
    /// 
    ///     GET guests/{guestId}/recently-visited-hotels?count=3
    ///     
    /// </remarks>
    /// <returns>
    /// A collection of <see cref="RecentlyVisitedHotelOutputDto"/> objects, each representing an hotel the user recently visited
    /// </returns>
    /// <response code="200">Returns the last 5 different hotels the guest visited</response>>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">If the guest is not found</response>
    [Authorize(Policy = Policies.GuestOnly)]
    [HttpGet("recently-visited-hotels")]
    public async Task<ActionResult<IEnumerable<RecentlyVisitedHotelOutputDto>>> GetRecentlyVisitedHotels(int count = 5)
    {
        _logger.LogInformation(APILogMessages.GetRecentlyVisitedHotelsCurrentUserStarted, count);
        IEnumerable<RecentlyVisitedHotelOutputDto> hotels = await _guestService.GetRecentlyVisitedHotelsForCurrentUserAsync(count);
        _logger.LogInformation(APILogMessages.GetRecentlyVisitedHotelsCurrentUserCompleted, count);

        return Ok(hotels);
    }
}
