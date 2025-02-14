using HotelBookingPlatform.Infrastructure.Identity;

namespace HotelBookingPlatform.Infrastructure.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateToken(ApplicationUser user, IList<string> roles);
}
