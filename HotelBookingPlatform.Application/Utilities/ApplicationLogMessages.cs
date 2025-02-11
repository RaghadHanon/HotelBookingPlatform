namespace HotelBookingPlatform.Application.Utilities;

public static class ApplicationLogMessages
{
    // CityService-related messages
    public const string MostVisitedCitiesStarted = "MostVisitedCitiesAsync started with count: {count}";
    public const string ValidatingMostVisitedCitiesCount = "Validating {countMostVisited}";
    public const string NoCitiesRetrieved = "No Cities were retrieved from the repository at MostVisitedCitiesAsync";
    public const string MappingCitiesToOutputModel = "Mapping the City Entities to CityAsTrendingDestinationOutputModel";
    public const string MostVisitedCitiesCompleted = "MostVisitedCitiesAsync completed successfully with count: {count}";
    public const string InvalidBasePathForImageUpload = "Base path is invalid or empty.";
    public const string FetchingTheMostVisitedCities = "Fetching the most visited cities from the repository";

    // DiscountService-related messages
    public const string AddDiscountStarted = "AddDiscountAsync started for room with ID: {RoomId}, request: {@Request}";
    public const string ValidateDiscountRequest = "Validating the request DiscountedPrice and Percentage";
    public const string FetchRoomFromRepository = "Getting the room with ID: {RoomId} from repository";
    public const string CreatingDiscountEntity = "Creating the discount entity for room with ID: {RoomId}";
    public const string DiscountCreated = "Created discount entity: {@Discount}";
    public const string AddingDiscountToRoom = "Adding the discount to the room with ID: {RoomId}";
    public const string SavingDiscountToDatabase = "Saving the created discount to the database";
    public const string MappingDiscountToOutputModel = "Mapping the Discount entity to DiscountOutputModel";
    public const string AddDiscountCompleted = "AddDiscountAsync completed successfully for room with ID: {RoomId}, created discount: {@Discount}";
    public const string GetDiscountStarted = "GetDiscountAsync started for Room ID: {RoomId}, Discount ID: {DiscountId}";
    public const string GetDiscountsStarted = "GetDiscountsAsync started for Room ID: {RoomId}";
    public const string FetchDiscountFromRepository = "Getting the discount with ID: {DiscountId} from repository";
    public const string FetchDiscountsFromRepository = "Getting the discounts for room with ID: {RoomId} from repository";
    public const string GetDiscountCompleted = "GetDiscountAsync completed successfully for Room ID: {RoomId}, Discount ID: {DiscountId}";
    public const string GetDiscountsCompleted = "GetDiscountsAsync completed successfully for Room ID: {RoomId}";
    public const string DeleteDiscountStarted = "DeleteDiscountAsync started for Room ID: {RoomId}, Discount ID: {DiscountId}";
    public const string DeleteDiscountNotFound = "The discount with ID: {DiscountId} was not found in the repository";
    public const string DeleteDiscountCompleted = "DeleteDiscountAsync completed successfully for Room ID: {RoomId}, Discount ID: {DiscountId}";
    public const string GetFeaturedDealsStarted = "GetFeaturedDealsAsync started with deals: {Deals}";
    public const string ValidateNumberOfDeals = "Validating number of requested deals";
    public const string FetchFeaturedDealsFromRepository = "Fetching deals (Room) entities from the repository";
    public const string NoAvailableDeals = "No available deals";
    public const string MappingRoomsToDealsOutputModel = "Mapping Room entities to FeaturedDealOutputModel";
    public const string GetFeaturedDealsCompleted = "GetFeaturedDealsAsync completed successfully with deals: {Deals}";

    // GuestService-related messages
    public const string GetRecentlyVisitedHotelsStarted = "GetRecentlyVisitedHotelsAsync started for guest with ID: {GuestId}, count: {Count}.";
    public const string GetRecentlyVisitedHotelsForCurrentUserStarted = "GetRecentlyVisitedHotelsAsync started for current user, count: {Count}.";
    public const string GetRecentlyVisitedHotelsForCurrentUserCompleted = "GetRecentlyVisitedHotelsAsync completed for current user, count: {Count}, guest ID: {GuestId}.";
    public const string CheckingGuestExistence = "Checking if guest with ID: {GuestId} exists.";
    public const string FetchingRecentBookings = "Fetching recent bookings for guest with ID: {GuestId}.";
    public const string MappingBookingsToOutputModel = "Mapping retrieved booking entities to RecentlyVisitedHotelOutputModel.";
    public const string GettingUserIdFromCurrentUser = "Retrieving user ID from the current user.";
    public const string FetchingGuestByUserId = "Fetching guest by user ID: {UserId}.";

