classDiagram
    direction LR
    %% Main layers from left to right: Controller -> Handler -> Service -> Domain
    
    %% Controllers (Left)
    class ClassManagementController {
        -IClassManagementHandler _classManagementHandler
        +CreateClass(CreateClassRequest): Task<ActionResult<ClassDetailResponse>>
        +UpdateClass(Guid, UpdateClassRequest): Task<ActionResult<ClassDetailResponse>>
        +GetClass(Guid): Task<ActionResult<ClassDetailResponse>>
        +GetClasses(GetClassesRequest): Task<ActionResult<Pagination<ClassResponse>>>
        +DeleteClass(Guid): Task<IActionResult>
        +DeleteClasses(List<Guid>): Task<IActionResult>
        +GetTemplateExcelFile(): Task<IActionResult>
        +ImportData(IFormFile): Task<ActionResult>
    }

    %% Handlers (Middle-Left)
    class ClassManagementHandler {
        -IClassManagementService _classManagementService
        -ITeacherManagementService _teacherManagementService 
        -IMapper _mapper
        -IServiceProvider _serviceProvider
        -Context _context
        +GetClasses(Guid, GetClassesRequest): Task<Pagination<ClassResponse>>
        +GetClassById(Guid, Guid): Task<ClassDetailResponse>
        +AddClass(Guid, CreateClassRequest): Task<ClassDetailResponse>
        +UpdateClass(Guid, UpdateClassRequest): Task<ClassDetailResponse>
        +DeleteClass(Guid, Guid): Task
        +DeleteClass(Guid, List<Guid>): Task
        +GetTemplateExcelFile(): Task<MemoryStream>
        +ImportClassesFromExcelFile(Guid, IFormFile): Task
        -SetManagedTeachers(List<ClassDetailResponse>): Task
        -SetManagedTeachers(ClassDetailResponse): Task
        -GetExcelColumnDefinitions(): List<ExcelColumnDefinition<Class>>
    }

    %% Interfaces (Middle)
    class IClassManagementHandler {
        <<interface>>
        +GetClasses(Guid, GetClassesRequest): Task<Pagination<ClassResponse>>
        +GetClassById(Guid, Guid): Task<ClassDetailResponse>
        +AddClass(Guid, CreateClassRequest): Task<ClassDetailResponse>
        +UpdateClass(Guid, UpdateClassRequest): Task<ClassDetailResponse>
        +DeleteClass(Guid, Guid): Task
        +DeleteClass(Guid, List<Guid>): Task
        +GetTemplateExcelFile(): Task<MemoryStream>
        +ImportClassesFromExcelFile(Guid, IFormFile): Task
    }

    class IClassManagementService {
        <<interface>>
        +GetClasses(Guid): Task<IEnumerable<Class>>
        +GetClassesByFilter(Guid, string?, Grade?): Task<IEnumerable<Class>>
        +GetClassesQueryAbleByFilter(Guid, string?, Grade?): Task<IQueryable<Class>>
        +GetClassById(Guid): Task<Class>
        +AddClass(CreateClassDto): Task<Class>
        +UpdateClass(UpdateClassDto): Task<Class>
        +DeleteClass(Guid): Task
        +DeleteClass(List<Guid>): Task
        +IsOwnerOfClass(Guid, Guid): Task
        +ImportClassesFromExcel(Guid, List<Class>): Task
    }

    %% Services (Middle-Right)
    class ClassManagementService {
        -Context _context
        -ISchoolManagement _schoolManagement
        -ITeacherManagementService _teacherManagement
        +GetClasses(Guid): Task<IEnumerable<Class>>
        +GetClassesByFilter(Guid, string?, Grade?): Task<IEnumerable<Class>>
        +GetClassesQueryAbleByFilter(Guid, string?, Grade?): Task<IQueryable<Class>>
        +GetClassById(Guid): Task<Class>
        +AddClass(CreateClassDto): Task<Class>
        +UpdateClass(UpdateClassDto): Task<Class>
        +DeleteClass(Guid): Task
        +DeleteClass(List<Guid>): Task
        +IsOwnerOfClass(Guid, Guid): Task
        +ImportClassesFromExcel(Guid, List<Class>): Task
        -CheckExistClassName(Guid, string): Task
        -ValidateGrade(SchoolType, Grade): void
    }

    %% Domain Entities (Right)
    class Context {
        +DbSet<Class> Classes
        +DbSet<Student> Students
        +SaveChangesAsync(): Task
        +Entry<T>(T): EntityEntry<T>
    }

    class Class {
        +Guid Id
        +Guid SchoolId 
        +string ClassName
        +Grade Grade
        +int NumberOfStudent
        +List<Student> Students
        +List<ManagedTeacher> ManagedTeachers
        +School School
    }

    class Student {
        +Guid Id
        +Guid SchoolId
        +Guid ClassId
        +string FirstName
        +string LastName
        +string FullName 
        +Class Class
    }

    %% Relationships ordered by layers
    %% Inheritance (--|>) Left to Right
    ClassManagementHandler --|> IClassManagementHandler : implements
    ClassManagementService --|> IClassManagementService : implements

    %% Composition (--*) Right to Left
    Context --* Class : owns <
    Context --* Student : owns <
    Class --* ManagedTeacher : owns < 

    %% Aggregation (--o) Bidirectional
    Class --o Student : contains >
    Student --o Class : belongs to >
    
    %% Association (-->) Left to Right
    ClassManagementController --> IClassManagementHandler : uses >
    ClassManagementHandler --> IClassManagementService : uses >
    ClassManagementService --> Context : uses >
    ClassManagementHandler --> Context : uses >