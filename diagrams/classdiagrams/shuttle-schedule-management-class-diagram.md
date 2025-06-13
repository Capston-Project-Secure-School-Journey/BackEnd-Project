classDiagram
    direction TB
    %% Main layers from top to bottom: Controller -> Handler -> Service -> Domain/Jobs

    %% Controllers
    class ShuttleScheduleManagementController {
        -IShuttleScheduleManagementHandler _handler
        +GetShuttleScheduleView(DateOnly): Task<ShuttleScheduleView>
        +GetShuttleScheduleByDate(GetShuttleScheduleByDateRequest): Task<Pagination<ShuttleScheduleResponse>>
        +GetShuttleSchedule(Guid): Task<ShuttleSchedule>
    }

    %% Handlers
    class ShuttleScheduleManagementHandler {
        -IShuttleScheduleManagementService _service
        -IMapper _mapper
        +GetShuttleScheduleView(Guid, DateOnly): Task<ShuttleScheduleView>
        +GetShuttleScheduleByDate(Guid, GetShuttleScheduleByDateRequest): Task<Pagination<ShuttleScheduleResponse>>
        +GetShuttleSchedule(Guid, Guid): Task<ShuttleSchedule>
    }

    %% Interfaces
    class IShuttleScheduleManagementHandler {
        <<interface>>
        +GetShuttleScheduleView(Guid, DateOnly): Task<ShuttleScheduleView>
        +GetShuttleScheduleByDate(Guid, GetShuttleScheduleByDateRequest): Task<Pagination<ShuttleScheduleResponse>>
        +GetShuttleSchedule(Guid, Guid): Task<ShuttleSchedule>
    }

    class IShuttleScheduleManagementService {
        <<interface>>
        +UpdateShuttleSchedule(ShuttleSchedule): Task
        +UpdateStudentOnShuttleSchedule(Guid, StudentOnBus): Task
        +AddShuttleSchedule(List<CreateShuttleScheduleDto>): Task<List<ShuttleSchedule>>
        +DeleteShuttleSchedule(Guid, DateOnly, DateOnly): Task
        +GetShuttleScheduleView(Guid, DateOnly): Task<ShuttleScheduleView>
        +GetShuttleScheduleByDate(Guid, GetShuttleScheduleByDateRequest): Task<IFindFluent<ShuttleSchedule, ShuttleSchedule>>
        +GetShuttleSchedule(Guid): Task<ShuttleSchedule>
        +IsOwnerOfShuttleSchedule(Guid, Guid): Task
    }

    %% Services
    class ShuttleScheduleManagementService {
        -Context _context
        -IUserService _userService
        -IFileUploadService _fileUploadService
        -ISchoolManagement _schoolManagement
        -IMemoryCache _cache
        -GoogleMapsService _googleMapsService
        -string CreateShuttleCacheKey
        +UpdateShuttleSchedule(ShuttleSchedule): Task
        +UpdateStudentOnShuttleSchedule(Guid, StudentOnBus): Task
        +AddShuttleSchedule(List<CreateShuttleScheduleDto>): Task<List<ShuttleSchedule>>
        +DeleteShuttleSchedule(Guid, DateOnly, DateOnly): Task
        +GetShuttleScheduleView(Guid, DateOnly): Task<ShuttleScheduleView>
        +GetShuttleScheduleByDate(Guid, GetShuttleScheduleByDateRequest): Task<IFindFluent<ShuttleSchedule, ShuttleSchedule>>
        +GetShuttleSchedule(Guid): Task<ShuttleSchedule>
        +IsOwnerOfShuttleSchedule(Guid, Guid): Task
        -CreateShuttleScheduleFromDto(CreateShuttleScheduleDto): Task<ShuttleSchedule>
        -GetBestRoute(ShuttleSchedule, School): Task
        -GetPickupStartTime(ShuttleScheduleType, SessionType, School): TimeSpan
        -GetPickupEndTime(ShuttleScheduleType, SessionType, School): TimeSpan
        -GetDriverInCache(Guid): Task<Driver>
        -GetSchoolInCache(Guid): Task<School>
        -GetImageUrl(Guid): Task<string>
    }

    %% Domain Models 
    class ShuttleSchedule {
        +Guid Id
        +Guid SchoolId
        +string SchoolName
        +ShuttleScheduleType Type
        +SessionType SessionType
        +DateOnly Date
        +Guid DriverId
        +string DriverPhoneNumber
        +string DriverName  
        +string DriverAvatar
        +string VehicleType
        +Gender DriverGender
        +string LicenseNumber
        +bool IsAllNotesRead
        +TimeSpan PickupStartTime
        +TimeSpan PickupEndTime
        +TimeSpan? StartJourneyTime
        +TimeSpan? EndJourneyTime
        +JourneyStatus JourneyStatus
        +string CancelReason
        +int NumberOfStudents
        +int NumberOfPickedUpStudents
        +int NumberOfDroppedOffStudents
        +double CurrentLat
        +double CurrentLng
        +BestRoute BestRoute
        +List<StudentOnBus> Students
        +double TotalDistanceKm
    }

    class BestRoute {
        +Point Origin
        +Point Destination  
        +List<Point> WayPoints
    }

    class Point {
        +string FullAddress
        +double Latitude
        +double Longitude
    }

    %% Jobs
    class CreateShuttleScheduleJob {
        -IServiceProvider _serviceProvider
        -ILogger<CreateShuttleScheduleJob> _logger
        -IStudentGroupingAlgorithm _groupingAlgorithm
        -IShuttleScheduleManagementService _service 
        -GoogleMapsService _googleMapsService
        +ExecuteAsync(object[]): Task
        -HasStudentMissingAddress(Context, Guid): Task
        -IsDriverCapacityInsufficient(Context, Guid): Task
        -GroupStudentsBySessionAsync(List<ClassSchedule>, Context, Guid): Task<Dictionary>
        -GetStudentsAsync(Dictionary<Guid,List<Student>>, Context, Guid, Guid): Task<List<Student>>
        -GetTotalDistance(CreateShuttleScheduleDto, School): Task<double>
        -ExpandSessionTypes(SessionType): IEnumerable<SessionType>
    }

    class AlertMissingAddressJob {
        -IServiceProvider _serviceProvider
        -ILogger<AlertMissingAddressJob> _logger
        +ExecuteAsync(object[]): Task
        -CreateCsvFile(List<Student>): MemoryStream
        -EscapeCsv(string): string
    }

    class AlertInsufficientDriversJob {
        -IServiceProvider _serviceProvider 
        -ILogger<AlertInsufficientDriversJob> _logger
        +ExecuteAsync(object[]): Task
        -CreateCsvFile(List<ActiveDriver>, int): MemoryStream
        -EscapeCsv(string): string
    }

    %% Message Types  
    class CreateNotificationDto {
        +string Title
        +string Content  
        +Guid RecipientId
        +string Navigation
    }

    class SendNotificationJob {
        +ExecuteAsync(List<Guid>, Dictionary~string,string~): Task
    }

    %% Relationships
    %% Inheritance
    ShuttleScheduleManagementHandler --|> IShuttleScheduleManagementHandler : implements
    ShuttleScheduleManagementService --|> IShuttleScheduleManagementService : implements

    %% Association
    ShuttleScheduleManagementController --> IShuttleScheduleManagementHandler : uses >
    ShuttleScheduleManagementHandler --> IShuttleScheduleManagementService : uses >
    CreateShuttleScheduleJob --> IShuttleScheduleManagementService : uses >
    ShuttleSchedule --> BestRoute : has >
    BestRoute --> Point : has >

    %% Composition
    ShuttleSchedule --* StudentOnBus : contains <
    ShuttleSchedule --* BestRoute : contains <

    %% Additional Relationships
    AlertMissingAddressJob --> CreateNotificationDto : creates >
    AlertInsufficientDriversJob --> CreateNotificationDto : creates >
    AlertMissingAddressJob --> SendNotificationJob : enqueues >
    AlertInsufficientDriversJob --> SendNotificationJob : enqueues >