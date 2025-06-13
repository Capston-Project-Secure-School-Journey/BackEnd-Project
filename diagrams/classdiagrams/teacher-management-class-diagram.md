classDiagram
    direction LR
    %% Main layers from left to right: Controller -> Handler -> Service -> Domain
    
    %% Controllers (Left)
    class TeacherManagementController {
        -ITeacherManagementHandler _teacherManagementHandler
        +CreateTeacher(CreateTeacherRequest): Task<ActionResult<TeacherDetailResponse>>
        +UpdateTeacher(Guid, UpdateTeacherRequest): Task<ActionResult<TeacherDetailResponse>>
        +GetTeacher(Guid): Task<ActionResult<TeacherDetailResponse>>
        +GetTeachers(GetTeacherRequest): Task<ActionResult<Pagination<TeacherResponse>>>
        +DeleteTeacher(Guid): Task<IActionResult>
        +DeleteTeachers(List<Guid>): Task<IActionResult>
        +UploadAvatar(Guid, IFormFile): Task<IActionResult>
        +GetTemplateExcelFile(): Task<IActionResult>
        +ImportData(IFormFile): Task<ActionResult>
    }

    %% Handlers (Middle-Left)
    class TeacherManagementHandler {
        -ITeacherManagementService _teacherManagementService
        -IMapper _mapper
        -IFileUploadService _uploadFileService
        -Context _context
        -IServiceProvider _serviceProvider
        +GetTeachers(Guid, GetTeacherRequest): Task<Pagination<TeacherResponse>>
        +GetTeacherById(Guid, Guid): Task<TeacherDetailResponse>
        +AddTeacher(Guid, CreateTeacherRequest): Task<TeacherDetailResponse>
        +UpdateTeacher(Guid, UpdateTeacherRequest): Task<TeacherDetailResponse>
        +DeleteTeacher(Guid, Guid): Task
        +DeleteTeacher(Guid, List<Guid>): Task
        +UploadAvatar(Guid, Guid, IFormFile): Task<string>
        +GetTemplateExcelFile(): Task<MemoryStream>
        +ImportTeachersFromExcelFile(Guid, IFormFile): Task
        -MapToTeacherResponse(Teacher, IMapper, IFileUploadService): Task<TeacherDetailResponse>
        -GetExcelColumnDefinitions(): List<ExcelColumnDefinition<Teacher>>
    }

    %% Interfaces (Middle)
    class ITeacherManagementHandler {
        <<interface>>
        +GetTeachers(Guid, GetTeacherRequest): Task<Pagination<TeacherResponse>>
        +GetTeacherById(Guid, Guid): Task<TeacherDetailResponse>
        +AddTeacher(Guid, CreateTeacherRequest): Task<TeacherDetailResponse>
        +UpdateTeacher(Guid, UpdateTeacherRequest): Task<TeacherDetailResponse>
        +DeleteTeacher(Guid, Guid): Task
        +DeleteTeacher(Guid, List<Guid>): Task
        +UploadAvatar(Guid, Guid, IFormFile): Task<string>
        +GetTemplateExcelFile(): Task<MemoryStream>
        +ImportTeachersFromExcelFile(Guid, IFormFile): Task
    }

    class ITeacherManagementService {
        <<interface>>
        +GetTeachers(Guid): Task<IEnumerable<Teacher>>
        +GetTeachersByFilter(Guid, string?, string?, string?): Task<IEnumerable<Teacher>>
        +GetTeachersByFilterQueryAble(Guid, string?, string?, string?): Task<IQueryable<Teacher>>
        +GetTeacherById(Guid): Task<Teacher>
        +AddTeacher(CreateTeacherDto): Task<Teacher>
        +ImportTeachersFromExcel(Guid, List<Teacher>): Task
        +UpdateTeacher(UpdateTeacherDto): Task<Teacher>
        +DeleteTeacher(Guid): Task
        +DeleteTeacher(List<Guid>): Task
        +CheckExistTeacher(Guid, Guid): Task
        +IsOwnerOfTeacher(Guid, Guid): Task
        +UploadAvatar(Guid, IFormFile): Task<string>
    }

    %% Services (Middle-Right)
    class TeacherManagementService {
        -Context _context
        -IFileUploadService _uploadService
        -ILogger<TeacherManagementService> _logger
        +GetTeachers(Guid): Task<IEnumerable<Teacher>>
        +GetTeachersByFilter(Guid, string?, string?, string?): Task<IEnumerable<Teacher>>
        +GetTeachersByFilterQueryAble(Guid, string?, string?, string?): Task<IQueryable<Teacher>>
        +GetTeacherById(Guid): Task<Teacher>
        +AddTeacher(CreateTeacherDto): Task<Teacher>
        +ImportTeachersFromExcel(Guid, List<Teacher>): Task
        +UpdateTeacher(UpdateTeacherDto): Task<Teacher>
        +DeleteTeacher(Guid): Task
        +DeleteTeacher(List<Guid>): Task
        +CheckExistTeacher(Guid, Guid): Task
        +IsOwnerOfTeacher(Guid, Guid): Task
        +UploadAvatar(Guid, IFormFile): Task<string>
    }

    %% Domain Entities (Right)
    class Context {
        +DbSet<Teacher> Teachers
        +DbSet<Class> Classes
        +SaveChangesAsync(): Task
        +Entry<T>(T): EntityEntry<T>
    }

    class Teacher {
        +Guid Id
        +Guid SchoolId
        +string FirstName
        +string LastName
        +string FullName
        +DateOnly DateOfBirth
        +Gender Gender
        +string PhoneNumber
        +string Email
        +Guid? AvatarKey
        +School School
        +HashSet<Class> ManagedClasses
    }

    class Class {
        +Guid Id
        +Guid SchoolId
        +string ClassName
        +Grade Grade
        +int NumberOfStudent
        +List<ManagedTeacher> ManagedTeachers
        +School School
        +HashSet<Student> Students
    }

    class ManagedTeacher {
        +Guid ManagedTeacherId
    }

    %% Relationships ordered by layers
    %% Inheritance (--|>) Left to Right  
    TeacherManagementHandler --|> ITeacherManagementHandler : implements
    TeacherManagementService --|> ITeacherManagementService : implements

    %% Composition (--*) Right to Left
    Context --* Teacher : owns <
    Context --* Class : owns <
    Class --* ManagedTeacher : owns <

    %% Aggregation (--o) Bidirectional
    Teacher --o Class : manages >
    Class --o Teacher : managed by >

    %% Association (-->) Left to Right
    TeacherManagementController --> ITeacherManagementHandler : uses >
    TeacherManagementHandler --> ITeacherManagementService : uses >
    TeacherManagementService --> Context : uses >
    TeacherManagementHandler --> Context : uses >