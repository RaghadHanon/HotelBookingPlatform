using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Infrastructure.Authentication;
using HotelBookingPlatform.Infrastructure.Utilities;
using HotelBookingPlatform.Infrastructure.Interfaces;
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
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
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