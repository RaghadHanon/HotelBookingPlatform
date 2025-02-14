namespace HotelBookingPlatform.API.Utilities;
public class APILogMessages
{
    // Log messages for the BookingsController
    public const string GetBookingStarted = "GetBooking started for booking with ID: {BookingId}";
    public const string GetBookingCompleted = "GetBooking for booking with ID: {BookingId} completed successfully";
    public const string GetInvoiceStarted = "GetInvoice started for booking with ID: {BookingId}";
    public const string GetInvoiceCompleted = "GetInvoice for booking with ID: {BookingId} completed successfully";
    public const string CreateBookingStarted = "CreateBooking started for request: {@CreateBooking}";
    public const string CreateBookingCompleted = "CreateBooking for request: {@CreateBooking} completed successfully";
    public const string DeleteBookingStarted = "DeleteBooking started for booking with ID: {BookingId}";
    public const string DeleteBookingCompleted = "DeleteBooking for booking with ID: {BookingId} completed successfully";

    // Log messages for the CitiesController
    public const string GetCityStart = "GetCity started for city with ID: {CityId}";
    public const string GetCityComplete = "GetCity for city with ID: {CityId} completed successfully";
    public const string CreateCityStart = "CreateCity started for request: {@CreateCity}";
    public const string CreateCityComplete = "CreateCity for request: {@CreateCity} completed successfully";
    public const string DeleteCityStart = "DeleteCity started for city with ID: {CityId}";
    public const string DeleteCityComplete = "DeleteCity for city with ID: {CityId} completed successfully";
    public const string UpdateCityStart = "UpdateCity started for city with ID: {CityId}, request: {@UpdateCity}";
    public const string UpdateCityComplete = "UpdateCity completed successfully for city with ID: {CityId}, request: {@UpdateCity}";
    public const string MostVisitedCitiesStart = "Retrieving top {Count} most visited cities has started";
    public const string MostVisitedCitiesComplete = "Retrieving top {Count} most visited cities completed successfully";
    public const string UploadImageStart = "UploadImage started for city with ID: {CityId}";
    public const string UploadImageComplete = "UploadImage completed successfully for city with ID: {CityId}";
    public const string GetCitiesStart = "GetCities started with query parameters: {@GetCitiesQuery}";
    public const string GetCitiesComplete = "GetCities successfully completed for query parameters: {@GetCitiesQuery}";

    // Log messages for the HotelsController
    public const string GetHotelStarted = "GetHotel started for hotel with ID: {HotelId}";
    public const string GetHotelCompleted = "GetHotel for hotel with ID: {HotelId} completed successfully";
    public const string CreateHotelStarted = "CreateHotel started for request: {@CreateHotel}";
    public const string CreateHotelCompleted = "CreateHotel for request: {@CreateHotel} completed successfully";
    public const string DeleteHotelStarted = "DeleteHotel started for hotel with ID: {HotelId}";
    public const string DeleteHotelCompleted = "DeleteHotel for hotel with ID: {HotelId} completed successfully";
    public const string UpdateHotelStarted = "UpdateHotel started for hotel with ID: {HotelId}, request: {@UpdateHotel}";
    public const string UpdateHotelCompleted = "UpdateHotel completed successfully for hotel with ID: {HotelId}, request: {@UpdateHotel}";
    public const string UploadImageStarted = "UploadImage started for hotel with ID: {HotelId}";
    public const string UploadImageCompleted = "UploadImage completed successfully for hotel with ID: {HotelId}";
    public const string GetHotelsStarted = "GetHotels started for query: {@GetHotelsQuery}";
    public const string GetHotelsCompleted = "GetHotels for query: {@GetHotelsQuery} completed successfully";
    public const string SearchAndFilterHotelsStarted = "SearchAndFilterHotels started for query: {@HotelSearchAndFilterParameters}";
    public const string SearchAndFilterHotelsCompleted = "SearchAndFilterHotels for query: {@HotelSearchAndFilterParameters} completed successfully";

    // Log messages for the DiscountsController
    public const string AddDiscountStarted = "AddDiscount started for room with ID: {RoomId}, request: {@CreateDiscount}";
    public const string AddDiscountCompleted = "AddDiscount for room with ID: {RoomId}, request: {@CreateDiscount} completed successfully";
    public const string GetDiscountStarted = "GetDiscount started for room with ID: {RoomId}, discount with ID: {DiscountId}";
    public const string GetDiscountCompleted = "GetDiscount for room with ID: {RoomId}, discount with ID: {DiscountId} completed successfully";
    public const string GetDiscountsStarted = "GetDiscounts started for room with ID: {RoomId}";
    public const string GetDiscountsCompleted = "GetDiscounts for room with ID: {RoomId} completed successfully";
    public const string DeleteDiscountStarted = "DeleteDiscount started for room with ID: {RoomId}, discount with ID: {DiscountId}";
    public const string DeleteDiscountCompleted = "DeleteDiscount for room with ID: {RoomId}, discount with ID: {DiscountId} completed successfully";
    public const string GetFeaturedDealsStarted = "GetFeaturedDeals started with count: {featuredDealsCount}";
    public const string GetFeaturedDealsCompleted = "GetFeaturedDeals with count: {featuredDealsCount} completed successfully";

