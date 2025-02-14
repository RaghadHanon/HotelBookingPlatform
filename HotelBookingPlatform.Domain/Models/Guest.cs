namespace HotelBookingPlatform.Domain.Models;

public class Guest : Entity
{
    public Guest(string firstName, string lastName, DateOnly? dateOfBirth, string? addres)
    {
        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Address = addres;
    }
    public Guest()
    {
        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public DateOnly? DateOfBirth { get; set; } = default!;
    public string? Address { get; set; } = default!;
}
