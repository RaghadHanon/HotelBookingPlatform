namespace HotelBookingPlatform.Application.DTOs.Identity;

public class LoginOutputDto
{
    public LoginOutputDto(string userId, string token)
    {
        UserId = userId;
        Token = token;
    }

    public string UserId { get; }
    public string Token { get; }
}
