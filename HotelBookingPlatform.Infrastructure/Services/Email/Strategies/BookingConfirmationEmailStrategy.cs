using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Infrastructure.Interfaces.Strategies;
using MimeKit;


namespace HotelBookingPlatform.Infrastructure.Services.Email.Strategies;
public class BookingConfirmationEmailStrategy : IEmailStrategy
{
    public MimeMessage CreateEmail(MailRequest mail, EmailSettings emailSettings)
    {
        MimeMessage email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(emailSettings.Email);
        email.To.Add(MailboxAddress.Parse(mail.ToEmail));
        email.Subject = mail.Subject;
        BodyBuilder builder = new BodyBuilder();
        builder.HtmlBody = AsHtml(mail.Body);
        builder.Attachments.Add("invoice.pdf", mail.Attachment, new ContentType("application", "pdf"));
        email.Body = builder.ToMessageBody();

        return email;
    }

    private static string AsHtml(string body)
    {
        return $"""
        <h3>New Booking At Hotel Booking Platform</h3>
        <p>{body}</p>
        """;
    }
}
