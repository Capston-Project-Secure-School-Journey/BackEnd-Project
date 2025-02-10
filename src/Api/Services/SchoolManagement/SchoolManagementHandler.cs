using Api.Common.Enums;
using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.DTOs.SchoolManagement;
using Api.Transfers.Requests;
using Api.Transfers.Responses;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;

namespace Api.IOC.Services.SchoolManagement;

public class SchoolManagementHandler : ISchoolManagementHandler
{
    private readonly ISchoolManagement _schoolManagement;
    private readonly IMapper _mapper;
    private readonly Context _context;

    public SchoolManagementHandler(ISchoolManagement schoolManagement,
        IMapper mapper,
        Context context)
    {
        _schoolManagement = schoolManagement;
        _mapper = mapper;
        _context = context;
    }

    public async Task<SchoolResponse> CreateSchool(CreateSchoolRequest data)
    {
        var school = await _schoolManagement.CreateSchool(_mapper.Map<CreateSchoolDto>(data));
        return _mapper.Map<SchoolResponse>(school);
    }

    public async Task<SchoolResponse> UpdateSchool(Guid schoolId, UpdateSchoolRequest data, Guid userRequested,
        UserType userType)
    {
        if (userType == UserType.SchoolAdmin)
        {
            var user = _context.SchoolPersons.FirstOrDefault(sc => sc.Id == userRequested);
            if (user == null || (user != null && user.SchoolId != schoolId))
                throw new ForbiddenException("Access Denied");
        }

        var dto = _mapper.Map<UpdateSchoolDto>(data);
        dto.Id = schoolId;
        var school = await _schoolManagement.UpdateSchool(dto);
        return _mapper.Map<SchoolResponse>(school);
    }

    public async Task DeleteSchool(Guid schoolId)
    {
        await _schoolManagement.DeleteSchool(schoolId);
    }

    public async Task DeleteSchool(List<Guid> schoolIds)
    {
        await _schoolManagement.DeleteSchool(schoolIds);
    }

    public async Task<IEnumerable<SchoolResponse>> GetListOfSchool()
    {
        return (await _schoolManagement.GetListOfSchool())
            .Select(x => _mapper.Map<SchoolResponse>(x))
            .ToList();
    }
}