namespace HotelBookingPlatform.Application.DTOs.Identity;

public class LoginSuccessDto
{
    public LoginSuccessDto(string userId, string token, string role)
    {
        UserId = userId;
        Token = token;
        Role = role;
    }
    public string UserId { get; }
    public string Role { get; }
    public string Token { get; }
}
