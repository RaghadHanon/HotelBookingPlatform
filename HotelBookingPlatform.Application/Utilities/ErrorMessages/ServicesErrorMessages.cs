namespace HotelBookingPlatform.Application.Utilities.ErrorMessages;

public static class ServicesErrorMessages
{
    public const string InvalidNumberOfCities = "Invalid parameter: {0}. Number of cities must be between 1 and 100";
    public const string ErrorWithUploadingImage = "An error occurred while uplooding the image";
    public const string ErrorDiscountedPriceOrPercentage = "Either a discounted price or a discount percentage must be provided";
    public const string ErrorInvalidNumberOfDeals = "Invalid number of deals: {0}, The number of deals must be between 1 and 20";
    public const string InvalidHotelCount = "Invalid hotel count: {0}. The count must be between 1 and 100.";
    public const string InvalidFilePath = "Invalid or empty base path provided.";
    public const string GuestNotBooked = "Guest has not booked this hotel";
    public const string GuestAlreadyReviewed = "Guest has already reviewed this hotel";
    public const string CannotDeleteStartedBooking = "Cannot delete a booking that has started";
    public const string RoomsCannotBeNull = "Rooms cannot be null";
    public const string RoomDoesNotBelongToHotel = "Room with ID: {0} does not belong to hotel with ID: {1}";
    public const string ErrorDuringBookingCreation = "Error occurred during booking creation.";
    public const string InsufficientAdultsCapacity = "Total adult capacity ({0}) is less than the number of adults ({1}).";
    public const string InsufficientChildrenCapacity = "Total children capacity ({0}) is less than the number of children ({1}).";
}
