using HotelBookingPlatform.Infrastructure.Identity;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Validators;

public interface IJwtServiceValidator
{
    void ValidateJwtSettings(JwtSettings jwtSettings);
    void ValidateRoles(IList<string> roles);
    void ValidateUser(ApplicationUser user);
}
