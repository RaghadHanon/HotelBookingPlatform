using HotelBookingPlatform.Application.AuthorizationOptions;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Infrastructure.Interfaces;
using HotelBookingPlatform.Infrastructure.Interfaces.Validators;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelBookingPlatform.Infrastructure.Identity;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly IGuestRepository _guestRepository;
    private readonly IJwtServiceValidator _jwtValidationService;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings, IGuestRepository guestRepository, IJwtServiceValidator jwtValidationService)
    {
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _guestRepository = guestRepository;
        _jwtValidationService = jwtValidationService;
    }

    public async Task<string> GenerateToken(ApplicationUser user, IList<string> roles)
    {
        _jwtValidationService.ValidateJwtSettings(_jwtSettings);
        _jwtValidationService.ValidateUser(user);
        _jwtValidationService.ValidateRoles(roles);
        IEnumerable<Claim> claims = await CreateClaims(user, roles);
        SigningCredentials signingCredentials = CreateSigningCredentials();
        JwtSecurityToken token = CreateJwtToken(claims, signingCredentials);

        return SerializeToken(token);
    }

    private async Task<IEnumerable<Claim>> CreateClaims(ApplicationUser user, IList<string> roles)
    {
        if (user.Id == null || user.Email == null)
        {
            throw new UnauthenticatedException();
        }

        List<Claim> claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email)
        };
        await AddGuestClaims(user, roles, claims);

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    private async Task AddGuestClaims(ApplicationUser user, IList<string> roles, List<Claim> claims)
    {
        if (roles.Contains(UserRoles.Guest.ToString()))
        {
            Domain.Models.Guest? guest = await _guestRepository.GetGuestByUserIdAsync(user.Id);
            if (guest != null)
            {
                claims.Add(new Claim(ClaimTypes.GivenName, guest.FirstName));
                claims.Add(new Claim(ClaimTypes.Surname, guest.LastName));
                claims.Add(new Claim(ClaimTypes.DateOfBirth, guest.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty));
                claims.Add(new Claim(ClaimTypes.StreetAddress, guest.Address ?? string.Empty));
            }
        }
    }

    private SigningCredentials CreateSigningCredentials()
    {
        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key));
        return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    }

    private JwtSecurityToken CreateJwtToken(IEnumerable<Claim> claims, SigningCredentials signingCredentials)
    {
        return new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.TokenExpirationInHours),
            claims: claims,
            signingCredentials: signingCredentials
        );
    }

    private string SerializeToken(JwtSecurityToken token)
    {
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
