using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Infrastructure.Identity;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Validators;
public interface IUserValidator
{
    void ValidatePasswordsMatch(string password, string confirmPassword);
    Task<ApplicationUser> ValidateUserCredentialsAsync(LoginUserDto model);
}
