using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Factories;
public interface IEmailStrategyFactory
{
    IEmailStrategy CreateStrategy(EmailType emailType);
}
