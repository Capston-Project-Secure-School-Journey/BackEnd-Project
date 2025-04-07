using Api.Common.Enums;
using Api.Common.Utilities;
using Api.Common.Utilities.Exceptions;
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

        var childrenId = parent.RelationshipWithStudents
            .Select(x => new { x.StudentId, x.Relationship, x.IsFirstAdded })
            .ToList();

        IQueryable<Student>? children = null;
        foreach (var id in childrenId)
            if (children == null)
                children = _context.Students.AsQueryable().Where(x => x.Id == id.StudentId);
            else
                children = children.Union(
                    _context.Students.AsQueryable().Where(x => x.Id == id.StudentId));

        var response = children!
            .ToList()
            .Select(MapStudentToChildDto)
            .ToList();
        
        foreach (var child in response)
        {
            child.IsFirstAdded = childrenId.First(x => x.StudentId == child.Id).IsFirstAdded;
        }
        return response;
    }

    public async Task<ChildDetailDto> GetChildById(Guid parentId, Guid childId)
    {
        var parent = await GetParent(parentId);

        if (parent.RelationshipWithStudents.Count == 0)
            throw new BadRequestException("Bạn chưa thêm bất kì học sinh nào.");

        if (parent.RelationshipWithStudents.All(x => x.StudentId != childId))
            throw new BadRequestException("Bạn không có quyền truy cập");

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
            throw new BadRequestException(
                "Thông tin học sinh không trùng khớp. Lưu ý nếu sai quá 5 lần bạn sẽ bị cấm trong 24h");
        }
        await _userBanService.RemoveUserBan(parentId, BanType.AddChild);
        if (parent.RelationshipWithStudents.Count == 0)
        {
            parent.RelationshipWithStudents = [];
        }
        else
        {
            if (parent.RelationshipWithStudents.Any(x => x.StudentId == student.Id))
                throw new BadRequestException("Học sinh đã được thêm trước đây.");
        }

        var isFirstAdded = await _context.Parents
            .FromSqlRaw(
                @"SELECT * FROM users 
                  WHERE discriminator = 'parent' 
                    and IsDeleted = 0 
                    and  JSON_CONTAINS(RelationshipWithStudents, JSON_OBJECT('StudentId', {0})",
                student.Id)
            .AnyAsync();
        
        parent.RelationshipWithStudents.Add(
            new RelationshipWithStudent()
            {
                StudentId = student.Id,
                Relationship = dto.Relationship,
                IsFirstAdded = !isFirstAdded,
            });

        _context.Entry(parent).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<string> UpdateChildPickupLocation(Guid parentId, UpdateChildPickupLocationDto dto)
    {
        var parent = await GetParent(parentId);
        if (parent.RelationshipWithStudents.All(x => x.StudentId != dto.ChildId))
            throw new BadRequestException("Bạn không có quyền truy cập");
        
        if(!parent.RelationshipWithStudents.First(x => x.StudentId == dto.ChildId).IsFirstAdded)
            throw new BadRequestException("Chỉ có phụ huynh thêm con đầu tiên mới được chỉnh sửa địa chỉ.");
        
        var child = await _context.Students.FirstOrDefaultAsync(x => x.Id == dto.ChildId);
        
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        child.PickUpLocation = dto.PickUpLocation;
        child.PickUpLat = dto.PickUpLat;
        child.PickUpLng = dto.PickUpLng;
        // UTC
        child.LastTimeUpdatedPickupLocation = DateTimeHelper.GetDateTimeUtc7();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        _context.Entry(child).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        var date = DateTimeHelper.GetDateTimeUtc7();
        if (date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(2);
        
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime startOfCurrentWeek = date.AddDays(-diff).Date;
        DateOnly startOfNextWeek = DateOnly.FromDateTime(startOfCurrentWeek.AddDays(7));

        return $"Địa chỉ có hiệu lực từ ngày: {startOfNextWeek.ToShortDateString()}. Vì vậy hãy đón con tại địa chỉ cũ.";
    }

    private Student FindStudentWithHash(string hash)
    {
        var studentId = _context.Students
            .Select(st => st.Id)
            .AsEnumerable()
            .FirstOrDefault(id => HashGenerator.ComputeSha256(Constants.GetStudentStringToHash(id)) == hash);

        if (studentId == Guid.Empty)
            throw new BadRequestException("Đã xẩy ra lỗi trong quá trình xử lí");

        var studentEntity = _context.Students.FirstOrDefault(st => st.Id == studentId);
        return studentEntity!;
    }

    private async Task<Parent> GetParent(Guid parentId)
    {
        var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Id == parentId);

        if (parent == null)
            throw new NotFoundException("Không tìm thấy người dùng");

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