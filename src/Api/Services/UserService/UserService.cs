using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.User;
using Api.Services.UploadFileService;
using Api.TransferDTOs.Requests;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UserService;

public class UserService : IUserService
{
    private readonly Context _context;
    private readonly IFileUploadService _uploadFileService;

    public UserService(Context context, IFileUploadService uploadFileService)
    {
        _context = context;
        _uploadFileService = uploadFileService;
    }

    public async Task<User> GetUser(Guid id, UserType userType)
    {
        User? user = null;

        if (userType is UserType.SchoolAdmin or UserType.SchoolSuperVisor)
            user = await _context.SchoolPersons.FirstOrDefaultAsync(x => x.Id == id);
        else if (userType == UserType.Parent)
            user = await _context.Parents.FirstOrDefaultAsync(x => x.Id == id);
        else if (userType == UserType.Driver)
            user = await _context.Drivers.FirstOrDefaultAsync(x => x.Id == id);
        else if (userType == UserType.Admin) user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException("Không tìm thấy người dùng.");

        return user;
    }

    public async Task<User> UpdateUserInfo(Guid id, UpdateUserInfoDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException("Không tìm thấy người dùng.");

        if (user.AccountStatus == AccountStatus.Verified &&
            (user.UserType == UserType.Parent || user.UserType == UserType.Driver))
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


        if (string.IsNullOrEmpty(dto.Email) && string.IsNullOrEmpty(dto.PhoneNumber))
            throw new BadRequestException("Email và số điện thoại đều trống. Vui lòng điền ít nhất 1.");

        if (!string.IsNullOrEmpty(dto.Email) && user.Email != dto.Email)
            if (_context.Users.Any(x => x.Email == dto.Email))
                throw new BadRequestException("Email đã được đăng kí.");

        if (!string.IsNullOrEmpty(dto.PhoneNumber) && user.PhoneNumber != dto.PhoneNumber)
            if (_context.Users.Any(x => x.PhoneNumber == dto.PhoneNumber))
                throw new BadRequestException("Số điện thoại đã được đăng kí.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.DateOfBirth = dto.DateOfBirth;
        user.Gender = dto.Gender;
        user.PhoneNumber = dto.PhoneNumber;
        user.Email = dto.Email;
        user.Address = dto.Address;
        user.DetailAddress = dto.DetailAddress;
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<string> UpdateAvatar(Guid id, IFormFile file)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new NotFoundException("Không tìm thấy người dùng.");

        if (user.AvatarKey != null)
            await _uploadFileService.DeleteFileAsync(user.AvatarKey.Value);
        var response = await _uploadFileService.UploadFileAsync(file, "avatar");

        user.AvatarKey = response.Key;
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return response.S3Url;
    }

    public async Task<User> UpdateDriverInformation(Guid id, UpdateDriverInformationRequest request)
    {
        
        var user = await _context.Drivers.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            throw new NotFoundException("Không tìm thấy người dùng.");


        user.LicenseNumber = request.LicenseNumber;
        user.SeatingCapacity = request.SeatingCapacity;
        user.VehicleType = request.VehicleType;
        
        if (request.DriverInformationImages.Count == 1)
        {
            throw new BadRequestException("Bạn cần tải cả mặt trước và mặt sau.");
        }
        
        user.VehicleImages = await UpdateListFiles(user.VehicleImages, request.VehicleImages);
        if (request.DriverInformationImages.Count == 2)
        {
            user.DriverInformationImages = await UpdateListFiles(
                user.DriverInformationImages,
                request.DriverInformationImages
                );
        }
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<List<FileMetadata>> UpdateListFiles(List<FileMetadata> currentFiles, List<Guid> newFiles)
    {
        var baseUploadService = _uploadFileService as BaseUploadFileService;
        var oldFileData = currentFiles;
        var newList = new List<FileMetadata>();
        var keepList = new List<FileMetadata>();

        foreach (var i in newFiles)
        {
            var fileData = await baseUploadService!.GetFileData(i);

            if (fileData == null)
                throw new BadRequestException("Tải ảnh xe không thành công.");

            if (oldFileData.Any(x => x.FileManagementId == fileData.Id))
                keepList.Add(oldFileData.First(x => x.FileManagementId == fileData.Id));
            else
            {
                if (fileData.IsUploaded == false)
                    newList.Add(new FileMetadata() { Key = fileData.S3Key, FileManagementId = fileData.Id });
                else
                    throw new BadRequestException("Tải ảnh xe không thành công.");
            }
        }

        var removeList = oldFileData.Except(keepList).ToList();
        foreach (var i in removeList)
        {
            await _uploadFileService.DeleteFileAsync(i.FileManagementId);
            currentFiles.Remove(i);
        }

        foreach (var t in newList)
        {
            await baseUploadService!.MarkFileAsUploadedAsync(t.FileManagementId);
            currentFiles.Add(t);
        }
        
        return currentFiles;
    }
    
    private async Task<List<DriverInformationImage>> UpdateListFiles(List<DriverInformationImage> currentFiles, 
        List<(Guid, DriverInformationImageType)> newFiles)
    {
        var baseUploadService = _uploadFileService as BaseUploadFileService;
        var oldFileData = currentFiles;
        var newList = new List<DriverInformationImage>();
        var keepList = new List<DriverInformationImage>();

        foreach (var i in newFiles)
        {
            var fileData = await baseUploadService!.GetFileData(i.Item1);

            if (fileData == null)
                throw new BadRequestException("Tải ảnh bằng lái không thành công.");

            if (oldFileData.Any(x => x.FileManagementId == fileData.Id))
                keepList.Add(oldFileData.First(x => x.FileManagementId == fileData.Id));
            else
            {
                if (fileData.IsUploaded == false)
                    newList.Add(new DriverInformationImage()
                    {
                        Key = fileData.S3Key, 
                        FileManagementId = fileData.Id ,
                        Type = i.Item2
                    });
                else
                    throw new BadRequestException("Tải ảnh bằng lái không thành công.");
            }
        }

        var removeList = oldFileData.Except(keepList).ToList();
        foreach (var i in removeList)
        {
            await _uploadFileService.DeleteFileAsync(i.FileManagementId);
            currentFiles.Remove(i);
        }

        foreach (var t in newList)
        {
            await baseUploadService!.MarkFileAsUploadedAsync(t.FileManagementId);
            currentFiles.Add(t);
        }
        
        return currentFiles;
    }
}