    // HotelService-related messages
    public const string GetAllHotelsStarted = "Retrieving all hotels with request: {@Request}.";
    public const string GetAllHotelsCompleted = "Successfully retrieved all hotels for request: {@Request}.";
    public const string GetHotelStarted = "Retrieving hotel with ID: {HotelId}.";
    public const string GetHotelCompleted = "Successfully retrieved hotel with ID: {HotelId}.";
    public const string DeleteHotelStarted = "Deleting hotel with ID: {HotelId}.";
    public const string DeleteHotelCompleted = "Successfully deleted hotel with ID: {HotelId}.";
    public const string CreateHotelStarted = "Creating new hotel with request: {@Request}.";
    public const string CreateHotelCompleted = "Successfully created hotel with ID: {HotelId}.";
    public const string UpdateHotelStarted = "Updating hotel with ID: {HotelId}.";
    public const string UpdateHotelCompleted = "Successfully updated hotel with ID: {HotelId}.";
    public const string UploadImageStarted = "Uploading image for hotel with ID: {HotelId}.";
    public const string UploadImageCompleted = "Successfully uploaded image for hotel with ID: {HotelId}.";
    public const string AddAmenityFroHotelStarted = "Adding an amenity for hotel with ID: {HotelId} started.";
    public const string AddAmenityFroHotelCompleted = "Successfully added an amenity for hotel with ID: {HotelId}.";

    public const string SearchAndFilterHotelsStarted = "Searching and filtering hotels with request: {@Request}.";
    public const string SearchAndFilterHotelsCompleted = "Successfully searched and filtered hotels with request: {@Request}.";

    // ReviewService-related messages
    public const string AddReviewStarted = "AddReviewAsync started for hotel with ID: {HotelId}";
    public const string AddReviewCompleted = "AddReviewAsync completed successfully for hotel with ID: {HotelId}, from guest with ID: {GuestId}";
    public const string GetHotel = "Getting the hotel from the repository";
    public const string MapRequestToEntity = "Mapping the request model to a Review entity";
    public const string AddReviewToRepository = "Adding the review to the repository";
    public const string MapEntityToOutput = "Mapping the Review entity to a ReviewOutputModel";
    public const string UpdateReviewStarted = "UpdateReviewAsync started for hotel with ID: {HotelId}, review with ID: {ReviewId}";
    public const string UpdateReviewCompleted = "UpdateReviewAsync completed successfully for hotel with ID: {HotelId}, review with ID: {ReviewId}";
    public const string DeleteReviewStarted = "DeleteReviewAsync started for hotel with ID: {HotelId}, review with ID: {ReviewId}";
    public const string DeleteReviewCompleted = "DeleteReviewAsync completed successfully for hotel with ID: {HotelId}, from guest with ID: {GuestId}";
    public const string GetGuestFromCurrentUser = "Getting guest representing the current user";
    public const string ValidateGuestAccess = "Validating user access to add a review";

    // Booking-related messages
    public const string ConvertingBookingToInvoiceStart = "Converting booking to invoice. Booking ID: {BookingId}";
    public const string ConvertingBookingToInvoiceSuccess = "Converting booking to invoice Completed successfully, Invoice: {invoice}";
    public const string StartingRoomWithinInvoiceConversion = "Starting to get RoomWithinInvoice for Room ID: {roomId}";
    public const string RoomPricePerNightRetrieval = "Getting room price per night for Room ID: {roomId}";
    public const string CalculatedNumberOfNights = "Calculated number of nights: {NumberOfNights} for Room ID: {RoomId}";
    public const string RoomWithinInvoiceCreationSuccess = "Successfully created RoomWithinInvoice for Room ID: {RoomId}, {RoomWithinInvoice}";
    public const string GuestIdRetrieval = "Getting the guest id from CurrentUser";
    public const string GuestIdMatchCheck = "Checking if the guest id from the repository matches the guest id from the booking";
    public const string CannotDeleteStartedBooking = "Cannot delete a booking that has started";
    public const string RoomAvailabilityCheck = "Checking availability for RoomId: {RoomId}, CheckIn: {CheckIn}, CheckOut: {CheckOut}";
    public const string RoomAvailabilitySuccess = "RoomId: {RoomId} is available for the given dates, CheckIn: {CheckIn}, CheckOut: {CheckOut}";
    public const string RoomsCapacityValidation = "Rooms capacity validated for request: {@CreateBookingCommand}";
    public const string EmailSentSuccess = "Email sent successfully to {Email}";
    public const string PdfGenerationStart = "Generating PDF for Invoice with Id: {bookingId}";
    public const string PdfGenerationSuccess = "PDF generated successfully for Invoice with Id: {bookingId}";
    public const string DeleteBookingStart = "DeleteBookingAsync started for booking ID: {BookingId}";
    public const string RetrievingBooking = "Retrieving booking with ID: {BookingId}";
    public const string DeletingBooking = "Deleting booking with ID from the Repository: {BookingId}";
    public const string DeleteBookingSuccess = "DeleteBookingAsync completed successfully. Booking ID: {BookingId}";
    public const string SendingEmail = "Sending mail: {@mail} to {@toEmail} with emailService";

    //Review-Related messages
    public const string StartGetHotelAverageRating = "GetHotelAverageRatingAsync started for hotel with ID: {HotelId}";
    public const string GettingHotelFromRepository = "Getting the hotel from the repository";
    public const string GettingAverageRatingFromRepository = "Getting the average rating from the repository";
    public const string RoundingRating = "Rounding the rating to 1 decimal place";
    public const string CompletedGetHotelAverageRating = "GetHotelAverageRatingAsync for hotel with ID: {HotelId} completed successfully";

}

