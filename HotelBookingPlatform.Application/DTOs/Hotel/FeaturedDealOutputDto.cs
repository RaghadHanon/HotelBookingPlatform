using HotelBookingPlatform.Application.DTOs.Location;
using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.DTOs.Hotel;

public class FeaturedDealOutputDto
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = default!;
    public HotelImageOutputDto HotelImage { get; set; } = default!;
    public int StarRate { get; set; }
    public string? Street { get; set; }
    public LocationDto Location { get; set; } = default!;

    public Guid RoomId { get; set; }
    public int RoomNumber { get; set; }
    public RoomType RoomType { get; set; }
    public decimal OriginalPrice { get; set; }

    public decimal DiscountedPrice { get; set; }
    public double DiscountPercentage { get; set; }

    public string CityName { get; set; } = default!;
    public string Country { get; set; } = default!;
}
