using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace HotelBookingPlatform.API.Utilities;

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
        var newPageNumber = isNextPage ? paginationMetadata.PageNumber + 1 : paginationMetadata.PageNumber - 1;
        var routeValues = new Dictionary<string, object?>
        {
            { "PageNumber", newPageNumber },
            { "PageSize", paginationMetadata.PageSize }
        };

        AddIfNotNull(routeValues, "SortOrder", parameters.SortOrder?.ToString());
        AddIfNotNull(routeValues, "SortColumn", parameters.SortColumn?.ToString());
        AddIfNotNull(routeValues, "SearchTerm", parameters.SearchTerm);

        if (parameters is HotelSearchAndFilterParametersDto hotelParams)
        {
            AddHotelSpecificParameters(routeValues, hotelParams);
        }

        return url.Link(routeName, routeValues);
    }

    /// <summary>
    /// Adds hotel-specific parameters to the route values dictionary if they have values.
    /// </summary>
    private static void AddHotelSpecificParameters(
        IDictionary<string, object?> routeValues,
        HotelSearchAndFilterParametersDto hotelParams)
    {
        AddIfNotNull(routeValues, "CheckInDate", hotelParams.CheckInDate);
        AddIfNotNull(routeValues, "CheckOutDate", hotelParams.CheckOutDate);
        AddIfNotNull(routeValues, "Adults", hotelParams.Adults);
        AddIfNotNull(routeValues, "Children", hotelParams.Children);
        AddIfNotNull(routeValues, "Rooms", hotelParams.Rooms);
        AddIfNotNull(routeValues, "MinStarRating", hotelParams.MinStarRating);
        AddIfNotNull(routeValues, "MaxPrice", hotelParams.MaxPrice);
        AddIfNotNull(routeValues, "MinPrice", hotelParams.MinPrice);
    }

    /// <summary>
    /// Helper method to add a value to the route values dictionary if it's not null or empty.
    /// </summary>
    private static void AddIfNotNull<T>(IDictionary<string, object?> routeValues, string key, T value)
    {
        if (value == null) return;

        if (value is string strValue && string.IsNullOrEmpty(strValue)) return;

        routeValues[key] = value;
    }
}