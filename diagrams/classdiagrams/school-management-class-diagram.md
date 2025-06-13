classDiagram
    direction LR
    %% Main layers from left to right: Controller -> Handler -> Service -> Domain
    
    %% Controllers (Left)
    class SchoolManagementController {
        -ISchoolManagementHandler _schoolManagementHandler
        +CreateSchool(CreateSchoolRequest): Task<ActionResult<SchoolDetailResponse>>
        +UpdateSchool(Guid, UpdateSchoolRequest): Task<ActionResult<SchoolDetailResponse>>
        +DeleteSchool(Guid): Task<IActionResult>
        +DeleteSchools(List<Guid>): Task<IActionResult>
        +GetSchools(GetSchoolRequest): Task<Pagination<SchoolResponse>>
        +GetSchool(Guid): Task<SchoolDetailResponse>
        +ChangeSchoolAdminPassword(Guid, string): Task<ActionResult>
        +GetPreSignedUploadUrl(Guid, string, long, string): Task<PreSignedUrlResponse>
    }

    %% Handlers (Middle-Left)
    class SchoolManagementHandler {
        -ISchoolManagement _schoolManagement 
        -IFileUploadService _uploadFileService
        -IUserManagement _userManagement
        -IMapper _mapper
        -Context _context
        -IUserBanService _userBanService
        +CreateSchool(CreateSchoolRequest): Task<SchoolDetailResponse>
        +UpdateSchool(Guid, UpdateSchoolRequest, Guid, UserType): Task<SchoolDetailResponse>
        +DeleteSchool(Guid): Task
        +DeleteSchool(List<Guid>): Task
        +GetSchools(GetSchoolRequest): Task<Pagination<SchoolResponse>>
        +GetSchool(Guid): Task<SchoolDetailResponse>
        +ChangeSchoolAdminPassword(Guid, string): Task
        +IsOwner(Guid, Guid): Task
        +GetPreSignedUploadImage(Guid, Guid, string, string, long): Task<PreSignedUrlResponse>
        -GetPreSignedDownload(List<string>): Task<List<string>>
        -AttachSchoolAdminInfo(SchoolDetailResponse): Task
    }

    %% Interfaces (Middle)
    class ISchoolManagementHandler {
        <<interface>>
        +CreateSchool(CreateSchoolRequest): Task<SchoolDetailResponse>
        +UpdateSchool(Guid, UpdateSchoolRequest, Guid, UserType): Task<SchoolDetailResponse>
        +DeleteSchool(Guid): Task
        +DeleteSchool(List<Guid>): Task
        +GetSchools(GetSchoolRequest): Task<Pagination<SchoolResponse>>
        +GetSchool(Guid): Task<SchoolDetailResponse>
        +ChangeSchoolAdminPassword(Guid, string): Task
        +IsOwner(Guid, Guid): Task
        +GetPreSignedUploadImage(Guid, Guid, string, string, long): Task<PreSignedUrlResponse>
    }

    class ISchoolManagement {
        <<interface>>
        +CreateSchool(CreateSchoolDto): Task<School>
        +UpdateSchool(UpdateSchoolDto): Task<School>
        +DeleteSchool(Guid): Task
        +DeleteSchool(List<Guid>): Task
        +GetSchool(Guid): Task<School>
        +GetSchoolsByFilter(SchoolType?, string?): Task<IEnumerable<School>>
        +GetSchoolsQueryAble(SchoolType?, string?): Task<IQueryable<School>>
    }

    %% Services (Middle-Right)
    class SchoolManagement {
        -Context _dbContext
        -IFileUploadService _fileUploadService
        -GoogleMapsService _googleMapsService
        +CreateSchool(CreateSchoolDto): Task<School>
        +UpdateSchool(UpdateSchoolDto): Task<School>
        +DeleteSchool(Guid): Task
        +DeleteSchool(List<Guid>): Task
        +GetSchool(Guid): Task<School>
        +GetSchoolsByFilter(SchoolType?, string?): Task<IEnumerable<School>>
        +GetSchoolsQueryAble(SchoolType?, string?): Task<IQueryable<School>>
        -GetById(Guid): Task<School>
        -ValidateSchoolTime(TimeSpan, TimeSpan, TimeSpan, TimeSpan): void
    }

    %% Domain Entities (Right)
    class Context {
        +DbSet<School> Schools
        +DbSet<Class> Classes
        +DbSet<SchoolPerson> SchoolPersons
        +SaveChangesAsync(): Task
        +Entry<T>(T): EntityEntry<T>
    }

    class School {
        +Guid Id
        +SchoolType SchoolType
        +string SchoolName
        +string? SchoolDescription
        +string Address
        +double AddressLat
        +double AddressLng
        +TimeSpan MorningStartTime
        +TimeSpan MorningEndTime
        +TimeSpan AfternoonEndTime
        +TimeSpan AfternoonStartTime
        +string? Email
        +string PhoneNumber
        +List<FileMetadata> Images
        +HashSet<ClassSchedule> ClassSchedules
        +HashSet<SchoolPerson> SchoolPersons
    }

    %% Relationships ordered by layers
    %% Inheritance (--|>) Left to Right
    SchoolManagementHandler --|> ISchoolManagementHandler : implements
    SchoolManagement --|> ISchoolManagement : implements

    %% Composition (--*) Right to Left
    Context --* School : owns <
    School --* ClassSchedule : owns <
    School --* SchoolPerson : owns <

    %% Association (-->) Left to Right
    SchoolManagementController --> ISchoolManagementHandler : uses >
    SchoolManagementHandler --> ISchoolManagement : uses >
    SchoolManagement --> Context : uses >
    SchoolManagementHandler --> Context : uses >