using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Handles Rooms-related operations such as retrieving, creating, and deleting rooms.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(
        IRoomService roomService,
        IWebHostEnvironment environment,
        ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves details of a specific room by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the room.</param>
    /// <returns>An object of type <see cref="RoomWithFullDataOutputDto"/> containing details of the specified room.</returns>
    /// <response code="200">Returns the details of the requested room.</response>
    /// <response code="404">Indicates that the room with the specified ID was not found.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomWithFullDataOutputDto>> GetRoom(Guid id)
    {
        _logger.LogInformation(APILogMessages.FetchRoomDetails, id);
        RoomWithFullDataOutputDto? room = await _roomService.GetRoomAsync(id);
        _logger.LogInformation(APILogMessages.SuccessfullyFetchedRoomDetails, id);

        return Ok(room);
    }

    /// <summary>
    /// Creates a new room in the system.
    /// </summary>
    /// <param name="request">An object of type <see cref="CreateRoomDto"/> containing the details of the room to be created.</param>
    /// <returns>The details of the newly created room, returned as <see cref="RoomOutputDto"/>.</returns>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /rooms
    ///     {
    ///        "hotelId": "{hotelId}",
    ///        "roomNumber": "651",
    ///        "adultsCapacity": 2,
    ///        "childrenCapacity": 3,
    ///        "price": 250,
    ///        "roomType": "Standard"
    ///     }
    /// </remarks>
    /// <response code="201">Indicates that the room was successfully created.</response>
    /// <response code="400">Indicates invalid input data for room creation.</response>
    /// <response code="401">Indicates the user is not authenticated.</response>
    /// <response code="403">Indicates the user is not authorized to perform this action.</response>
    [HttpPost]
    public async Task<ActionResult<RoomOutputDto>> CreateRoom(CreateRoomDto request)
    {
        _logger.LogInformation(APILogMessages.CreatingRoom, request);
        RoomOutputDto room = await _roomService.CreateRoomAsync(request);
        _logger.LogInformation(APILogMessages.SuccessfullyCreatedRoom, room.Id);

        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    /// <summary>
    /// Deletes a room by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the room to be deleted.</param>
    /// <returns>A status indicating the result of the delete operation. No content will be returned.</returns>
    /// <response code="204">Indicates the room was successfully deleted.</response>
    /// <response code="401">Indicates the user is not authenticated.</response>
    /// <response code="403">Indicates the user is not authorized to delete the room.</response>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRoom(Guid id)
    {
        _logger.LogInformation(APILogMessages.DeletingRoom, id);
        await _roomService.DeleteRoomAsync(id);
        _logger.LogInformation(APILogMessages.SuccessfullyDeletedRoom, id);

        return NoContent();
    }

    /// <summary>
    /// Updates the details of an existing room.
    /// </summary>
    /// <param name="id">The unique identifier of the room to update.</param>
    /// <param name="request">An object of type <see cref="UpdateRoomDto"/> containing the updated room details.</param>
    /// <returns>A status indicating the result of the update operation. No content will be returned.</returns>
    /// <remarks>
    /// Example request:
    /// 
    ///     PUT /rooms/{roomId}
    ///     {
    ///        "roomNumber": "651",
    ///        "adultsCapacity": 2,
    ///        "childrenCapacity": 3,
    ///        "price": 250
    ///     }
    /// </remarks>
    /// <response code="204">Indicates the room was successfully updated.</response>
    /// <response code="400">Indicates invalid input data for room update.</response>
    /// <response code="404">Indicates that the room with the specified ID was not found.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateRoom(Guid id, UpdateRoomDto request)
    {
        _logger.LogInformation(APILogMessages.UpdatingRoom, id, request);
        await _roomService.UpdateRoomAsync(id, request);
        _logger.LogInformation(APILogMessages.SuccessfullyUpdatedRoom, id);

        return NoContent();
    }

    /// <summary>
    /// Uploads an image for a specific room.
    /// </summary>
    /// <param name="id">The unique identifier of the room.</param>
    /// <param name="file">The image file to be uploaded, represented as <see cref="IFormFile"/>.</param>
    /// <param name="alternativeText">Optional alternative text describing the image.</param>
    /// <param name="thumbnail">Specifies whether the image is a thumbnail.</param>
    /// <returns>A status indicating the result of the upload operation. No content will be returned.</returns>
    /// <response code="204">Indicates the image was successfully uploaded.</response>
    /// <response code="400">Indicates invalid input data for the image upload.</response>
    /// <response code="404">Indicates that the room with the specified ID was not found.</response>
    [HttpPost("{id}/images")]
    public async Task<ActionResult> UploadImage(Guid id, IFormFile file, string? alternativeText, bool? thumbnail = false)
    {
        _logger.LogInformation(APILogMessages.UploadingImage, id);
        await _roomService.UploadImageAsync(id, file, _environment.WebRootPath, alternativeText, thumbnail);
        _logger.LogInformation(APILogMessages.SuccessfullyUploadedImage, id);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated list of rooms based on query parameters.
    /// </summary>
    /// <param name="request">An object of type <see cref="GetRoomsQueryParametersDto"/> containing query parameters for filtering, sorting, and pagination.</param>
    /// <returns>A paginated list of rooms, represented as <see cref="IEnumerable{RoomOutputDto}"/> matching the criteria.</returns>
    /// <remarks>
    /// Example request:
    /// 
    ///     GET /rooms?sortOrder=asc&amp;sortColumn=price&amp;pageNumber=1&amp;pageSize=10&amp;searchQuery=Ritz
    /// </remarks>
    /// <response code="200">Returns a list of rooms matching the query parameters.</response>
    /// <response code="400">Indicates invalid query parameters.</response>
    [AllowAnonymous]
    [HttpGet(Name = "GetRooms")]
    public async Task<ActionResult<IEnumerable<RoomOutputDto>>> GetRooms([FromQuery] GetRoomsQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.FetchingRoomsWithQuery, request);
        HotelBookingPlatform.Application.DTOs.Common.PaginatedResult<RoomOutputDto> paginatedResult = await _roomService.GetAllRoomsAsync(request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetRooms), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.SuccessfullyFetchedRoomsWithQuery, request);

        return Ok(paginatedResult.Data);
    }

    /// <summary>
    /// add an amenity to a room
    /// </summary>
    /// <param name="roomId">The id of the room to add amenity to</param>
    /// <param name="amenityId">The id of the amenity to be added</param>
    /// <returns>No content</returns>
    /// <response code="201">if the amenity is successfully added</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized (not an admin)</response>
    [HttpPost("{roomId}/amenities/{amenityId}")]
    public async Task<ActionResult<AmenityOutputDto>> AddAmenity(Guid roomId, Guid amenityId)
    {
        //_logger.LogInformation(LogMessages.AddAmenityStarted, request);
        bool amenity = await _roomService.AddAmenityToRoomAsync(roomId, amenityId);
        //_logger.LogInformation(LogMessages.AddAmenityCompleted, request);

        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated and optionally sorted list of amenities for a room based on the specified query parameters.
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
    ///     GET /hotels/{roomId}/amenities?sortOrder=asc&amp;sortColumn=name&amp;pageNumber=1&amp;pageSize=10&amp;searchTerm=SmartTV
    /// 
    /// The response includes paginated metadata in the `X-Pagination` header and links to related pages (e.g., next, previous).
    /// </remarks>
    /// <param name="roomId"></param>
    /// <param name="request">The query parameters to filter, sort, and paginate the amenity list.</param>
    /// <returns>
    /// A collection of <see cref="AmenityOutputDto"/> objects that match the specified query parameters.
    /// </returns>
    /// <response code="200">Returns the list of amenities that match the query parameters.</response>
    /// <response code="400">Returned if the provided query parameters are invalid.</response>
    [AllowAnonymous]
    [HttpGet("{roomId}/amenities", Name = "GetRoomAmenities")]
    public async Task<ActionResult<IEnumerable<AmenityOutputDto>>> GetRoomAmenities(Guid roomId, [FromQuery] GetAmenitiesQueryParametersDto request)
    {
        _logger.LogInformation(APILogMessages.GetAmenitiesStarted, request);
        HotelBookingPlatform.Application.DTOs.Common.PaginatedResult<AmenityOutputDto> paginatedResult = await _roomService.GetAmenitiesForHotelAsync(roomId, request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetRoomAmenities), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetAmenitiesCompleted, request);

        return Ok(paginatedResult.Data);
    }
}
