using Api.Domain.Models;
using Api.TransferDTOs.Responses;
using AutoMapper;

namespace Api.Mappings;

public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<DriverApprovalRequest, ApplicationResponse>()
            .ForMember(x => x.SchoolName,
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                opt => opt.MapFrom(app => app.School == null ? "" :app.School.SchoolName))
            .ForMember(x => x.DriverName,
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                opt => opt.MapFrom(app => app.Driver == null ? "" : app.Driver.FirstName + " " + app.Driver.LastName))
            .ForMember(x => x.VehicleImages,
                opt =>
                    opt.Ignore())
            .ForMember(x => x.DriverInformationImages,
                opt =>
                    opt.Ignore())
            .ForMember(x => x.DriverRequestStatusHistoryResponse,
                opt =>
                    opt.Ignore());

        CreateMap<DriverRequestStatusHistory, DriverRequestStatusHistoryResponse>();
    }
}