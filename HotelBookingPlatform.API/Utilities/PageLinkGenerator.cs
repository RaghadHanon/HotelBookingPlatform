using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using Microsoft.AspNetCore.Mvc;
using Serilog;

public static class PageLinkGenerator
{
    /// <summary>
    /// Adds next and previous page links to the provided pagination metadata.
    /// Handles both general and domain-specific parameters dynamically.
    /// </summary>
    public static void AddPageLinks<TSortColumn>(
        IUrlHelper url,
        string routeName,
        PaginationMetadata paginationMetadata,
        QueryParameters<TSortColumn> parameters) where TSortColumn : Enum
    {
        Log.Debug("Adding page links: {@parameters}, Metadata: {@paginationMetadata}", parameters, paginationMetadata);
        paginationMetadata.PreviousPageLink = paginationMetadata.HasPreviousPage
            ? GeneratePageLink(url, routeName, paginationMetadata, parameters, isNextPage: false)
            : null;
        paginationMetadata.NextPageLink = paginationMetadata.HasNextPage
            ? GeneratePageLink(url, routeName, paginationMetadata, parameters, isNextPage: true)
            : null;
        Log.Debug("Page links added successfully: {@parameters}, Metadata: {@paginationMetadata}", parameters, paginationMetadata);
    }

    /// <summary>
    /// Generates the page link dynamically, adding domain-specific parameters if applicable.
    /// </summary>
    private static string? GeneratePageLink<TSortColumn>(
        IUrlHelper url,
        string routeName,
        PaginationMetadata paginationMetadata,
        QueryParameters<TSortColumn> parameters,
        bool isNextPage) where TSortColumn : Enum
    {
        Log.Debug("Generating {PageDirection} page link: {@parameters}, Metadata: {@paginationMetadata}",
            isNextPage ? "next" : "previous", parameters, paginationMetadata);
        int newPageNumber = isNextPage ? paginationMetadata.PageNumber + 1 : paginationMetadata.PageNumber - 1;
        Dictionary<string, object?> routeValues = new Dictionary<string, object?>
        {
            { "SortOrder", parameters.SortOrder },
            { "SortColumn", parameters.SortColumn },
            { "PageNumber", newPageNumber },
            { "PageSize", paginationMetadata.PageSize },
            { "SearchTerm", parameters.SearchTerm }
        };
        if (parameters is HotelSearchAndFilterParametersDto hotelParams)
        {
            AddHotelSpecificParameters(routeValues, hotelParams);
        }

        return url.Link(routeName, routeValues);
    }

    /// <summary>
    /// Adds hotel-specific parameters to the route values dictionary.
    /// </summary>
    private static void AddHotelSpecificParameters(
        IDictionary<string, object?> routeValues,
        HotelSearchAndFilterParametersDto hotelParams)
    {
        routeValues["CheckInDate"] = hotelParams.CheckInDate;
        routeValues["CheckOutDate"] = hotelParams.CheckOutDate;
        routeValues["Adults"] = hotelParams.Adults;
        routeValues["Children"] = hotelParams.Children;
        routeValues["Rooms"] = hotelParams.Rooms;
        routeValues["MinStarRating"] = hotelParams.MinStarRating;
        routeValues["MaxPrice"] = hotelParams.MaxPrice;
        routeValues["MinPrice"] = hotelParams.MinPrice;
        routeValues["Amenities"] = hotelParams.Amenities;
        routeValues["RoomTypes"] = hotelParams.RoomTypes;
    }
}
