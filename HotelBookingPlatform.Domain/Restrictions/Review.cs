namespace HotelBookingPlatform.Domain.Restrictions;

public class Review
{
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const int MinReviewTitleLength = 2;
    public const int MaxReviewTitleLength = 50;
    public const int MinReviewDescriptionLength = 2;
    public const int MaxReviewDescriptionLength = 1000;
}