using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Handles City-related operations such as retrieving, creating, and deleting cities.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/[controller]")]
[ApiController]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CitiesController> _logger;

    public CitiesController(ICityService cityService, IWebHostEnvironment environment, ILogger<CitiesController> logger)
    {
        _cityService = cityService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Get a city by its ID
    /// </summary>
    /// <param name="id">The ID of the city</param>
    /// <returns>The city with the given ID</returns>
    /// <response code="200">Returns the city with the given ID</response>
    /// <response code="404">If the city is not found</response>
    [AllowAnonymous]
    [HttpGet("{id}", Name = "GetCity")]
    public async Task<ActionResult<CityOutputWithHotelsAndImagesDto>> GetCity(Guid id)
    {
        _logger.LogInformation(APILogMessages.GetCityStart, id);
        CityOutputWithHotelsAndImagesDto? city = await _cityService.GetCityAsync(id);
        _logger.LogInformation(APILogMessages.GetCityComplete, id);

        return Ok(city);
    }

    /// <summary>
    /// Create a new city
    /// </summary>
    /// <param name="request">The data for the new city</param>
    /// <returns>Returns the newly created city</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /cities
    ///     {
    ///        "name": "Paris",
    ///        "country": "France",
    ///        "postOffice": "45651"
    ///     }
    /// </remarks>
    /// <response code="201">Returns the newly created city</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpPost]
    public async Task<ActionResult<CityOutputDto>> CreateCity(CreateCityDto request)
    {
        _logger.LogInformation(APILogMessages.CreateCityStart, request);
        CityOutputDto city = await _cityService.CreateCityAsync(request);
        _logger.LogInformation(APILogMessages.CreateCityComplete, request);

        return CreatedAtAction(nameof(GetCity), new { id = city.Id }, city);
    }

    /// <summary>
    /// Delete a city
    /// </summary>
    /// <param name="id">The ID of the city to delete</param>
    /// <returns>A success message with no content</returns>
    /// <response code="204">If the operation is successfully done</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCity(Guid id)
    {
        _logger.LogInformation(APILogMessages.DeleteCityStart, id);
        await _cityService.DeleteCityAsync(id);
        _logger.LogInformation(APILogMessages.DeleteCityComplete, id);

        return NoContent();
    }

    /// <summary>
    /// Update a city
    /// </summary>
    /// <param name="id">The ID of the city to update</param>
    /// <param name="request">The data for the updated city</param>
    /// <returns>A success message with no content</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /cities/{cityId}
    ///     {
    ///        "name": "Paris",
    ///        "country": "France",
    ///        "postOffice": "98745"
    ///     }
    /// </remarks>
    /// <response code="204">If the city is updated</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    /// <response code="404">If the city is not found</response>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCity(Guid id, UpdateCityDto request)
    {
        _logger.LogInformation(APILogMessages.UpdateCityStart, id, request);
        await _cityService.UpdateCityAsync(id, request);
        _logger.LogInformation(APILogMessages.UpdateCityComplete, id, request);

        return NoContent();
    }

    /// <summary>
    /// Retrieves the top N most visited cities, where N is 5 by default.
    /// </summary>
    /// <param name="count">The number of trending destinations to retrieve. Defaults to 5 if no value is provided.</param>
    /// <remarks>
    /// This endpoint provides a list of the most visited cities, serving as a curated selection of trending travel destinations.
    /// Each entry in the response includes key details such as:
    /// - The city's unique identifier.
    /// - The city's name.
    /// - A visually appealing thumbnail image.
    /// 
    /// The number of destinations retrieved can be adjusted by specifying the <paramref name="count"/> parameter. 
    /// If the parameter is omitted, the default value of 5 is used.
    /// </remarks>
    /// <returns>
    /// A collection of <see cref="CityAsTrendingDestinationOutputDto"/> objects, each representing a trending destination city.
    /// </returns>
    /// <response code="200">Returns the top N most visited cities, with N defaulting to 5 if not specified.</response>
    /// <seealso cref="CityAsTrendingDestinationOutputDto"/>
    [AllowAnonymous]
    [HttpGet("trending-destinations")]
    public async Task<ActionResult<IEnumerable<CityAsTrendingDestinationOutputDto>>> MostVisitedCities(int count = 5)
    {
        _logger.LogInformation(APILogMessages.MostVisitedCitiesStart, count);
        IEnumerable<CityAsTrendingDestinationOutputDto> cities = await _cityService.MostVisitedCitiesAsync(count);
        _logger.LogInformation(APILogMessages.MostVisitedCitiesComplete, count);

        return Ok(cities);
    }

    /// <summary>
    /// Upload an image to a city
    /// </summary>
    /// <param name="id">The ID of the city to upload the image</param>
    /// <param name="file">City image data</param>
    /// <param name="alternativeText">Alternative text (Alt)</param>
    /// <param name="thumbnail">Indicates if the image should be used as thumbnail</param>
    /// <returns>Returns a success response after the image upload</returns>
    /// <response code="204">If the image is successfully uploaded</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    /// <response code="404">If the city is not found</response>
    [HttpPost("{id}/images")]
    public async Task<ActionResult> UploadImage(Guid id, IFormFile file, string? alternativeText, bool? thumbnail = false)
    {
        _logger.LogInformation(APILogMessages.UploadImageStart, id);
        await _cityService.UploadImageAsync(id, file, _environment.WebRootPath, alternativeText, thumbnail);
        _logger.LogInformation(APILogMessages.UploadImageComplete, id);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated and optionally sorted list of cities based on the specified query parameters.
    /// </summary>
    /// <remarks>
    /// This endpoint allows clients to fetch cities with options for sorting, pagination, and filtering.
    /// The available query parameters include:
    /// - **SortOrder**: Specifies the sorting order (e.g., ascending or descending).
    /// - **SortColumn**: Specifies the column to sort by (e.g., name).
    /// - **PageNumber**: Specifies the page number for pagination.
    /// - **PageSize**: Specifies the number of items per page.
    /// - **SearchTerm**: Filters cities based on a search term that matches city names.
    ///
    /// Example usage:
    /// 
    ///     GET /cities?sortOrder=asc&amp;sortColumn=name&amp;pageNumber=1&amp;pageSize=10&amp;searchTerm=paris
    /// 
    /// The response includes paginated metadata in the `X-Pagination` header and links to related pages (e.g., next, previous).
    /// </remarks>
    /// <param name="request">The query parameters to filter, sort, and paginate the city list.</param>
    /// <returns>
    /// A collection of <see cref="CityOutputDto"/> objects that match the specified query parameters.
    /// </returns>
    /// <response code="200">Returns the list of cities that match the query parameters.</response>
    /// <response code="400">Returned if the provided query parameters are invalid.</response>
    [AllowAnonymous]
    [HttpGet(Name = "GetCities")]
    public async Task<ActionResult<IEnumerable<CityOutputDto>>> GetCities([FromQuery] GetCitiesQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.GetCitiesStart, request);
        HotelBookingPlatform.Application.DTOs.Common.PaginatedResult<CityOutputDto> paginatedResult = await _cityService.GetAllCitiesAsync(request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetCities), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetCitiesComplete, request);

        return Ok(paginatedResult.Data);
    }
}
