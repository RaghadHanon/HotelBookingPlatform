using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Handles Amenity-related operations such as retrieving, creating, and deleting amenities.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/[controller]")]
[ApiController]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;
    private readonly ILogger<AmenitiesController> _logger;

    public AmenitiesController(IAmenityService amenityService, ILogger<AmenitiesController> logger)
    {
        _amenityService = amenityService;
        _logger = logger;
    }

    /// <summary>
    /// Get an amenity by its ID
    /// </summary>
    /// <param name="id">The ID of the amenity</param>
    /// <returns>Returns the amenity with the given ID</returns>
    /// <response code="200">Returns the amenity with the given ID</response>
    /// <response code="404">If the amenity is not found</response>
    [AllowAnonymous]
    [HttpGet("{id}", Name = "GetAmenity")]
    public async Task<ActionResult<AmenityOutputDto>> GetAmenity(Guid id)
    {
        _logger.LogInformation(APILogMessages.GetAmenityStarted, id);
        AmenityOutputDto amenity = await _amenityService.GetAmenity(id);
        _logger.LogInformation(APILogMessages.GetAmenityCompleted, id);

        return Ok(amenity);
    }

    /// <summary>
    /// Create a new amenity
    /// </summary>
    /// <param name="request">The data for the new amenity</param>
    /// <returns>The newly created amenity</returns>
    /// <remarks> 
    /// Sample request:
    /// 
    ///     POST /amenities
    ///     {
    ///        "name": "Smart home features",
    ///        "Description": "voice-controlled lighting, temperature"
    ///     }
    ///     
    /// </remarks>
    /// <response code="201">Returns the newly created amenity</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpPost]
    public async Task<ActionResult<AmenityOutputDto>> AddAmenity(CreateAndUpdateAmenityDto request)
    {
        _logger.LogInformation(APILogMessages.AddAmenityStarted, request);
        AmenityOutputDto amenity = await _amenityService.CreateAmenity(request);
        _logger.LogInformation(APILogMessages.AddAmenityCompleted, request);

        return CreatedAtAction(nameof(GetAmenity), new { id = amenity.Id }, amenity);
    }

    /// <summary>
    /// Update an amenity
    /// </summary>
    /// <param name="id">The ID of the amenity to update</param>
    /// <param name="request">The data for the updated amenity</param>
    /// <returns>No content</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /amenities/{amenityId}
    ///     {
    ///        "name": "Smart home features",
    ///        "Description": "voice-controlled lighting, temperature, Smoke detector and fire alarm"
    ///     }
    ///     
    /// </remarks>
    /// <response code="204">If the amenity is successfully updated</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    /// <response code="404">If the amenity is not found</response>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAmenity(Guid id, CreateAndUpdateAmenityDto request)
    {
        _logger.LogInformation(APILogMessages.UpdateAmenityStarted, request);
        await _amenityService.UpdateAmenity(id, request);
        _logger.LogInformation(APILogMessages.UpdateAmenityCompleted, request);

        return NoContent();
    }

    /// <summary>
    /// Delete an amenity
    /// </summary>
    /// <param name="id">The ID of the amenity to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the operation is successfully done</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAmenity(Guid id)
    {
        _logger.LogInformation(APILogMessages.DeleteAmenityStarted, id);
        await _amenityService.DeleteAmenityAsync(id);
        _logger.LogInformation(APILogMessages.DeleteBookingCompleted, id);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated and optionally sorted list of amenities based on the specified query parameters.
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
    ///     GET /amenities?sortOrder=asc&amp;sortColumn=name&amp;pageNumber=1&amp;pageSize=10&amp;searchTerm=SmartTV
    /// 
    /// The response includes paginated metadata in the `X-Pagination` header and links to related pages (e.g., next, previous).
    /// </remarks>
    /// <param name="request">The query parameters to filter, sort, and paginate the amenity list.</param>
    /// <returns>
    /// A collection of <see cref="AmenityOutputDto"/> objects that match the specified query parameters.
    /// </returns>
    /// <response code="200">Returns the list of amenities that match the query parameters.</response>
    /// <response code="400">Returned if the provided query parameters are invalid.</response>
    [AllowAnonymous]
    [HttpGet(Name = "GetAmenities")]
    public async Task<ActionResult<IEnumerable<AmenityOutputDto>>> GetAmenities([FromQuery] GetAmenitiesQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.GetAmenitiesStarted, request);
        Application.DTOs.Common.PaginatedResult<AmenityOutputDto> paginatedResult = await _amenityService.GetAllAmenitiesAsync(request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetAmenities), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetAmenitiesCompleted, request);

        return Ok(paginatedResult.Data);
    }
}
