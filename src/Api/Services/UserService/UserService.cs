using System.Security.Claims;
using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.Domain.ModelSettings;
using Api.DTOs;
using Api.DTOs.UploadFileService;
using Api.DTOs.User;
using Api.Extensions;
using Api.Services.MailService;
using Api.Services.TokenService;
using Api.Services.UploadFileService;
using Api.Services.UserBanService;
using Api.TransferDTOs.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services.UserService;

public class UserService(
    Context context,
    IFileUploadService uploadFileService,
    IMailService mailService,
    IUserBanService userBanService,
    IOptions<AppSettings> appSettings,
    ITokenService tokenService,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User> GetUser(Guid id, UserType userType)
    {
        User? user = null;

        if (userType is UserType.SchoolAdmin or UserType.SchoolSuperVisor)
            user = await context.SchoolPersons.FirstOrDefaultAsync(x => x.Id == id);
        else if (userType == UserType.Parent)
            user = await context.Parents.FirstOrDefaultAsync(x => x.Id == id);
        else if (userType == UserType.Driver)
            user = await context.Drivers.FirstOrDefaultAsync(x => x.Id == id);
        else if (userType == UserType.Admin) user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        return user;
    }

    public async Task<User> UpdateUserInfo(Guid id, UpdateUserInfoDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        if (IsVerified(user))
        {
            if (user.Email != dto.Email && user.VerificationMethod == VerificationMethod.Email)
            {
                user.AccountStatus = AccountStatus.New;
                user.VerificationMethod = null;
            }

            if (user.PhoneNumber != dto.PhoneNumber && user.VerificationMethod == VerificationMethod.PhoneNumber)
            {
                user.AccountStatus = AccountStatus.New;
                user.VerificationMethod = null;
            }
        }


        await ValidateUserInput(dto, user);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.DateOfBirth = dto.DateOfBirth;
        user.Gender = dto.Gender;
        user.PhoneNumber = dto.PhoneNumber;
        user.Email = dto.Email;
        user.Address = dto.Address;
        user.DetailAddress = dto.DetailAddress;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<string> UpdateAvatar(Guid id, IFormFile file)
    {
        UploadFileResponse uploadResponse;
        await uploadFileService.BeginTransactionAsync();
        try
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                throw new NotFoundException(ErrorMessages.UserNotFound);

            if (user.AvatarKey != null)
                await uploadFileService.DeleteFileManagementAsync(user.AvatarKey.Value);
            uploadResponse = await uploadFileService.UploadFileAsync(file, "avatar");

            user.AvatarKey = uploadResponse.Key;
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            uploadFileService
                .RollBackAsync()
                .FireAndForget((ex) => logger.LogError(ex, "UploadFileService.RollBackAsync"));
            throw;
        }

        return uploadResponse.S3Url;
    }

    public async Task<User> UpdateDriverInformation(Guid id, UpdateDriverInformationRequest request)
    {
        var user = await context.Drivers.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);


        user.LicenseNumber = request.LicenseNumber;
        user.SeatingCapacity = request.SeatingCapacity;
        user.VehicleType = request.VehicleType;

        if (request.DriverInformationImages.Count == 1)
            throw new BadRequestException(ErrorMessages.RequireBothSidesUploaded);

        user.VehicleImages = await RefreshUploadedFileList.RefreshUploadedFiles(user.VehicleImages,
            request.VehicleImages,
            uploadFileService);

        if (request.DriverInformationImages.Count == 2)
            user.DriverInformationImages = await RefreshUploadedFileList.RefreshUploadedFiles(
                user.DriverInformationImages,
                request.DriverInformationImages,
                uploadFileService
            );

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task AddDeviceToken(Guid userId, string deviceToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        var oldUser = await context.Users
            .FirstOrDefaultAsync(u =>
                EF.Functions.JsonContains(u.DeviceTokens, $"[\"{deviceToken}\"]")
                && u.Id != userId);

        if (oldUser != null)
        {
            oldUser.DeviceTokens = oldUser.DeviceTokens
                .Where(t => t != deviceToken)
                .ToArray();

            context.Users.Update(oldUser);
            await context.SaveChangesAsync();
        }

        if (!user.DeviceTokens.Contains(deviceToken))
        {
            user.DeviceTokens = user.DeviceTokens.Append(deviceToken).ToArray();
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<string[]> GetDeviceTokens(Guid userId)
    {
        return (await context.Users
            .Where(x => x.Id == userId)
            .Select(u => u.DeviceTokens)
            .FirstOrDefaultAsync()) ?? [];
    }

    public async Task SendVerifyEmail(Guid userId)
    {
        await userBanService.CheckUserBaned(userId, BanType.SendVerifyEmail, true);

        var user = await context
            .Users
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);
        if (user.AccountStatus != AccountStatus.New)
            throw new BadRequestException("Bạn không thể xác thực email.");
        if (string.IsNullOrEmpty(user.Email))
            throw new BadRequestException("Bạn chưa nhập thông tin email.");

        var mailContent = new SendMailDto();
        using (var str = new StreamReader(Constants.RootPathMailTemplate + "/VerifyEmail.html"))
        {
            mailContent.Body = await str.ReadToEndAsync();
        }

        var claims = new List<Claim>();
        claims.AddRange([
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimType.TokenType, TokenType.VerifyEmail.ToString())
        ]);

        var verifyUrl =
            $"{appSettings.Value.ClientPath}/registrationConfirm?token={tokenService.GenerateAccessToken(claims, 24)}";
        mailContent.Body = mailContent.Body.Replace("{{userName}}", user.FirstName + user.LastName);
        mailContent.Body = mailContent.Body.Replace("{{verifyUrl}}", verifyUrl);
        mailContent.To = user.Email;
        mailContent.Subject = "Xác thực email";

        await mailService.SendEmail(mailContent);

        await userBanService.AddErrorRequest(userId, BanType.SendVerifyEmail);
    }

    public async Task VerifyEmail(string token)
    {
        var tokenValidationResult = tokenService.ValidateToken(token, TokenType.VerifyEmail);

        var user = await context
            .Users.FirstOrDefaultAsync(x => x.Id == tokenValidationResult.UserId);

        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        user.AccountStatus = AccountStatus.Verified;
        user.VerificationMethod = VerificationMethod.Email;
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task SendForgetPasswordEmail(SendForgetPasswordEmailDto dto)
    {
        var user = await context
            .Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.UserName == dto.Username);

        if (user == null)
            throw new BadRequestException("Không thể gửi email quên mật khẩu.");
        var mailContent = new SendMailDto();
        using (var str = new StreamReader(Constants.RootPathMailTemplate + "/ForgotPasswordEmail.html"))
        {
            mailContent.Body = await str.ReadToEndAsync();
        }

        var claims = new List<Claim>();
        claims.AddRange([
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimType.TokenType, TokenType.ForgotEmail.ToString())
        ]);

        var verifyUrl =
            $"{appSettings.Value.ClientPath}/registrationConfirm?token={tokenService.GenerateAccessToken(claims, 24)}";
        mailContent.Body = mailContent.Body.Replace("{{userName}}", user.FirstName + user.LastName);
        mailContent.Body = mailContent.Body.Replace("{{verifyUrl}}", verifyUrl);
        mailContent.To = user.Email;
        mailContent.Subject = "Xác thực email";

        await mailService.SendEmail(mailContent);
    }

    public Task ResetPassword(ResetPasswordDto dto)
    {
        throw new NotImplementedException();
    }

    private static bool IsVerified(User user)
    {
        return user is { AccountStatus: AccountStatus.Verified, UserType: UserType.Parent or UserType.Driver };
    }

    private async Task ValidateUserInput(UpdateUserInfoDto dto, User user)
    {
        if (string.IsNullOrEmpty(dto.Email) && string.IsNullOrEmpty(dto.PhoneNumber))
            throw new BadRequestException(ErrorMessages.EmailOrPhoneRequired);

        if (!string.IsNullOrEmpty(dto.Email) &&
            user.Email != dto.Email &&
            await context.Users.AnyAsync(x => x.Email == dto.Email)
           )
            throw new BadRequestException(ErrorMessages.EmailExists);

        if (!string.IsNullOrEmpty(dto.PhoneNumber) &&
            user.PhoneNumber != dto.PhoneNumber &&
            await context.Users.AnyAsync(x => x.PhoneNumber == dto.PhoneNumber)
           )
            throw new BadRequestException(ErrorMessages.PhoneExists);
    }
}