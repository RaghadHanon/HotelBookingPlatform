namespace HotelBookingPlatform.Application.DTOs.Review;

/// <summary>
/// DTO for creating or updating a review
/// </summary>
public class CreateOrUpdateReviewDto
{
    /// <summary>
    /// Title of the review
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Content of the review (optional)
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Rating for the hotel (1-5)
    /// </summary>
    public int Rating { get; set; }
}
