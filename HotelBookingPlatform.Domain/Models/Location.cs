namespace HotelBookingPlatform.Domain.Models;
public class Location : Entity
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid HotelId { get; set; }
}
