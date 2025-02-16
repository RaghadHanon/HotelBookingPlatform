namespace HotelBookingPlatform.Infrastructure.Utilities;
public static class InfrastructureLogMessages
{
    public const string SearchAndFilterHotelsStarted = "SearchAndFilterHotelsAsync started for query: {@HotelSearchAndFilterParameters}";
    public const string SearchingInNameOrDescription = "Searching in name or description";
    public const string FilteringByRoomsAvailability = "Filtering by rooms availability";
    public const string FilteringByAdultsAndChildrenCapacity = "Filtering by adults and children capacity";
    public const string FilteringByStarRating = "Filtering by star rating";
    public const string FilteringByPrice = "Filtering by price";
    public const string FilteringByAmenities = "Filtering by amenities";
    public const string FilteringByRoomTypes = "Filtering by room types";
    public const string ApplyingSorting = "Applying sorting";
    public const string GettingPaginationMetadata = "Getting pagination metadata";
    public const string ApplyingPagination = "Applying pagination";
    public const string InvokingDatabaseAndGettingResult = "Invoking the Database and Getting the result";
    public const string SearchAndFilterHotelsCompleted = "SearchAndFilterHotelsAsync for query: {@HotelSearchAndFilterParameters} completed successfully";
}