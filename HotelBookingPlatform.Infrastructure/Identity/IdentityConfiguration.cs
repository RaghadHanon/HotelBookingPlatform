using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Identity;
using HotelBookingPlatform.Infrastructure.Interfaces;
using HotelBookingPlatform.Infrastructure.Interfaces.Validators;
using HotelBookingPlatform.Infrastructure.Persistence;
using HotelBookingPlatform.Infrastructure.Utilities;
using HotelBookingPlatform.Infrastructure.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HotelBookingPlatform.Infrastructure.Identity;

public static class IdentityConfiguration
{
    private const int PasswordRequiredLength = 6;
    private const bool RequireDigit = true;
    private const bool RequireUppercase = true;
    private const bool RequireNonAlphanumeric = true;
    private const bool RequireUniqueEmail = true;

    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureIdentity(services);
        ConfigureJwtAuthentication(services, configuration);
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddTransient<IIdentityManager, IdentityManager>();
        services.AddTransient<IUserValidator, UserValidator>();
        services.AddTransient<IJwtServiceValidator, JwtServiceValidator>();
        services.AddTransient<IImageServiceValidator, ImageServiceValidator>();
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

        return services;
    }

    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = PasswordRequiredLength;
            options.Password.RequireDigit = RequireDigit;
            options.Password.RequireUppercase = RequireUppercase;
            options.Password.RequireNonAlphanumeric = RequireNonAlphanumeric;
            options.User.RequireUniqueEmail = RequireUniqueEmail;
        }
        ).AddEntityFrameworkStores<HotelBookingPlatformDbContext>();
    }

    private static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>()
            ?? throw new TokenGenerationFailedException(InfrastructureErrorMessages.SecretKeyIsMissing);

        byte[] keyBytes = Encoding.ASCII.GetBytes(jwtSettings.Key ?? throw new TokenGenerationFailedException(InfrastructureErrorMessages.SecretKeyIsMissing));

        services.AddAuthentication(authentication =>
        {
            authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
        ).AddJwtBearer(bearer =>
            {
                bearer.RequireHttpsMetadata = false;
                bearer.SaveToken = true;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });
    }
}
