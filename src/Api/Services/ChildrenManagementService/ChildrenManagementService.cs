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

public class ChildrenManagementService(
    Context context,
    IMapper mapper,
    IFileUploadService uploadFileService,
    IUserBanService userBanService,
    GoogleMapsService googleMapsService)
    : IChildrenManagementService
{
    public async Task<IEnumerable<ChildDto>> GetMyChildren(Guid parentId)
    {
        var parent = await GetParent(parentId);

        if (parent.RelationshipWithStudents.Count == 0)
            return new List<ChildDto>();

        var children = parent.RelationshipWithStudents
            .Select(x => new { x.StudentId, x.Relationship, x.IsFirstAdded })
            .ToList();
        
        IQueryable<Student>? childrenDetail = null;
        foreach (var id in children.Select(x => x.StudentId))
            if (childrenDetail == null)
                childrenDetail = context.Students.AsQueryable().Where(x => x.Id == id);
            else
                childrenDetail = childrenDetail.Union(
                    context.Students.AsQueryable().Where(x => x.Id == id));

        if (childrenDetail == null) return new List<ChildDto>();
        
        var response = (await childrenDetail
                .ToListAsync())
            .Select(MapStudentToChildDto)
            .OrderBy(c => c.FullName)
            .ToList();

        foreach (var child in response) child.IsFirstAdded = children.First(x => x.StudentId == child.Id).IsFirstAdded;
        return response;
    }

    public async Task<ChildDetailDto> GetChildById(Guid parentId, Guid childId)
    {
        var parent = await GetParent(parentId);

        if (parent.RelationshipWithStudents.Count == 0)
            throw new BadRequestException(ErrorMessages.NoStudentAdded);

        if (parent.RelationshipWithStudents.All(x => x.StudentId != childId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);

        var child = await context.Students.FirstOrDefaultAsync(x => x.Id == childId);

        var response = await MapStudentToChildDetailDto(child!);
        response.IsFirstAdded = parent.RelationshipWithStudents.First(x => x.StudentId == child!.Id).IsFirstAdded;
        return response;
    }

    public async Task RegisterChild(Guid parentId, RegisterChildDto dto)
    {
        await userBanService.CheckUserBaned(parentId, BanType.AddChild, true);

        var student = FindStudentWithHash(dto.SecretCode);
        var parent = await GetParent(parentId);

        if (!student.FirstName.Equals(dto.FirstName, StringComparison.CurrentCultureIgnoreCase) ||
            !student.LastName.Equals(dto.LastName, StringComparison.CurrentCultureIgnoreCase) ||
            student.DateOfBirth != dto.DateOfBirth)
        {
            await userBanService.AddErrorRequest(parent.Id, BanType.AddChild);
            throw new BadRequestException(ErrorMessages.StudentInfoMismatch);
        }

        await userBanService.RemoveUserBan(parentId, BanType.AddChild);
        if (parent.RelationshipWithStudents.Count == 0)
        {
            parent.RelationshipWithStudents = [];
        }
        else
        {
            if (parent.RelationshipWithStudents.Any(x => x.StudentId == student.Id))
                throw new BadRequestException(ErrorMessages.StudentAlreadyAdded);
        }

        var isFirstAdded = await context.Parents
            .FromSqlRaw(
                @"SELECT * FROM users 
                  WHERE discriminator = 'parent' 
                    and IsDeleted = 0 
                    and  JSON_CONTAINS(relationship_with_students, JSON_OBJECT('StudentId', {0}))",
                student.Id)
            .AnyAsync();

        parent.RelationshipWithStudents.Add(
            new RelationshipWithStudent()
            {
                StudentId = student.Id,
                Relationship = dto.Relationship,
                IsFirstAdded = !isFirstAdded
            });

        context.Parents.Update(parent);
        await context.SaveChangesAsync();
    }

    public async Task<string> UpdateChildPickupLocation(Guid parentId, UpdateChildPickupLocationDto dto)
    {
        var parent = await GetParent(parentId);
        if (parent.RelationshipWithStudents.All(x => x.StudentId != dto.ChildId))
            throw new ForbiddenException(ErrorMessages.AccessDenied);

        if (!parent.RelationshipWithStudents.First(x => x.StudentId == dto.ChildId).IsFirstAdded)
            throw new BadRequestException(ErrorMessages.OnlyFirstParentCanEditAddress);

        var child = await context.Students.FirstOrDefaultAsync(x => x.Id == dto.ChildId);

        if (!await googleMapsService.IsCarAccessibleAddressAsync(dto.PickUpLocation))
            throw new BadRequestException("Địa chỉ này ôtô không thể đi vào.\n Vui lòng chọn địa chỉ khác.");
        
        var locationAddress = await googleMapsService.GetLatLngFromAddressAsync(dto.PickUpLocation);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        child.PickUpLocation = dto.PickUpLocation;
        child.PickUpLat = locationAddress.lat;
        child.PickUpLng = locationAddress.lng;
        // UTC
        child.LastTimeUpdatedPickupLocation = DateTimeHelper.GetDateTimeUtc7();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        context.Students.Update(child);
        await context.SaveChangesAsync();

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
        var studentId = context.Students
            .Select(st => st.Id)
            .AsEnumerable()
            .FirstOrDefault(id => HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(id)) == hash);

        if (studentId == Guid.Empty)
            throw new BadRequestException(ErrorMessages.ErrorDuringProcessing);

        var studentEntity = context.Students.FirstOrDefault(st => st.Id == studentId);
        return studentEntity!;
    }

    private async Task<Parent> GetParent(Guid parentId)
    {
        var parent = await context.Parents.FirstOrDefaultAsync(p => p.Id == parentId);

        if (parent == null)
            throw new NotFoundException(ErrorMessages.UserNotFound);

        return parent;
    }

    private async Task<ChildDetailDto> MapStudentToChildDetailDto(Student child)
    {
        if (!context.Entry(child).Reference(x => x.School).IsLoaded)
            await context.Entry(child).Reference(x => x.School).LoadAsync();
        if (!context.Entry(child).Reference(x => x.Class).IsLoaded)
            await context.Entry(child).Reference(x => x.Class).LoadAsync();

        var response = mapper.Map<ChildDetailDto>(child);
        response.SchoolName = child.School.SchoolName;
        response.ClassName = child.Class.ClassName;

        if (child.AvatarKey != null)
            response.AvatarUrl = await uploadFileService
                .GeneratePreSignedDownloadUrlAsync(child.AvatarKey.Value, 30);
        return response;
    }

    private ChildDto MapStudentToChildDto(Student child)
    {
        if (!context.Entry(child).Reference(x => x.School).IsLoaded)
            context.Entry(child).Reference(x => x.School).Load();
        var response = mapper.Map<ChildDto>(child);
        response.SchoolName = child.School.SchoolName;

        return response;
    }
}