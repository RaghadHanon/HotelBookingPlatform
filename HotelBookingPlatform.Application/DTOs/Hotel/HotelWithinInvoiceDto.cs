namespace HotelBookingPlatform.Application.DTOs.Hotel;

public class HotelWithinInvoiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string CityName { get; set; } = default!;
    public string? Street { get; set; } = default!;
}
