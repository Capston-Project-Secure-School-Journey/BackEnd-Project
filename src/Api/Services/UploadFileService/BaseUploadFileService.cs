using Api.Common.Exceptions;
using Api.Common.Utilities;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.UploadFileService;
using Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UploadFileService;

public class BaseUploadFileService(Context context, IUploadTransactionManager uploadTransactionManager)
{
    private readonly Context _context = context;
    protected readonly IUploadTransactionManager UploadTransactionManager = uploadTransactionManager;

    protected async Task<FileManagement> AddFileManagement(AddFileManagementDto data, bool preSign)
    {
        var file = new FileManagement()
        {
            FileName = data.FileName,
            FileSize = data.FileSize,
            S3Key = data.S3Key,
            UploadBy = data.UploadBy,
            RelatedObjectId = data.RelatedObjectId,
            RelatedObjectType = data.RelatedObjectType,
            FileType = data.FileType,
            UploadDate = DateTimeHelper.GetDateTimeUtc7(),
            IsUploaded = !preSign
        };

        _context.FileManagements.Add(file);
        _context.Entry(file).State = EntityState.Added;
        await _context.SaveChangesAsync();

        return file;
    }

    protected async Task DeleteFileManagement(Guid id)
    {
        var file = await _context.FileManagements.FirstOrDefaultAsync(t => t.Id == id);

        if (file != null)
        {
            _context.FileManagements.Remove(file);
            await _context.SaveChangesAsync();
        }
    }

    protected async Task<string> GetS3Key(Guid id)
    {
        var file = await CheckIfFileExist(id);
        return file.S3Key;
    }

    protected async Task<FileManagement> GetFileManagement(Guid id)
    {
        var file = await CheckIfFileExist(id);
        return file;
    }

    public async Task MarkFileAsUploadedAsync(Guid id)
    {
        var file = await CheckIfFileExist(id);
        file.IsUploaded = true;
        _context.FileManagements.Update(file);
        await _context.SaveChangesAsync();
    }

    public async Task<FileManagement> GetFileData(Guid id)
    {
        return await CheckIfFileExist(id);
    }

    private async Task<FileManagement> CheckIfFileExist(Guid id)
    {
        var file = await _context.FileManagements.FirstOrDefaultAsync(t => t.Id == id);

        if (file == null)
            throw new NotFoundException(ErrorMessages.FileNotFound(id));

        return file;
    }
}