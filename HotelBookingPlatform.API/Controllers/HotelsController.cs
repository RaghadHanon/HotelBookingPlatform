using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Handles Hotel-related operations such as retrieving, creating, and deleting hotels.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HotelsController> _logger;

    public HotelsController(IHotelService hotelService, IWebHostEnvironment environment, ILogger<HotelsController> logger)
    {
        _hotelService = hotelService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Get a hotel by its ID
    /// </summary>
    /// <param name="id">The ID of the hotel</param>
    /// <returns>Returns the hotel with the given ID</returns>
    /// <response code="200">Returns the hotel with the given ID</response>
    /// <response code="404">If the hotel is not found</response>
    [AllowAnonymous]
    [HttpGet("{id}", Name = "GetHotel")]
    public async Task<ActionResult<HotelWithFullDataOutputDto>> GetHotel(Guid id)
    {
        _logger.LogInformation(APILogMessages.GetHotelStarted, id);
        HotelWithFullDataOutputDto? hotel = await _hotelService.GetHotelAsync(id);
        _logger.LogInformation(APILogMessages.GetHotelCompleted, id);

        return Ok(hotel);
    }

    /// <summary>
    /// Create a new hotel
    /// </summary>
    /// <param name="request">The data for the new hotel</param>
    /// <returns>The newly created hotel</returns>
    /// <remarks> 
    /// Sample request:
    /// 
    ///     POST /hotels
    ///     {
    ///        "name": "Paris Hotel",
    ///        "owner": "John Smith",
    ///        "street": "233 st.",
    ///        "location": {
    ///           "latitude": "10.2",
    ///           "longitude": "47.1"
    ///        },
    ///        "cityId": "{cityId}"
    ///        "starRate" : "5",
    ///        "checkInTime": "13:00:00.0000000",
    ///        "checkOutTime": "12:00:00.0000000"
    ///     }
    ///     
    /// </remarks>
    /// <response code="201">Returns the newly created hotel</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpPost]
    public async Task<ActionResult<HotelOutputDto>> CreateHotel(CreateHotelDto request)
    {
        _logger.LogInformation(APILogMessages.CreateHotelStarted, request);
        HotelOutputDto hotel = await _hotelService.CreateHotelAsync(request);
        _logger.LogInformation(APILogMessages.CreateHotelCompleted, request);

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    /// <summary>
    /// Delete a hotel
    /// </summary>
    /// <param name="id">The ID of the hotel to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the operation is successfully done</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHotel(Guid id)
    {
        _logger.LogInformation(APILogMessages.DeleteHotelStarted, id);
        await _hotelService.DeleteHotelAsync(id);
        _logger.LogInformation(APILogMessages.DeleteHotelCompleted, id);

        return NoContent();
    }

    /// <summary>
    /// Update a hotel
    /// </summary>
    /// <param name="id">The ID of the hotel to update</param>
    /// <param name="request">The data for the updated hotel</param>
    /// <returns>No content</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /hotels/{hotelId}
    ///     {
    ///        "cityId": "{cityId}"
    ///        "name": "Paris Hotel",
    ///        "owner": "John Smith",
    ///        "street": "230 st.",
    ///        "location": {
    ///           "latitude": "17.2",
    ///           "longitude": "47.1"
    ///        }
    ///     }
    /// </remarks>
    /// <response code="204">If the hotel is successfully updated</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    /// <response code="404">If the hotel is not found</response>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHotel(Guid id, UpdateHotelDto request)
    {
        _logger.LogInformation(APILogMessages.UpdateHotelStarted, id, request);
        await _hotelService.UpdateHotelAsync(id, request);
        _logger.LogInformation(APILogMessages.UpdateHotelCompleted, id, request);

        return NoContent();
    }

    /// <summary>
    /// Upload an image to a hotel
    /// </summary>
    /// <param name="id">The id of the hotel to upload image</param>
    /// <param name="file">Hotel image data</param>
    /// <param name="alternativeText">Alternative Text (Alt)</param>
    /// <param name="thumbnail">Indicates if the image should be used as a thumbnail</param>
    /// <response code="204">If the image is successfully uploaded</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    /// <response code="404">If the hotel is not found</response>
    [HttpPost("{id}/images")]
    public async Task<ActionResult> UploadImage(Guid id, IFormFile file, string? alternativeText, bool? thumbnail = false)
    {
        _logger.LogInformation(APILogMessages.UploadImageStarted, id);
        await _hotelService.UploadImageAsync(id, file, _environment.WebRootPath, alternativeText, thumbnail);
        _logger.LogInformation(APILogMessages.UploadImageCompleted, id);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a list of hotels based on the specified query parameters.
    /// </summary>
    /// <remarks>
    /// The retrieval of hotels can be customized by providing various query parameters.
    /// These parameters include sorting options, page number, page size, and a search term.
    /// 
    /// Sample request:
    /// 
    ///     GET /hotels?sortOrder=asc&amp;sortColumn=name&amp;pageNumber=1&amp;pageSize=10&amp;searchQuery=Carlton
    /// </remarks>
    /// <param name="request">The query parameters for hotel retrieval.</param>
    /// <returns>
    /// A collection of <see cref="HotelOutputDto"/> objects, each representing a hotel that matches the specified criteria.
    /// </returns>
    /// <response code="200">Returns the list of hotels based on the query parameters.</response>
    /// <response code="400">If the request parameters are invalid or missing.</response>
    [AllowAnonymous]
    [HttpGet(Name = "GetHotels")]
    public async Task<ActionResult<IEnumerable<HotelOutputDto>>> GetHotels([FromQuery] GetHotelsQueryParametersDto request)
    {

        _logger.LogInformation(APILogMessages.GetHotelsStarted, request);
        Application.DTOs.Common.PaginatedResult<HotelOutputDto> paginatedResult = await _hotelService.GetAllHotelsAsync(request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetHotels), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetHotelsCompleted, request);

        return Ok(paginatedResult.Data);
    }

    /// <summary>
    /// Searches and filters hotels based on the specified criteria.
    /// </summary>
    /// <remarks>
    /// Performs a search based on a query string and optional parameters, allowing users to refine results.
    /// Parameters include:
    /// - Check-in and check-out dates
    /// - Number of adults, children, and rooms
    /// Additional filters can be applied, such as:
    /// - Minimum star rating
    /// - Price range (minimum and maximum)
    /// - Amenities
    /// - Room types
    /// These options help narrow down the search to provide more tailored results.
    /// 
    /// Sample request:
    /// 
    ///     GET /hotels/search?searchTerm=Tokyo&amp;maxPrice=100&amp;minStarRating=4&amp;amenities=FreeWifi&amp;roomTypes=Standard
    /// </remarks>
    /// <param name="request">The query parameters for hotel search and filtering.</param>
    /// <returns>
    /// A collection of <see cref="HotelSearchResultOutputDto"/> representing the search results.
    /// </returns>
    /// <response code="200">Returns the list of hotels based on the search criteria.</response>
    /// <response code="400">If the request parameters are invalid or missing.</response>
    [AllowAnonymous]
    [HttpGet("Search-Filter", Name = "SearchAndFilterHotels")]
    public async Task<ActionResult<IEnumerable<HotelSearchResultOutputDto>>> SearchAndFilterHotels([FromQuery] HotelSearchAndFilterParametersDto request)
    {
        _logger.LogInformation(APILogMessages.SearchAndFilterHotelsStarted, request);
        Application.DTOs.Common.PaginatedResult<HotelSearchResultOutputDto> paginatedResult = await _hotelService.SearchAndFilterHotelsAsync(request);
        PageLinkGenerator.AddPageLinks(Url, nameof(SearchAndFilterHotels), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.SearchAndFilterHotelsCompleted, request);

        return Ok(paginatedResult.Data);
    }

    /// <summary>
    /// add an amenity to a hotel
    /// </summary>
    /// <param name="hotelId">The id of the hotel to add amenity to</param>
    /// <param name="amenityId">The id of the amenity to be added</param>
    /// <returns>No content</returns>
    /// <response code="201">if the amenity is successfully added</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpPost("{hotelId}/amenities/{amenityId}")]
    public async Task<ActionResult<AmenityOutputDto>> AddAmenity(Guid hotelId, Guid amenityId)
    {
        //_logger.LogInformation(LogMessages.AddAmenityStarted, request);
        bool amenity = await _hotelService.AddAmenityToHotelAsync(hotelId, amenityId);
        //_logger.LogInformation(LogMessages.AddAmenityCompleted, request);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated and optionally sorted list of amenities for a hotel based on the specified query parameters.
    /// </summary>
    /// <remarks>
    /// This endpoint allows clients to fetch amenities with options for sorting, pagination, and filtering.
    /// The available query parameters include:
    /// - **SortOrder**: Specifies the sorting order (e.g., ascending or descending).
    /// - **SortColumn**: Specifies the column to sort by (e.g., name, description).
    /// - **PageNumber**: Specifies the page number for pagination.
    /// - **PageSize**: Specifies the number of items per page.
    /// - **SearchTerm**: Filters amenities based on a search term that matches amenity name or Description.
    ///
    /// Example usage:
    /// 
    ///     GET /hotels/{hotelId}/amenities?sortOrder=asc&amp;sortColumn=name&amp;pageNumber=1&amp;pageSize=10&amp;searchTerm=SmartTV
    /// 
    /// The response includes paginated metadata in the `X-Pagination` header and links to related pages (e.g., next, previous).
    /// </remarks>
    /// <param name="hotelId"></param>
    /// <param name="request">The query parameters to filter, sort, and paginate the amenity list.</param>
    /// <returns>
    /// A collection of <see cref="AmenityOutputDto"/> objects that match the specified query parameters.
    /// </returns>
    /// <response code="200">Returns the list of amenities that match the query parameters.</response>
    /// <response code="400">Returned if the provided query parameters are invalid.</response>
    [AllowAnonymous]
    [HttpGet("{hotelId}/amenities", Name = "GetHotelAmenities")]
    public async Task<ActionResult<IEnumerable<AmenityOutputDto>>> GetHotelAmenities(Guid hotelId, [FromQuery] GetAmenitiesQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.GetAmenitiesStarted, request);
        Application.DTOs.Common.PaginatedResult<AmenityOutputDto> paginatedResult = await _hotelService.GetAmenitiesForHotelAsync(hotelId, request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetHotelAmenities), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetAmenitiesCompleted, request);

        return Ok(paginatedResult.Data);
    }
}
