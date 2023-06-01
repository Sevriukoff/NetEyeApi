using MailKit.Net.Smtp;
using MimeKit;

namespace TechNetworkControlApi.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string email, string subject, string messageBody)
    {
        var message = CreateMessage("robot.Net-Eye@yandex.ru", email, subject);
        
        var builder = new BodyBuilder { HtmlBody = messageBody} ;
        message.Body = builder.ToMessageBody();

        await SendMessageSmtpAsync(message);
    }

    public async Task SendEmailWithAttachmentsAsync(string email, string subject, string messageBody, params string[] attachmentsPath)
    {
        var message = CreateMessage("robot.Net-Eye@yandex.ru", email, subject);
        
        var builder = new BodyBuilder{HtmlBody = messageBody};
        
        foreach (var attachment in attachmentsPath)
            builder.Attachments.Add(attachment.Replace('/', '\\'));

        message.Body = builder.ToMessageBody();

        await SendMessageSmtpAsync(message);
    }

    private MimeMessage CreateMessage(string from, string to, string subject)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Служба поддержки Net-Eye", from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;

        return message;
    }

    private async Task SendMessageSmtpAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.yandex.ru", 25);
        await client.AuthenticateAsync("robot.Net-Eye@yandex.ru", "hyvnzzixeprjikpa");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}