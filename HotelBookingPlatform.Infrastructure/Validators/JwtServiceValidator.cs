using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Infrastructure.Identity;
using HotelBookingPlatform.Infrastructure.Interfaces.Validators;
using HotelBookingPlatform.Infrastructure.Utilities;

namespace HotelBookingPlatform.Infrastructure.Validators;

public class JwtServiceValidator : IJwtServiceValidator
{
    public void ValidateJwtSettings(JwtSettings jwtSettings)
    {
        if (string.IsNullOrEmpty(jwtSettings.Key))
        {
            throw new TokenGenerationFailedException(InfrastructureErrorMessages.JwtKeyMissing);
        }

        if (string.IsNullOrEmpty(jwtSettings.Issuer))
        {
            throw new TokenGenerationFailedException(InfrastructureErrorMessages.JwtIssuerMissing);
        }
    }

    public void ValidateUser(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user?.Id) || string.IsNullOrEmpty(user.Email))
        {
            throw new UnauthenticatedException(InfrastructureErrorMessages.InvalidUser);
        }
    }

    public void ValidateRoles(IList<string> roles)
    {
        if (roles == null || roles.Count == 0)
        {
            throw new TokenGenerationFailedException(InfrastructureErrorMessages.RolesMissing);
        }
    }
}
