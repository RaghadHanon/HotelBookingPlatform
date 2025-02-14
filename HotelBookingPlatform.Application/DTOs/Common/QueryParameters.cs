using HotelBookingPlatform.Application.Enums;

namespace HotelBookingPlatform.Application.DTOs.Common;

public abstract class QueryParameters<TSortColumn> where TSortColumn : Enum
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Search term for filtering results.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// The column to sort by, as a strongly typed enum.
    /// </summary>
    public TSortColumn? SortColumn { get; set; }

    /// <summary>
    /// The sort order, either Ascending or Descending.
    /// </summary>
    public SortOrder? SortOrder { get; set; }
}
