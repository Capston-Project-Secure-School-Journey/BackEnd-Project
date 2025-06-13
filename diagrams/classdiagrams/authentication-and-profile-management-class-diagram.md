classDiagram
    direction LR

    %% Controllers
    class AuthenController {
        +Login(): Task
    }
    class UserController {
        +GetProfile(): Task
        +UpdateProfile(): Task 
        +UploadAvatar(): Task
    }

    %% Handlers
    class IUserHandler {
        <<interface>>
        +GetProfile(id: Guid): Task
        +UpdateProfile(id: Guid): Task
        +UpdateAvatar(id: Guid): Task
    }
    class UserHandler {
        -_userService: IUserService
        -_mapper: IMapper
        -_fileService: IFileUploadService
        +GetProfile(): Task
        +UpdateProfile(): Task
        +UpdateAvatar(): Task
    }

    %% Services
    class IAuthService {
        <<interface>>
        +Login(): Task
    }
    class AuthService {
        -_context: Context
        -_tokenService: ITokenService  
        -_banService: IUserBanService
        +Login(): Task
    }

    class IUserService {
        <<interface>>
        +GetUser(id: Guid): Task~User~
        +UpdateUser(id: Guid): Task~User~
        +UpdateAvatar(id: Guid): Task
        +SendVerifyEmail(id: Guid): Task
        +VerifyEmail(token: string): Task
    }
    class UserService {
        -_context: Context
        -_fileService: IFileUploadService
        -_mailService: IMailService
        -_banService: IUserBanService
        -_tokenService: ITokenService
        +GetUser(): Task~User~
        +UpdateUser(): Task~User~
        +UpdateAvatar(): Task
        +SendVerifyEmail(): Task
        +VerifyEmail(): Task
    }

    %% Domain Models
    class User {
        +Id: Guid
        +UserName: string
        +Password: string
        +UserType: UserType
        +Email: string
        +PhoneNumber: string
        +FirstName: string
        +LastName: string
        +Gender: Gender
        +DateOfBirth: DateOnly
        +Address: string
        +AvatarKey: Guid
        +Status: AccountStatus
        +VerificationMethod: VerificationMethod
    }

    class Driver {
        +VehicleType: string
        +LicenseNumber: string
        +Capacity: int
        +VerifiedBy: List~VerifiedBy~
        +VehicleImages: List~string~
        +DriverInfoImages: List~DriverInfoImage~
    }

    class Parent {
        +Students: List~RelationshipWithStudent~
    }

    class SchoolPerson {
        +SchoolId: Guid
        +School: School
    }

    %% Relationships
    %% Inheritance
    UserHandler ..|> IUserHandler
    UserService ..|> IUserService
    AuthService ..|> IAuthService
    Driver --|> User
    Parent --|> User 
    SchoolPerson --|> User

    %% Association
    AuthenController --> IAuthService
    UserController --> IUserHandler
    UserHandler --> IUserService
    UserService --> User : manages
    AuthService --> User : authenticates

    %% Composition
    Driver *-- DriverInfoImage
    Parent *-- RelationshipWithStudent