using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Infrastructure.Interfaces.Factories;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;
using HotelBookingPlatform.Infrastructure.Services.Email.Strategies;
using HotelBookingPlatform.Infrastructure.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBookingPlatform.Infrastructure.Services.Email;
public class EmailStrategyFactory : IEmailStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmailStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmailStrategy CreateStrategy(EmailType emailType)
    {
        return emailType switch
        {
            EmailType.BookingConfirmationEmail => _serviceProvider.GetRequiredService<BookingConfirmationEmailStrategy>(),
            _ => throw new ArgumentException(InfrastructureErrorMessages.NoSmailStrategyFound, emailType.ToString())
        };
    }
}
