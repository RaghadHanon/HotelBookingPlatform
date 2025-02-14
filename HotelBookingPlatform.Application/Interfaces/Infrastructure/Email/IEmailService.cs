using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Application.Enums;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(MailRequest mail, EmailType emailType);
}