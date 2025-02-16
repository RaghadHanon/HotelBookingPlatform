namespace HotelBookingPlatform.Application.DTOs.Identity;

/// <summary>
/// DTO for registering a Guest.
/// </summary>
public class RegisterGuestDto : RegisterUserDto
{
    public RegisterGuestDto(
        string email,
        string username,
        string password,
        string firstName,
        string lastName,
        string confirmPassword,
        DateOnly? dateOfBirth = default!,
        string? address = null) : base(email, username, password, confirmPassword)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Address = address;
    }
   
    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Date of birth of the user.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Address of the user.
    /// </summary>
    public string? Address { get; set; }
}

