using Asp.Versioning;
using HotelBookingPlatform.API.Utilities;
using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelBookingPlatform.API.Controllers;

/// <summary>
/// Handles Reviews-related operations such as retrieving, creating, and deleting reviews.
/// </summary>>
[ApiVersion("1.0")]
[Authorize(Policy = Policies.GuestOnly)]
[Route("api/hotels")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService,
                               ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;

    }

    /// <summary>
    /// Adds a review for a specific hotel.
    /// </summary>
    /// <param name="hotelId">The ID of the hotel for which the review is added.</param>
    /// <param name="request">The model containing review details.</param>
    /// <remarks>
    /// This endpoint allows users to submit a review for a particular hotel identified by the provided <paramref name="hotelId"/>.
    /// 
    /// Sample request:
    /// 
    ///     POST /hotels/{hotelId}/reviews
    ///     {
    ///         "Title": "Exceptional Hotel Experience",
    ///         "Description": "This hotel exceeded expectations with outstanding service, luxurious accommodations, and attention to detail. Highly recommended for a relaxing and memorable stay.",
    ///         "Rating": 5
    ///      }
    ///     
    /// </remarks>
    /// <returns>The newly created Review</returns>
    /// <response code="201">Review successfully added.</response>
    /// <response code="400">Invalid input or missing required fields.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized (not a guest, or didn't visit the hotel).</response>
    /// <response code="404">Hotel with the specified id not found.</response>
    [HttpPost("{hotelId}/reviews")]
    public async Task<ActionResult> AddReview(Guid hotelId, CreateOrUpdateReviewDto request)
    {
        _logger.LogInformation(APILogMessages.AddReviewStarted, hotelId, request);
        ReviewOutputDto review = await _reviewService.AddReviewAsync(hotelId, request);
        _logger.LogInformation(APILogMessages.AddReviewCompleted, hotelId, request);

        return CreatedAtAction(nameof(GetReview), new { hotelId = review.HotelId, reviewId = review.Id }, review);
    }

    /// <summary>
    /// Get a review by its ID
    /// </summary>
    /// <param name="hotelId">The ID of the hotel</param>
    /// <param name="reviewId">The id of the review</param>
    /// <returns>The review with the given ID</returns>
    /// <response code="200">Returns the review with the given ID</response>
    /// <response code="404">If the review is not found</response>
    [AllowAnonymous]
    [HttpGet("{hotelId}/reviews/{reviewId}", Name = "GetReview")]
    public async Task<ActionResult<ReviewOutputDto>> GetReview(Guid hotelId, Guid reviewId)
    {
        _logger.LogInformation(APILogMessages.GetReviewStarted, hotelId, reviewId);
        ReviewOutputDto review = await _reviewService.GetReviewAsync(hotelId, reviewId);
        _logger.LogInformation(APILogMessages.GetReviewCompleted, hotelId, reviewId);

        return Ok(review);
    }

    /// <summary>
    /// Get hotel average rating
    /// </summary>
    /// <param name="hotelId">The ID of the hotel</param>
    /// <returns>Hotel average user ratings</returns>
    /// <response code="200">Returns hotel average rating</response>
    /// <response code="404">If the hotel is not found</response>
    [AllowAnonymous]
    [HttpGet("{hotelId}/reviews/averageRating")]
    public async Task<ActionResult<double>> GetHotelAverageRating(Guid hotelId)
    {
        _logger.LogInformation(APILogMessages.GetHotelAverageRatingStarted, hotelId);
        double rating = await _reviewService.GetHotelAverageRatingAsync(hotelId);
        _logger.LogInformation(APILogMessages.GetHotelAverageRatingCompleted, hotelId);

        return Ok(new { rating });
    }

    /// <summary>
    /// Update a review
    /// </summary>
    /// <param name="hotelId">The ID of the hotel having the review to update</param>
    /// <param name="reviewId">The ID of the review to update</param>
    /// <param name="request">The data for the updated review</param>
    /// <returns>No content</returns>
    /// <remarks>
    /// 
    /// Sample request:
    /// 
    ///     PUT /hotels/{hotelId}/reviews/{reviewId}
    ///     {
    ///         "Title": "Exceptional Hotel Experience",
    ///         "Description": "This hotel exceeded expectations with outstanding service, luxurious accommodations, and attention to detail. Highly recommended for a relaxing and memorable stay.",
    ///         "Rating": 4
    ///     }
    ///     
    /// </remarks>
    /// <response code="204">If the review is successfully updated</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized (not a guest, or didn't visit the hotel).</response>
    /// <response code="404">If the hotel or review is not found</response>
    [HttpPut("{hotelId}/reviews/{reviewId}")]
    public async Task<ActionResult> UpdateReview(Guid hotelId, Guid reviewId, CreateOrUpdateReviewDto request)
    {
        _logger.LogInformation(APILogMessages.UpdateReviewStarted, hotelId, reviewId, request);
        await _reviewService.UpdateReviewAsync(hotelId, reviewId, request);
        _logger.LogInformation(APILogMessages.UpdateReviewCompleted, hotelId, reviewId, request);

        return NoContent();
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    /// <param name="hotelId">The ID of the hotel having the review to delete</param>
    /// <param name="reviewId">The ID of the review to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">If the operation is successfully done</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized (not a guest, or didn't visit the hotel).</response>
    [HttpDelete("{hotelId}/reviews/{reviewId}")]
    public async Task<ActionResult> DeleteReview(Guid hotelId, Guid reviewId)
    {
        _logger.LogInformation(APILogMessages.DeleteReviewStarted, hotelId, reviewId);
        await _reviewService.DeleteReviewAsync(hotelId, reviewId);
        _logger.LogInformation(APILogMessages.DeleteReviewCompleted, hotelId, reviewId);

        return NoContent();
    }


    /// <summary>
    /// Retrieves the reviews for a specific hotel based on the specified query parameters.
    /// </summary>
    /// <remarks>
    /// The retrieval of hotel reviews can be customized by providing various query parameters.
    /// These parameters include sorting options, page number, page size, and a search term.
    /// 
    /// Sample request:
    /// 
    ///     GET /hotels/{hotelId}/reviews?sortOrder=desc&amp;sortColumn=creationDate&amp;pageNumber=1&amp;pageSize=5&amp;searchQuery=Excellent
    ///     
    /// </remarks>
    /// <param name="hotelId">The ID of the hotel.</param>
    /// <param name="request">The query parameters for hotel review retrieval.</param>
    /// <returns>
    /// A collection of <see cref="ReviewOutputDto"/> objects, each representing a review that matches the specified criteria for the specified hotel.
    /// </returns>
    /// <response code="200">Returns the list of hotel reviews based on the query parameters.</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the hotel is not found.</response>
    [AllowAnonymous]
    [HttpGet("{hotelId}/reviews", Name = "GetHotelReviews")]
    public async Task<ActionResult<ReviewOutputDto>> GetHotelReviews(Guid hotelId, [FromQuery] GetHotelReviewsQueryParameters request)
    {
        _logger.LogInformation(APILogMessages.GetHotelReviewsStarted, hotelId, request);
        Application.DTOs.Common.PaginatedResult<ReviewOutputDto> paginatedResult = await _reviewService.GetHotelReviewsAsync(hotelId, request);
        PageLinkGenerator.AddPageLinks(Url, nameof(GetHotelReviews), paginatedResult.Metadata, request);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginatedResult.Metadata));
        _logger.LogInformation(APILogMessages.GetHotelReviewsCompleted, hotelId, request);

        return Ok(paginatedResult.Data);
    }
}
