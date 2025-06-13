classDiagram
    direction TB
    %% Controllers
    class DriverSchoolTripController {
        +GetShuttleScheduleByDate(date)
        +GetShuttleSchedule(shuttleScheduleId)
        +StartJourney(shuttleScheduleId)
        +EndJourney(shuttleScheduleId)
        +CancelJourney(shuttleScheduleId, request)
        +SkipStudent(shuttleScheduleId, request)
        +HasInProgressShuttle()
        +GetCurrentShuttleScheduleByDriver()
        +HasUpcomingShuttle()
        +GetUpcomingShuttleSchedule()
        +UpdateCurrentLocation(shuttleScheduleId, request)
    }
    
    %% Handlers
    class DriverSchoolTripHandler {
        +GetShuttleScheduleByDate(driverId, date)
        +GetShuttleSchedule(shuttleScheduleId, driverId)
        +StartJourney(shuttleScheduleId, driverId)
        +EndJourney(shuttleScheduleId, driverId)
        +CancelJourney(shuttleScheduleId, driverId, cancelReason)
        +SkipStudent(shuttleScheduleId, driverId, studentId, cancelReason)
        +HasInProgressShuttle(driverId)
        +GetCurrentShuttleScheduleByDriver(driverId)
        +HasUpcomingShuttle(driverId)
        +GetUpcomingShuttleSchedule(driverId)
        +UpdateCurrentAddress(shuttleScheduleId, driverId, lat, lng)
    }

    %% Services
    class DriverSchoolTripService {
        +GetShuttleScheduleByDate(driverId, date)
        +StartJourney(shuttleScheduleId)
        +EndJourney(shuttleScheduleId)
        +CancelJourney(shuttleScheduleId, cancelReason)
        +SkipStudentByDriver(shuttleScheduleId, studentId, cancelReason)
        +SkipStudent(shuttleScheduleId, studentId, cancelReason)
        +UndoSkipStudent(shuttleScheduleId, studentId)
        +HasInProgressShuttle(driverId)
        +GetCurrentShuttleScheduleByDriver(driverId)
        +HasUpcomingShuttle(driverId)
        +GetUpcomingShuttleSchedule(driverId)
        +IsOwnerOfShuttleSchedule(shuttleScheduleId, driverId)
        +UpdateCurrentAddress(shuttleScheduleId, driveId, lat, lng)
    }

    class ShuttleScheduleManagementService {
        +UpdateShuttleSchedule(shuttleSchedule)
        +UpdateStudentOnShuttleSchedule(shuttleScheduleId, studentOnBus)
        +AddShuttleSchedule(requests)
        +GetShuttleScheduleView(schoolId, date)
        +GetShuttleScheduleByDate(schoolId, request)
        +GetShuttleSchedule(shuttleScheduleId)
        +IsOwnerOfShuttleSchedule(schoolId, shuttleScheduleId)
        +DeleteShuttleSchedule(schoolId, startDate, endDate)
    }

    %% Entities
    class Driver {
        string VehicleType
        string LicenseNumber
        int SeatingCapacity
        List~VerifiedBy~ VerifiedBy
        DateTime? LastCheckDrivingLicense
        List~DriverInformationImage~ DriverInformationImages
        List~FileMetadata~ VehicleImages
        HashSet~DriverApprovalRequest~ DriverApprovalRequests
    }
    class Student {
        Guid Id
        Guid SchoolId
        string FirstName
        string LastName
        string FullName
        DateOnly DateOfBirth
        Guid ClassId
        Gender Gender
        Guid? AvatarKey
        Guid? QrImageKey
        string PickUpLocation
        double PickUpLat
        double PickUpLng
        DateTime? LastTimeUpdatedPickupLocation
        int? LocationGroup
        List~ManagedBy~ ManagedBy
        bool NeedsPickup
        School School
        Class Class
    }
    class ShuttleSchedule {
        Guid Id
        Guid SchoolId
        string SchoolName
        ShuttleScheduleType Type
        SessionType SessionType
        DateOnly Date
        Guid DriverId
        string DriverPhoneNumber
        string DriverName
        string DriverAvatar
        string VehicleType
        Gender DriverGender
        string LicenseNumber
        bool IsAllNotesRead
        TimeSpan PickupStartTime
        TimeSpan PickupEndTime
        TimeSpan? StartJourneyTime
        TimeSpan? EndJourneyTime
        JourneyStatus JourneyStatus
        string CancelReason
        int NumberOfStudents
        int NumberOfPickedUpStudents
        int NumberOfDroppedOffStudents
        double CurrentLat
        double CurrentLng
        BestRoute BestRoute
        List~StudentOnBus~ Students
        double TotalDistanceKm
    }
    class StudentOnBus {
        Guid StudentId
        List~ParentInfo~ Parents
        string PickupAddress
        double PickupLat
        double PickupLng
        Gender Gender
        string AvatarUrl
        string ClassName
        Guid ClassId
        string FullName
        bool IsPickedUp
        DateTimeOffset? PickedUpTime
        bool IsDroppedOff
        DateTimeOffset? DroppedOffTime
        bool SkipPickup
        string IsSkipUpReason
    }
    class JourneyNote {
        Guid Id
        string Description
        Guid JourneyId
        Guid ParentId
        Guid StudentId
        DateTime RequestedDate
        JourneyNoteType Type
        bool IsReadByDriver
    }

    %% Relationships
    DriverSchoolTripController --> DriverSchoolTripHandler : Association
    DriverSchoolTripHandler --> DriverSchoolTripService : Association
    DriverSchoolTripHandler --> ShuttleScheduleManagementService : Association
    DriverSchoolTripService --> ShuttleScheduleManagementService : Association
    ShuttleSchedule o-- StudentOnBus : Composition
    ShuttleSchedule o-- JourneyNote : Aggregation
    StudentOnBus o-- ParentInfo : Composition
    ShuttleSchedule --> Driver : Association
    StudentOnBus --> Student : Association
