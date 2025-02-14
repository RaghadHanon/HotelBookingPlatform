using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Handles Discount-related operations such as retrieving, creating, and deleting discounts.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/rooms")]
[ApiController]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;
    private readonly ILogger<DiscountsController> _logger;

    public DiscountsController(
        IDiscountService discountService,
        ILogger<DiscountsController> logger)
    {
        _discountService = discountService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new discount
    /// </summary>
    /// <param name="roomId">The ID of the room</param>
    /// <param name="request">The data for the new discount</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /rooms/{roomId}/discounts
    ///     {
    ///        "Percentage": 15
    ///        "StartDate": "2025-01-0200:00:00",
    ///        "EndDate": "2025-01-05T00:00:00"
    ///     }
    ///
    /// </remarks>
    /// <returns>The newly created discount</returns>
    /// <response code="201">Returns the newly created discount</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized (not an admin).</response>
    [HttpPost("{roomId}/discounts")]
    public async Task<ActionResult<DiscountOutputDto>> AddDiscount(Guid roomId, CreateDiscountDto request)
    {
        _logger.LogInformation(APILogMessages.AddDiscountStarted, roomId, request);
        DiscountOutputDto discount = await _discountService.CreateDiscountAsync(roomId, request);
        _logger.LogInformation(APILogMessages.AddDiscountCompleted, roomId, request);

        return CreatedAtAction(nameof(GetDiscount), new { roomId = discount.RoomId, id = discount.Id }, discount);
    }

    /// <summary>
    /// Get a discount by its ID
    /// </summary>
    /// <param name="roomId">The ID of the room</param>
    /// <param name="id">The ID of the discount</param>
    /// <returns>The discount with the given ID</returns>
    /// <response code="200">Returns the discount with the given ID</response>
    /// <response code="404">If the discount is not found</response>
    [AllowAnonymous]
    [HttpGet("{roomId}/discounts/{id}", Name = "GetDiscount")]
    public async Task<ActionResult<DiscountOutputDto>> GetDiscount(Guid roomId, Guid id)
    {
        _logger.LogInformation(APILogMessages.GetDiscountStarted, roomId, id);
        DiscountOutputDto? discount = await _discountService.GetDiscountAsync(roomId, id);
        _logger.LogInformation(APILogMessages.GetDiscountCompleted, roomId, id);

        return Ok(discount);
    }

    /// <summary>
    /// Get discounts for a room by its ID
    /// </summary>
    /// <param name="roomId">The ID of the room</param>
    /// <param name="request"></param>
    /// <returns>The discounts for a room with givin ID</returns>
    /// <response code="200">Returns the discounts for a room with givin ID</response>
    [AllowAnonymous]
    [HttpGet("{roomId}/discounts", Name = "GetDiscounts")]
    public async Task<ActionResult<DiscountForRoomOutputDto>> GetDiscounts(Guid roomId, [FromQuery] GetDiscountsQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.GetDiscountsStarted, roomId);
        Application.DTOs.Common.PaginatedResult<DiscountForRoomOutputDto> paginatedResult = await _discountService.GetDiscountsForRoomAsync(roomId, request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetDiscounts), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetDiscountsCompleted, roomId);

        return Ok(paginatedResult.Data);
    }

    /// <summary>
    /// Delete a discount
    /// </summary>
    /// <param name="roomId">The ID of the room</param>
    /// <param name="id">The ID of the discount</param>
    /// <returns></returns>
    /// <response code="204">If the operation is successfully done</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized (not an admin).</response>
    [HttpDelete("{roomId}/discounts/{id}")]
    public async Task<ActionResult> DeleteDiscount(Guid roomId, Guid id)
    {
        _logger.LogInformation(APILogMessages.DeleteDiscountStarted, roomId, id);
        await _discountService.DeleteDiscountAsync(roomId, id);
        _logger.LogInformation(APILogMessages.DeleteDiscountCompleted, roomId, id);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a collection of featured deals based on the specified count.
    /// </summary>
    /// <remarks>
    /// This endpoint allows clients to retrieve a curated list of featured deals.
    /// The response includes essential details for each deal,
    /// such as its hotel name, room type, star rating, discount percentage, and discounted price.
    /// 
    /// The number of featured deals to be retrieved can be specified
    /// using the <paramref name="deals"/> parameter. If no count is provided, the default is set to 5.
    /// 
    /// Sample request:
    /// 
    ///     GET /featured-deals?deals=3
    ///     
    /// </remarks>
    /// <param name="deals">The number of featured deals to retrieve. Default is 5.</param>
    /// <returns>
    /// A collection of <see cref="FeaturedDealOutputDto"/> objects, each representing a featured deal.
    /// </returns>
    /// <response code="200">Returns the collection of featured deals.</response>
    [AllowAnonymous]
    [HttpGet("featured-deals")]
    public async Task<ActionResult<IEnumerable<FeaturedDealOutputDto>>> GetFeaturedDeals([FromQuery] int deals = 5)
    {
        _logger.LogInformation(APILogMessages.GetFeaturedDealsStarted, deals);
        IEnumerable<FeaturedDealOutputDto> featuredDeals = await _discountService.GetFeaturedDealsAsync(deals);
        _logger.LogInformation(APILogMessages.GetFeaturedDealsCompleted, deals);

        return Ok(featuredDeals);
    }
}
