using HotelBookingPlatform.Application.DTOs.Email;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(MailRequest mail);
}