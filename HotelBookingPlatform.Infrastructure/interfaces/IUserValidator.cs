using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Infrastructure.Authentication;

namespace HotelBookingPlatform.Infrastructure.Interfaces;
public interface IUserValidator
{
    void ValidatePasswordsMatch(string password, string confirmPassword);
    Task<ApplicationUser> ValidateUserCredentialsAsync(LoginUserDto model);
}
