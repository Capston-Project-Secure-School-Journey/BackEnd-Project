using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain.Models;
using Api.Services.UploadFileService;

namespace Api.Extensions;

public static class RefreshUploadedFileList
{
    public static async Task<List<FileMetadata>> RefreshUploadedFiles(List<FileMetadata> currentFiles,
        List<Guid> newFiles, 
        IFileUploadService fileUploadService)
    {
        var baseUploadService = fileUploadService as BaseUploadFileService;
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
            await fileUploadService.DeleteFileAsync(i.FileManagementId);
            currentFiles.Remove(i);
        }

        foreach (var t in newList)
        {
            await baseUploadService!.MarkFileAsUploadedAsync(t.FileManagementId);
            currentFiles.Add(t);
        }
        return currentFiles;
    }
    
    public static async Task<List<DriverInformationImage>> RefreshUploadedFiles(List<DriverInformationImage> currentFiles, 
        List<(Guid, DriverInformationImageType)> newFiles,
        IFileUploadService fileUploadService)
    {
        var baseUploadService = fileUploadService as BaseUploadFileService;
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
            await fileUploadService.DeleteFileAsync(i.FileManagementId);
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