    // Log messages for the AmenitiesController
    public const string AddAmenityStarted = "AddAmenity started for request: {@CreateAmenity}";
    public const string AddAmenityCompleted = "AddAmenity for request: {@CreateAmenity} completed successfully";
    public const string GetAmenityStarted = "GetAmenity started for amenity with ID: {AmenityId}";
    public const string GetAmenityCompleted = "GetAmenity for amenity with ID: {AmenityId} completed successfully";
    public const string GetAmenitiesStarted = "GetAmenities started with query parameters: {@GetAmenitiesQuery}";
    public const string GetAmenitiesCompleted = "GetAmenities completed successfully for query parameters: {@GetAmenitiesQuery}";
    public const string DeleteAmenityStarted = "DeleteAmenity started for amenity with ID: {AmenityId}";
    public const string DeleteAmenityCompleted = "DeleteAmenity for room amenity with ID: {AmenityId} completed successfully";
    public const string UpdateAmenityStarted = "UpdateAmenity started for request:{@UpdateAmenity}";
    public const string UpdateAmenityCompleted = " UpdateAmenity for request:{@UpdateAmenity} completed successfully";

    // Log messages for the GuestsController
    public const string GetGuestStarted = "GetGuest started for guest ID: {GuestId}";
    public const string GetGuestCompleted = "GetGuest completed for guest ID: {GuestId}";
    public const string GetBookingsStarted = "GetBookings started with query parameters: {@GetBookingsQuery}";
    public const string GetBookingsCompleted = "GetBookings successfully completed for query parameters: {@GetBookingsQuery}";
    public const string GetRecentlyVisitedHotelsStarted = "GetRecentlyVisitedHotels started for guest with ID: {GuestId}, count: {recentlyVisitedHotelsCount}";
    public const string GetRecentlyVisitedHotelsCompleted = "GetRecentlyVisitedHotels for guest with ID: {GuestId}, count: {recentlyVisitedHotelsCount} completed successfully";
    public const string GetRecentlyVisitedHotelsCurrentUserStarted = "GetRecentlyVisitedHotels started for current user, count: {recentlyVisitedHotelsCount}";
    public const string GetRecentlyVisitedHotelsCurrentUserCompleted = "GetRecentlyVisitedHotels for current user, count: {recentlyVisitedHotelsCount} completed successfully";

    // Log messages for the ReviewsController
    public const string AddReviewStarted = "AddReview started for hotel with ID: {HotelId}, request: {@AddReview}";
    public const string AddReviewCompleted = "AddReview for hotel with ID: {HotelId}, request: {@AddReview} completed successfully";
    public const string GetReviewStarted = "GetReview started for hotel with ID: {HotelId}, review with ID: {ReviewId}";
    public const string GetReviewCompleted = "GetReview for hotel with ID: {HotelId}, review with ID: {ReviewId} completed successfully";
    public const string GetHotelAverageRatingStarted = "GetHotelAverageRating started for hotel with ID: {HotelId}";
    public const string GetHotelAverageRatingCompleted = "GetHotelAverageRating for hotel with ID: {HotelId} completed successfully";
    public const string UpdateReviewStarted = "UpdateReview started for hotel with ID: {HotelId}, review with ID: {ReviewId}, request: {@UpdateReview}";
    public const string UpdateReviewCompleted = "UpdateReview for hotel with ID: {HotelId}, review with ID: {ReviewId}, request: {@UpdateReview} completed successfully";
    public const string DeleteReviewStarted = "DeleteReview started for hotel with ID: {HotelId}, review with ID: {ReviewId}";
    public const string DeleteReviewCompleted = "DeleteReview for hotel with ID: {HotelId}, review with ID: {ReviewId} completed successfully";
    public const string GetHotelReviewsStarted = "GetHotelReviews started for hotel with ID: {HotelId}, request: {@GetHotelReviews}";
    public const string GetHotelReviewsCompleted = "GetHotelReviews for hotel with ID: {HotelId}, request: {@GetHotelReviews} completed successfully";

    // Log messages for the RoomsController
    public const string FetchRoomDetails = "Fetching room details for ID: {RoomId}";
    public const string SuccessfullyFetchedRoomDetails = "Successfully fetched room details for ID: {RoomId}";
    public const string CreatingRoom = "Creating a new room with request: {@CreateRoom}";
    public const string SuccessfullyCreatedRoom = "Successfully created a room with ID: {RoomId}";
    public const string DeletingRoom = "Deleting room with ID: {RoomId}";
    public const string SuccessfullyDeletedRoom = "Successfully deleted room with ID: {RoomId}";
    public const string UpdatingRoom = "Updating room with ID: {RoomId} using request: {@UpdateRoom}";
    public const string SuccessfullyUpdatedRoom = "Successfully updated room with ID: {RoomId}";
    public const string UploadingImage = "Uploading image for room with ID: {RoomId}";
    public const string SuccessfullyUploadedImage = "Successfully uploaded image for room with ID: {RoomId}";
    public const string FetchingRoomsWithQuery = "Fetching rooms with query: {@Query}";
    public const string SuccessfullyFetchedRoomsWithQuery = "Successfully fetched rooms with query: {@Query}";
}
