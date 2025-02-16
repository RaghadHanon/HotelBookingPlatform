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
        string confirmPassword) : base(email, password)
    {
        Username = username;
        ConfirmPassword = confirmPassword;
    }

    /// <summary>
    /// Confirm the password.
    /// </summary>
    public string ConfirmPassword { get; set; }

    /// <summary>
    /// Username for the user.
    /// </summary>
    public string Username { get; set; }
}
