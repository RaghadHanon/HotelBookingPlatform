namespace HotelBookingPlatform.Application.DTOs.Common;

public class PaginatedResult<T>
{
    public PaginatedResult(IEnumerable<T> data, PaginationMetadata metadata)
    {
        Data = data;
        Metadata = metadata;
    }

    public IEnumerable<T> Data { get; set; }
    public PaginationMetadata Metadata { get; set; }
}