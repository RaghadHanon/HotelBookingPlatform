using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Infrastructure.Identity;
using HotelBookingPlatform.Infrastructure.Interfaces.Validators;
using HotelBookingPlatform.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingPlatform.Infrastructure.Validators;

public class UserValidator : IUserValidator
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser> ValidateUserCredentialsAsync(LoginUserDto model)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync((ApplicationUser)user, model.Password))
        {
            throw new InvalidUserCredentialsException();
        }

        return user;
    }

    public void ValidatePasswordsMatch(string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            throw new BadRequestException(InfrastructureErrorMessages.PasswordsDoNotMatch);
        }
    }
}