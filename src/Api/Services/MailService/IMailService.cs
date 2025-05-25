using Api.DTOs;

namespace Api.Services.MailService;

public interface IMailService
{
    Task<bool> SendEmail(SendMailDto sendMailDto);
}