using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Infrastructure.Interfaces.Factories;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HotelBookingPlatform.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IEmailStrategyFactory _emailStrategyFactory;

    public EmailService(IOptions<EmailSettings> emailSettings, IEmailStrategyFactory emailStrategyFactory)
    {
        _emailSettings = emailSettings.Value;
        _emailStrategyFactory = emailStrategyFactory;
    }

    public async Task SendEmailAsync(MailRequest mail, EmailType emailType)
    {
        IEmailStrategy strategy = _emailStrategyFactory.CreateStrategy(emailType);
        MimeMessage email = strategy.CreateEmail(mail, _emailSettings);

        using SmtpClient smtp = new SmtpClient();
        smtp.Connect(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}
