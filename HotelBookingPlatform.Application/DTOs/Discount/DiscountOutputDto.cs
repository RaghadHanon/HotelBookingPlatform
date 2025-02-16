namespace HotelBookingPlatform.Application.DTOs.Discount;

public class DiscountOutputDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }

    public decimal Percentage { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
