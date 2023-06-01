using System.Net;
using System.Net.Mail;
using MimeKit;
using TechNetworkControlApi.DTO;
using TechNetworkControlApi.Infrastructure.Entities;

namespace TechNetworkControlApi.Services;

public class EmailService : IEmailService
{
    public void SendEmail(string email, string subject, string messageBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Служба поддержки Net-Eye", "robot.Net-Eye@yandex.ru"));
        message.To.Add(new MailboxAddress(email, email));
        message.Subject = "Регистрация в системе Net-Eye";
        var builder = new BodyBuilder();
        builder.HtmlBody = messageBody;

        message.Body = builder.ToMessageBody();

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            client.Connect("smtp.yandex.ru", 25);
            client.Authenticate("robot.Net-Eye@yandex.ru", "hyvnzzixeprjikpa");
            client.Send(message);
            client.Disconnect(true);
        }
    }
}