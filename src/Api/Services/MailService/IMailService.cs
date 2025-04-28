using Api.DTOs;

namespace Api.Services.MailService;

public interface IMailService
{
    Task<bool> SendConfirmEmail(SendMailDto sendMailDto);
}