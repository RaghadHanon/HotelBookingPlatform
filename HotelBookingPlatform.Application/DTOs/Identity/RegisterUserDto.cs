namespace HotelBookingPlatform.Application.DTOs.Identity;

/// <summary>
/// DTO for registering a user.
/// </summary>
public class RegisterUserDto : UserInputDto
{
    public RegisterUserDto(
        string email,
        string username,
        string password,
        string firstName,
        string lastName,
        string confirmPassword,
        DateOnly? dateOfBirth = default!,
        string? address = null) : base(email, password)
    {
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        ConfirmPassword = confirmPassword;
        DateOfBirth = dateOfBirth;
        Address = address;
    }

    /// <summary>
    /// Confirm the password.
    /// </summary>
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// Username for the user.
    /// </summary>
    public string Username { get; set; }

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
