classDiagram
    direction TB

    %% Controllers
    class ParentSchoolTripController {
        +GetShuttleSchedulesByStudent(studentId, date)
        +HasInProgressShuttle()
        +GetCurrentShuttleSchedule()
        +HasUpcomingShuttle()
        +GetUpcomingShuttleSchedule()
    }

    class JourneyNoteController {
        +AddJourneyNote(createJourneyNoteDto)
        +UpdateJourneyNote(updateJourneyNoteDto)
        +DeleteJourneyNote(journeyNoteId)
        +GetAllJourneyNotes(shuttleId)
        +GetAllJourneyNotesByParent(shuttleId, parentId)
        +ReadJourneyNote(journeyNoteId)
        +ReadAllJourneyNote(journeyId)
    }

    %% Handlers
    class ParentSchoolTripHandler {
        -IParentSchoolTripService _parentSchoolTripService
        +GetShuttleSchedulesByStudent(parentId, studentId, date)
        +HasInProgressShuttle(parentId)
        +GetCurrentShuttleSchedule(parentId)
        +HasUpcomingShuttle(parentId)
        +GetUpcomingShuttleSchedule(parentId)
    }

    %% Services
    class ParentSchoolTripService {
        -Context _context
        -IMapper _mapper
        -IUserService _userService
        +GetShuttleSchedulesByStudent(studentId, date)
        +HasInProgressShuttle(parentId)
        +GetCurrentShuttleSchedule(parentId)
        +HasUpcomingShuttle(parentId)
        +GetUpcomingShuttleSchedule(parentId)
        +IsManageByStudent(parentId, studentId)
        -ConvertShuttleScheduleResponse2ParentResponse(shuttleSchedule, studentId)
    }

    class JourneyNoteService {
        -Context _context
        -IShuttleScheduleManagementService _shuttleScheduleManagementService
        -IParentSchoolTripService _parentSchoolTripService
        -IDriverSchoolTripService _driverSchoolTripService
        +AddJourneyNote(createJourneyNoteDto)
        +UpdateJourneyNote(updateJourneyNoteDto)
        +GetAllJourneyNotes(shuttleId)
        +GetAllJourneyNotesByParent(shuttleId, parentId)
        +ReadJourneyNote(journeyNoteId)
        +DeleteJourneyNote(journeyNoteId)
        -UpdateStudentTripInfoWhenChangeNote(journey, note)
        -UpdateStudentTripInfoWhenDeleteNote(journey, note)
    }

    %% Domain Models
    class ShuttleSchedule {
        +Guid Id
        +String SchoolName
        +ShuttleScheduleType Type
        +SessionType SessionType
        +DateOnly Date
        +String DriverName
        +String DriverPhoneNumber
        +JourneyStatus JourneyStatus
        +TimeSpan PickupStartTime
        +TimeSpan PickupEndTime
        +TimeSpan? StartJourneyTime
        +TimeSpan? EndJourneyTime
        +int NumberOfStudents
        +List~StudentOnBus~ Students
        +double CurrentLat
        +double CurrentLng
        +bool IsAllNotesRead
    }

    class StudentOnBus {
        +Guid StudentId
        +List~ParentInfo~ Parents
        +String PickupAddress
        +double PickupLat
        +double PickupLng
        +String ClassName
        +String FullName
        +bool IsPickedUp
        +DateTimeOffset? PickedUpTime
        +bool IsDroppedOff
        +DateTimeOffset? DroppedOffTime
        +bool SkipPickup
        +String IsSkipUpReason
    }

    class ParentInfo {
        +Guid ParentId
        +String FullName
        +String PhoneNumber
        +Relationship Relationship
    }

    class JourneyNote {
        +Guid Id
        +String Description
        +Guid JourneyId
        +Guid ParentId
        +Guid StudentId
        +DateTime RequestedDate
        +JourneyNoteType Type
        +bool IsReadByDriver
    }

    %% Relationships
    ParentSchoolTripController --> ParentSchoolTripHandler : uses
    ParentSchoolTripHandler --> ParentSchoolTripService : uses
    ParentSchoolTripService --> ShuttleSchedule : manages
    JourneyNoteService --> ShuttleSchedule : manages
    JourneyNoteService --> JourneyNote : manages
    ShuttleSchedule "1" *-- "*" StudentOnBus : contains
    StudentOnBus "1" *-- "*" ParentInfo : contains