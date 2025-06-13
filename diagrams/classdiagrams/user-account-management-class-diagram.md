classDiagram
    direction LR
    %% Layout in a 3:4.5 ratio rectangle

    %% Controllers
    class UserManagementController {
        -_handler: IUserManagementHandler
        +CreateAccount(): Task
    }

    %% Handlers
    class IUserManagementHandler {
        <<interface>>
        +CreateAccount(): Task
    }

    class UserManagementHandler {
        -_userManagement: IUserManagement
        -_mapper: IMapper
        +CreateAccount(): Task
    }

    %% Services
    class IUserManagement {
        <<interface>>
        +CreateUser(): Task~User~
        +CreateSchoolAdmin(): Task~SchoolPerson~
        +DeleteSchoolAdmin(schoolId: Guid): Task
        +ChangeSchoolAdminPassword(schoolId: Guid, pwd: string): Task
        +GetSchoolAdmin(schoolId: Guid): Task~User~
    }

    class UserManagement {
        -_context: Context
        +CreateUser(): Task~User~
        +CreateSchoolAdmin(): Task~SchoolPerson~
        +DeleteSchoolAdmin(schoolId): Task
        +ChangeSchoolAdminPassword(schoolId, pwd): Task
        +GetSchoolAdmin(schoolId): Task~User~
    }

    %% Domain Models
    class User {
        +Id: Guid
        +UserName: string
        +Password: string
        +UserType: UserType
        +Status: AccountStatus
    }

    class SchoolPerson {
        +SchoolId: Guid
    }

    %% Relationships
    %% Inheritance
    UserManagementHandler --|> IUserManagementHandler
    UserManagement --|> IUserManagement
    SchoolPerson --|> User

    %% Association
    UserManagementController --> IUserManagementHandler
    UserManagementHandler --> IUserManagement
    UserManagement ..> User
    UserManagement ..> SchoolPerson