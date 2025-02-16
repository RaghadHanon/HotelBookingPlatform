namespace HotelBookingPlatform.Domain.Models;

public class Discount : Entity
{
    public Discount()
    {
    }

    public Discount(Room room, decimal percentage, DateTime startDate, DateTime endDate)
    {
        Room = room;
        Percentage = Math.Round(percentage, 2);
        StartDate = startDate;
        EndDate = endDate;
        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public Discount(Room room, decimal originalPrice, decimal discountedPrice,
    DateTime startDate, DateTime endDate)
    {
        Room = room;
        Percentage = CalculateDiscountPercentage(originalPrice, discountedPrice);
        StartDate = startDate;
        EndDate = endDate;

        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public decimal Percentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OriginalPrice => Room != null ? Room.Price : 0;
    public decimal DiscountedPrice => CalculateDiscountedPrice();
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!;

    private static decimal CalculateDiscountPercentage(decimal originalPrice, decimal discountedPrice)
    {
        return Math.Round((originalPrice - discountedPrice) / originalPrice * 100, 2);
    }

    private decimal CalculateDiscountedPrice()
    {
        return Math.Round(OriginalPrice - (OriginalPrice * Percentage / 100), 2);
    }
}
