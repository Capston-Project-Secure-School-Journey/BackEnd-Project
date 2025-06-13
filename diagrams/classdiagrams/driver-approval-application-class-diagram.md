classDiagram
direction TB
    class DriverApprovalApplicationController {
	    -IApplicationHandler _applicationHandler
	    +GetApplication(Guid) : Task~ApplicationResponse~
	    +GetApplications(GetDriverApprovalApplication) : Task~Pagination~ApplicationResponse~~
	    +GetActionsCanDo(Guid) : Task~List~ApplicationActionDto~~
	    +CreateApplication(Guid) : Task~ApplicationResponse~
	    +UpdateApplication(Guid) : Task~ApplicationResponse~
	    +SubmitApplication(Guid) : Task~ActionResult~
	    +DeleteApplication(Guid) : Task~ActionResult~
	    +RejectApplication(Guid, RejectApplicationRequest) : Task~ActionResult~
	    +ApproveApplication(Guid, ApproveApplicationRequest) : Task~ActionResult~
	    +RequestMoreInfo(Guid, RequestMoreInfoRequest) : Task~ActionResult~
	    +RequestCancellation(Guid, CancelApplicationRequest) : Task~ActionResult~
	    +CancelApplication(Guid, CancelApplicationRequest) : Task~ActionResult~
    }

    class IApplicationHandler {
	    +GetApplicationsBySchool(Guid, GetDriverApprovalApplication) : Task
	    +GetApplicationsByDriver(Guid, GetDriverApprovalApplication) : Task
	    +GetApplication(Guid) : Task~ApplicationResponse~
	    +CreateApplication(Guid, Guid) : Task~ApplicationResponse~
	    +UpdateApplication(Guid, Guid) : Task~ApplicationResponse~
	    +SubmitApplication(Guid, Guid) : Task
	    +ApproveApplication(Guid, Guid) : Task
	    +RejectApplication(Guid, Guid, string) : Task
	    +RequireAdditionalDetails(Guid, Guid, string) : Task
	    +RequestCancellationByReviewer(Guid, Guid, string) : Task
	    +RequestCancellationByDriver(Guid, Guid, string) : Task
	    +CancelApplication(Guid, Guid) : Task
	    +DeleteApplicationByDriver(Guid, Guid) : Task
	    +GetActionCanDoByReviewer(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +GetActionCanDoByDriver(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +IsDriverOwnerOfApplication(Guid, Guid) : Task
	    +IsSchoolOwnerOfApplication(Guid, Guid) : Task
    }

    class IApplicationService {
	    +GetApplicationsBySchool(Guid, RequestStatus?) : Task
	    +GetApplicationsByDriver(Guid, RequestStatus?) : Task
	    +GetApplication(Guid) : Task~DriverApprovalRequest~
    }

    class IApprovalProcessor {
	    +CreateApplication(Guid, Guid) : Task~DriverApprovalRequest~
	    +UpdateApplication(Guid, Guid) : Task~DriverApprovalRequest~
	    +SubmitApplication(Guid, Guid) : Task
	    +ApproveApplication(Guid, Guid) : Task
	    +RejectApplication(Guid, Guid, string) : Task
	    +RequireAdditionalDetails(Guid, Guid, string) : Task
	    +RequestCancellationByReviewer(Guid, Guid, string) : Task
	    +RequestCancellationByDriver(Guid, Guid, string) : Task
	    +CancelApplication(Guid, Guid) : Task
	    +DeleteApplicationByDriver(Guid, Guid) : Task
	    +GetActionCanDoByReviewer(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +GetActionCanDoByDriver(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +GetReviewerOfSchool(Guid) : Task~Guid~
    }

    class ApplicationHandler {
	    -Context _context
	    -IApplicationService _applicationService
	    -IApprovalProcessor _approvalProcessor
	    -IMapper _mapper
	    -IFileUploadService _fileUploadService
	    -ICurrentUserProvider _currentUserProvider
	    +GetApplicationsBySchool(Guid, GetDriverApprovalApplication) : Task
	    +GetApplicationsByDriver(Guid, GetDriverApprovalApplication) : Task
	    +GetApplication(Guid) : Task~ApplicationResponse~
	    +CreateApplication(Guid, Guid) : Task~ApplicationResponse~
	    +UpdateApplication(Guid, Guid) : Task~ApplicationResponse~
	    +SubmitApplication(Guid, Guid) : Task
	    +ApproveApplication(Guid, Guid) : Task
	    +RejectApplication(Guid, Guid, string) : Task
	    +RequireAdditionalDetails(Guid, Guid, string) : Task
	    +RequestCancellationByReviewer(Guid, Guid, string) : Task
	    +RequestCancellationByDriver(Guid, Guid, string) : Task
	    +CancelApplication(Guid, Guid) : Task
	    +DeleteApplicationByDriver(Guid, Guid) : Task
	    +GetActionCanDoByReviewer(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +GetActionCanDoByDriver(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +IsDriverOwnerOfApplication(Guid, Guid) : Task
	    +IsSchoolOwnerOfApplication(Guid, Guid) : Task
	    -MapToResponse(DriverApprovalRequest) : Task~ApplicationResponse~
    }

    class ApplicationService {
	    -Context _context
	    +GetApplicationsBySchool(Guid, RequestStatus?) : Task
	    +GetApplicationsByDriver(Guid, RequestStatus?) : Task
	    +GetApplication(Guid) : Task~DriverApprovalRequest~
	    +GetApplicationNotificationMessage(DriverApprovalRequest) : string
    }

    class ApprovalProcessor {
	    -Context _context
	    -IFileUploadService _fileUploadService
	    -ISchoolManagement _schoolManagement
	    +CreateApplication(Guid, Guid) : Task~DriverApprovalRequest~
	    +UpdateApplication(Guid, Guid) : Task~DriverApprovalRequest~
	    +SubmitApplication(Guid, Guid) : Task
	    +ApproveApplication(Guid, Guid) : Task
	    +RejectApplication(Guid, Guid, string) : Task
	    +RequireAdditionalDetails(Guid, Guid, string) : Task
	    +RequestCancellationByReviewer(Guid, Guid, string) : Task
	    +RequestCancellationByDriver(Guid, Guid, string) : Task
	    +CancelApplication(Guid, Guid) : Task
	    +DeleteApplicationByDriver(Guid, Guid) : Task
	    +GetActionCanDoByReviewer(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +GetActionCanDoByDriver(Guid, Guid) : Task~List~ApplicationActionDto~~
	    +GetReviewerOfSchool(Guid) : Task~Guid~
	    -GetApplicationByDriver(Guid, Guid) : Task~DriverApprovalRequest~
	    -GetApplicationByReviewer(Guid, Guid) : Task~DriverApprovalRequest~
	    -ValidateDriverInfo(Driver) : void
	    -CanCreateApplication(Driver) : Task
	    -GetActionCanDo(DriverApprovalRequest, bool) : List~ApplicationActionDto~
    }

    class DriverApprovalRequest {
	    +Guid Id
	    +Guid SchoolId
	    +DateTime RequestedDate
	    +string MotivationLetter
	    +Guid DriverId
	    +RequestStatus RequestStatus
	    +Guid? ApprovedBy
	    +string VehicleType
	    +string LicenseNumber
	    +int SeatingCapacity
	    +DateTime? LastCheckDrivingLicense
	    +List~DriverInformationImage~ DriverInformationImages
	    +List~FileMetadata~ VehicleImages
	    +Driver? Driver
	    +HashSet~DriverRequestStatusHistory~ DriverRequestStatusHistories
	    +School? School
    }

    class DriverRequestStatusHistory {
	    +Guid Id
	    +Guid RequestId
	    +RequestStatus? FromStatus
	    +RequestStatus ToStatus
	    +Guid ChangedBy
	    +DateTime ChangedAt
	    +string Note
	    +DriverApprovalRequest Request
    }

    class Context {
    }

    class DriverInformationImage {
    }

    class FileMetadata {
    }

	<<interface>> IApplicationHandler
	<<interface>> IApplicationService
	<<interface>> IApprovalProcessor

    ApplicationHandler --|> IApplicationHandler : implements
    ApplicationService --|> IApplicationService : implements
    ApprovalProcessor --|> IApprovalProcessor : implements
    DriverApprovalApplicationController --> IApplicationHandler : uses >
    ApplicationHandler --> IApplicationService : uses >
    ApplicationHandler --> IApprovalProcessor : uses >
    ApprovalProcessor --> Context : uses >
    DriverApprovalRequest --* DriverRequestStatusHistory : contains >
    DriverApprovalRequest --* DriverInformationImage : contains >
    DriverApprovalRequest --* FileMetadata : contains >
