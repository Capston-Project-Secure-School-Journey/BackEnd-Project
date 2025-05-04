using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.ChildrenManagement;
using Api.Extensions;
using Api.Services.UploadFileService;
using Api.Services.UserBanService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ChildrenManagementService;

public class ChildrenManagementService : IChildrenManagementService
{
    private readonly Context _context;
    private readonly IMapper _mapper;
    private readonly IFileUploadService _uploadFileService;
    private readonly IUserBanService _userBanService;

    public ChildrenManagementService(Context context, IMapper mapper,
        IFileUploadService uploadFileService,
        IUserBanService userBanService)
    {
        _context = context;
        _mapper = mapper;
        _uploadFileService = uploadFileService;
        _userBanService = userBanService;
    }


    public async Task<IEnumerable<ChildDto>> GetMyChildren(Guid parentId)
    {
        var parent = await GetParent(parentId);

        if (parent.RelationshipWithStudents.Count == 0)
            return new List<ChildDto>();

        var childs = parent.RelationshipWithStudents
            .Select(x => new { x.StudentId, x.Relationship, x.IsFirstAdded })
            .ToList();

        IQueryable<Student>? children = null;
        foreach (var id in childs.Select(x => x.StudentId))
            if (children == null)
                children = _context.Students.AsQueryable().Where(x => x.Id == id);
            else
                children = children.Union(
                    _context.Students.AsQueryable().Where(x => x.Id == id));

        var response = (await children!
                .ToListAsync())
            .Select(MapStudentToChildDto)
            .ToList();

        foreach (var child in response) child.IsFirstAdded = childs.First(x => x.StudentId == child.Id).IsFirstAdded;
        return response;
    }

    public async Task<ChildDetailDto> GetChildById(Guid parentId, Guid childId)
    {
        var parent = await GetParent(parentId);

        if (parent.RelationshipWithStudents.Count == 0)
            throw new BadRequestException(ErrorMessages.NoStudentAdded);

        if (parent.RelationshipWithStudents.All(x => x.StudentId != childId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);

        var child = await _context.Students.FirstOrDefaultAsync(x => x.Id == childId);

        var response = await MapStudentToChildDetailDto(child!);
        response.IsFirstAdded = parent.RelationshipWithStudents.First(x => x.StudentId == child!.Id).IsFirstAdded;
        return response;
    }

    public async Task RegisterChild(Guid parentId, RegisterChildDto dto)
    {
        await _userBanService.CheckUserBaned(parentId, BanType.AddChild, true);

        var student = FindStudentWithHash(dto.SecretCode);
        var parent = await GetParent(parentId);

        if (!student.FirstName.Equals(dto.FirstName, StringComparison.CurrentCultureIgnoreCase) ||
            !student.LastName.Equals(dto.LastName, StringComparison.CurrentCultureIgnoreCase) ||
            student.DateOfBirth != dto.DateOfBirth)
        {
            await _userBanService.AddErrorRequest(parent.Id, BanType.AddChild);
            throw new BadRequestException(ErrorMessages.StudentInfoMismatch);
        }

        await _userBanService.RemoveUserBan(parentId, BanType.AddChild);
        if (parent.RelationshipWithStudents.Count == 0)
        {
            parent.RelationshipWithStudents = [];
        }
        else
        {
            if (parent.RelationshipWithStudents.Any(x => x.StudentId == student.Id))
                throw new BadRequestException(ErrorMessages.StudentAlreadyAdded);
        }

        var isFirstAdded = await _context.Parents
            .FromSqlRaw(
                @"SELECT * FROM users 
                  WHERE discriminator = 'parent' 
                    and IsDeleted = 0 
                    and  JSON_CONTAINS(RelationshipWithStudents, JSON_OBJECT('StudentId', {0}))",
                student.Id)
            .AnyAsync();

        parent.RelationshipWithStudents.Add(
            new RelationshipWithStudent()
            {
                StudentId = student.Id,
                Relationship = dto.Relationship,
                IsFirstAdded = !isFirstAdded
            });

        _context.Parents.Update(parent);
        await _context.SaveChangesAsync();
    }

    public async Task<string> UpdateChildPickupLocation(Guid parentId, UpdateChildPickupLocationDto dto)
    {
        var parent = await GetParent(parentId);
        if (parent.RelationshipWithStudents.All(x => x.StudentId != dto.ChildId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);

        if (!parent.RelationshipWithStudents.First(x => x.StudentId == dto.ChildId).IsFirstAdded)
            throw new BadRequestException(ErrorMessages.OnlyFirstParentCanEditAddress);

        var child = await _context.Students.FirstOrDefaultAsync(x => x.Id == dto.ChildId);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        child.PickUpLocation = dto.PickUpLocation;
        child.PickUpLat = dto.PickUpLat;
        child.PickUpLng = dto.PickUpLng;
        // UTC
        child.LastTimeUpdatedPickupLocation = DateTimeHelper.GetDateTimeUtc7();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        _context.Students.Update(child);
        await _context.SaveChangesAsync();

        var date = DateTimeHelper.GetDateTimeUtc7();
        if (date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(2);

        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfCurrentWeek = date.AddDays(-diff).Date;
        var startOfNextWeek = DateOnly.FromDateTime(startOfCurrentWeek.AddDays(7));

        return
            $"Địa chỉ có hiệu lực từ ngày: {startOfNextWeek.ToShortDateString()}. Vì vậy hãy đón con tại địa chỉ cũ.";
    }

    private Student FindStudentWithHash(string hash)
    {
        var studentId = _context.Students
            .Select(st => st.Id)
            .AsEnumerable()
            .FirstOrDefault(id => HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(id)) == hash);

        if (studentId == Guid.Empty)
            throw new BadRequestException(ErrorMessages.ErrorDuringProcessing);

        var studentEntity = _context.Students.FirstOrDefault(st => st.Id == studentId);
        return studentEntity!;
    }

    private async Task<Parent> GetParent(Guid parentId)
    {
        var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Id == parentId);

        if (parent == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        return parent;
    }

    private async Task<ChildDetailDto> MapStudentToChildDetailDto(Student child)
    {
        if (!_context.Entry(child).Reference(x => x.School).IsLoaded)
            await _context.Entry(child).Reference(x => x.School).LoadAsync();
        if (!_context.Entry(child).Reference(x => x.Class).IsLoaded)
            await _context.Entry(child).Reference(x => x.Class).LoadAsync();

        var response = _mapper.Map<ChildDetailDto>(child);
        response.SchoolName = child.School.SchoolName;
        response.ClassName = child.Class.ClassName;

        if (child.AvatarKey != null)
            response.AvatarUrl = await _uploadFileService
                .GeneratePreSignedDownloadUrlAsync(child.AvatarKey.Value, 30);
        return response;
    }

    private ChildDto MapStudentToChildDto(Student child)
    {
        if (!_context.Entry(child).Reference(x => x.School).IsLoaded)
            _context.Entry(child).Reference(x => x.School).Load();
        var response = _mapper.Map<ChildDto>(child);
        response.SchoolName = child.School.SchoolName;

        return response;
    }
}