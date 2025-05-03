using System.ComponentModel.DataAnnotations;
using Api.Domain.ModelSettings;
using Api.DTOs;
using Microsoft.Extensions.Options;
using MailKit.Security;
using MimeKit;

namespace Api.Services.MailService;

public class MailService(IOptions<MailSettings> mailSettings) : IMailService
{
    private readonly MailSettings _mailSettings = mailSettings.Value;

    public async Task<bool> SendConfirmEmail(SendMailDto sendMailDto)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(sendMailDto, null, null);
        var isValid =
            Validator.TryValidateObject(sendMailDto, context, validationResults, true);

        if (!isValid) throw new ValidationException(validationResults[0].ErrorMessage);

        var email = new MimeMessage();
        email.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
        email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
        email.To.Add(MailboxAddress.Parse(sendMailDto.To));
        email.Subject = sendMailDto.Subject;
        var builder = new BodyBuilder
        {
            HtmlBody = sendMailDto.Body
        };
        email.Body = builder.ToMessageBody();
        using var smtp = new MailKit.Net.Smtp.SmtpClient();
        try
        {
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
        }
        catch (Exception)
        {
            await smtp.DisconnectAsync(true);
            return false;
        }

        await smtp.DisconnectAsync(true);
        return true;
    }
}