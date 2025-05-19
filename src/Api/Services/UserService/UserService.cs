using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UploadFileService;
using Api.DTOs.User;
using Api.Extensions;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Requests;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UserService;

public class UserService(Context context, IFileUploadService uploadFileService) : IUserService
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
            _ = uploadFileService.RollBackAsync();
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
        var user = await context.Drivers.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        var oldUser = await context.Users
            .FirstOrDefaultAsync(u => EF.Functions.JsonContains(u.DeviceTokens, $"[\"{deviceToken}\"]"));

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
        return (await context.Drivers
            .Where(x => x.Id == userId)
            .Select(u => u.DeviceTokens)
            .FirstOrDefaultAsync()) ?? [];
    }

    private static bool IsVerified(User user)
    {
        return user.AccountStatus == AccountStatus.Verified &&
               user.UserType is UserType.Parent or UserType.Driver;
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