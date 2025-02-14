namespace HotelBookingPlatform.Application.DTOs.Identity;

/// <summary>
/// DTO for logging in a user
/// </summary>
public class LoginUserDto : UserInputDto
{
    public LoginUserDto(string email, string password) : base(email, password)
    {
    }
}
