namespace HotelBookingPlatform.Application.DTOs.Identity;

public class RegisterAdminDto : RegisterUserDto
{
    public RegisterAdminDto(
        string email,
        string username,
        string password,
        string confirmPassword
        ) : base(email, username, password, confirmPassword)
    { }
}

