﻿namespace TechNetworkControlApi.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string messageBody);
    Task SendEmailWithAttachmentsAsync(string email, string subject, string messageBody, params string[] attachmentsPath);
}