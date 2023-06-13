using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TechNetworkControlApi.Services;

public class EmailService : IEmailService
{
    public string CorporationEmail => _config.Value.CorporationEmail;
    
    private readonly IOptions<EmailSettings> _config;

    public EmailService(IOptions<EmailSettings> config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string messageBody)
    {
        var message = CreateMessage(_config.Value.RobotLogin, email, subject);
        
        var builder = new BodyBuilder { HtmlBody = messageBody} ;
        message.Body = builder.ToMessageBody();

        await SendMessageSmtpAsync(message);
    }

    public async Task SendEmailWithAttachmentsAsync(string email, string subject, string messageBody, params string[] attachmentsPath)
    {
        var message = CreateMessage(_config.Value.RobotLogin, email, subject);
        
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
        await client.AuthenticateAsync(_config.Value.RobotLogin, _config.Value.RobotPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}