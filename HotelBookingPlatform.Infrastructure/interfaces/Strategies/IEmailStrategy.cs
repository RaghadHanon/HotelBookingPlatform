using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Infrastructure.Services.Email;
using MimeKit;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Strategies;
public interface IEmailStrategy
{
    MimeMessage CreateEmail(MailRequest mail, EmailSettings emailSettings);
}
