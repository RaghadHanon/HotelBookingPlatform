using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingPlatform.Infrastructure.Services.Email;

public static class EmailConfiguration
{
    public static IServiceCollection AddEmailInfrastructure
        (this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
