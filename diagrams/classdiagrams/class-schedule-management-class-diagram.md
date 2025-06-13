classDiagram
    direction LR
    %% Main layers from left to right: Controller -> Handler -> Service -> Domain

    %% Controllers
    class ScheduleManagementController {
        -IScheduleManagementHandler _scheduleManagementHandler
        +GetCurrentScheduleView(DateOnly): Task<ClassSchedulePaginationResponse>
        +CreateSchedule(CreateScheduleRequest): Task<IEnumerable<ClassScheduleResponse>>
        +UpdateSchedule(Guid, UpdateScheduleRequest): Task<ClassScheduleResponse>
        +GetScheduleByDate(GetScheduleByDateRequest): Task<Pagination<ClassScheduleResponse>>
        +DeleteSchedule(List<Guid>): Task<IActionResult>
        +DeleteSchedule(Guid): Task<IActionResult>
        +CloneWeekSchedule(CloneWeekScheduleRequest): Task<IActionResult>
        +CloneDaySchedule(CloneDayScheduleRequest): Task<IActionResult>
    }

    %% Handlers 
    class ScheduleManagementHandler {
        -IScheduleManagementService _scheduleManagementService
        -IMapper _mapper
        -Context _context
        +CreateSchedule(Guid, CreateScheduleRequest): Task<IEnumerable<ClassScheduleResponse>>
        +UpdateSchedule(Guid, UpdateScheduleRequest): Task<ClassScheduleResponse>
        +GetScheduleView(Guid, DateOnly): Task<ClassSchedulePaginationResponse>
        +GetScheduleByDate(Guid, GetScheduleByDateRequest): Task<Pagination<ClassScheduleResponse>>
        +DeleteSchedule(Guid, Guid): Task
        +DeleteSchedule(Guid, List<Guid>): Task
        +CloneWeekSchedule(Guid, DateOnly, DateOnly): Task
        +CloneDaySchedule(Guid, DateOnly, DateOnly): Task
        -MapToClassScheduleResponse(ClassSchedule): ClassScheduleResponse
    }

    %% Interfaces
    class IScheduleManagementHandler {
        <<interface>>
        +CreateSchedule(Guid, CreateScheduleRequest): Task<IEnumerable<ClassScheduleResponse>>
        +UpdateSchedule(Guid, UpdateScheduleRequest): Task<ClassScheduleResponse>
        +GetScheduleView(Guid, DateOnly): Task<ClassSchedulePaginationResponse>
        +GetScheduleByDate(Guid, GetScheduleByDateRequest): Task<Pagination<ClassScheduleResponse>>
        +DeleteSchedule(Guid, Guid): Task
        +DeleteSchedule(Guid, List<Guid>): Task
        +CloneWeekSchedule(Guid, DateOnly, DateOnly): Task
        +CloneDaySchedule(Guid, DateOnly, DateOnly): Task
    }

    class IScheduleManagementService {
        <<interface>>
        +CreateSchedule(Guid, CreateScheduleDto): Task<IEnumerable<ClassSchedule>>
        +UpdateSchedule(Guid, UpdateScheduleDto): Task<ClassSchedule>
        +DeleteSchedule(Guid, Guid): Task
        +DeleteSchedule(Guid, List<Guid>): Task
        +GetScheduleByWeek(Guid, DateTime): Task<IEnumerable<ClassSchedule>>
        +GetScheduleByDateQueryable(Guid, DateOnly, SessionType?, Guid?, string?, Grade?): Task<IQueryable<ClassSchedule>>
        +GetScheduleView(Guid, DateOnly): Task<ClassSchedulePaginationResponse>
        +CloneMonthSchedule(Guid, DateOnly, DateOnly): Task
        +CloneWeekSchedule(Guid, DateOnly, DateOnly): Task
        +CloneDaySchedule(Guid, DateOnly, DateOnly): Task
    }

    %% Services
    class ScheduleManagementService {
        -Context _context
        -IClassManagementService _classManagementService
        +CreateSchedule(Guid, CreateScheduleDto): Task<IEnumerable<ClassSchedule>>
        +UpdateSchedule(Guid, UpdateScheduleDto): Task<ClassSchedule>
        +DeleteSchedule(Guid, Guid): Task
        +DeleteSchedule(Guid, List<Guid>): Task
        +GetScheduleByWeek(Guid, DateTime): Task<IEnumerable<ClassSchedule>>
        +GetScheduleByDateQueryable(Guid, DateOnly, SessionType?, Guid?, string?, Grade?): Task<IQueryable<ClassSchedule>>
        +GetScheduleView(Guid, DateOnly): Task<ClassSchedulePaginationResponse>
        +CloneMonthSchedule(Guid, DateOnly, DateOnly): Task
        +CloneWeekSchedule(Guid, DateOnly, DateOnly): Task
        +CloneDaySchedule(Guid, DateOnly, DateOnly): Task
        -DetachScheduleFromGroup(ClassSchedule): Task
        -FindScheduleGroupMatchWithSchedule(Guid, DateOnly, SessionType, Grade): ScheduleGroup
        -GetTargetClasses(Guid, ScheduleType, Grade?, List<Guid>): Task<List<Guid>>
        -CheckOverlap(Guid, DateOnly, List<Guid>, SessionType): Task
        -GetGrades(Guid): Task<Dictionary<Grade, List<Guid>>>
        -CheckScheduleDate(DateOnly): void
        -ValidateCreateScheduleDto(CreateScheduleDto): void
    }

    %% Domain Models
    class ClassSchedule {
        +Guid Id
        +Guid SchoolId 
        +DateOnly Date
        +string Note
        +SessionType SessionType
        +ScheduleType ScheduleType
        +Guid ClassId
        +Grade? Grade
        +Guid? ScheduleGroupId
        +ScheduleGroup? ScheduleGroup
        +School School
        +Class Class
        +object Clone()
    }

    class ScheduleGroup {
        +Guid Id
        +Guid SchoolId
        +DateOnly Date 
        +SessionType SessionType
        +ScheduleType ScheduleType
        +Grade? Grade
        +List<Guid> ClassException
        +School School
        +HashSet<ClassSchedule> ClassSchedules
        +object Clone()
    }

    class Context {
        +DbSet<ClassSchedule> ClassSchedules
        +DbSet<ScheduleGroup> ScheduleGroups
        +SaveChangesAsync(): Task
    }

    %% Relationships
    %% Inheritance
    ScheduleManagementHandler --|> IScheduleManagementHandler : implements
    ScheduleManagementService --|> IScheduleManagementService : implements

    %% Association 
    ScheduleManagementController --> IScheduleManagementHandler : uses >
    ScheduleManagementHandler --> IScheduleManagementService : uses >
    ScheduleManagementService --> Context : uses >
    ScheduleManagementHandler --> Context : uses >

    %% Composition
    Context --* ClassSchedule : contains <
    Context --* ScheduleGroup : contains <
    ScheduleGroup --* ClassSchedule : contains <