using HotelBookingPlatform.Infrastructure.Authentication;
using HotelBookingPlatform.Infrastructure.Identity;

namespace HotelBookingPlatform.Infrastructure.Interfaces;

public interface IJwtServiceValidator
{
    void ValidateJwtSettings(JwtSettings jwtSettings);
    void ValidateRoles(IList<string> roles);
    void ValidateUser(ApplicationUser user);
}
