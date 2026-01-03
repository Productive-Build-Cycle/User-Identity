# Identity Service Solution

A Clean Architecture Identity Provider built with ASP.NET Core. This solution handles secure user authentication (JWT), role management, dynamic permissions, and account security features like locking and banning.

## 🏗 Architecture

The project follows a **Clean Architecture** pattern separating concerns between the API (Presentation), Core (Business Logic), and Data access.

```mermaid
classDiagram
    %% Domain Entities
    class ApplicationUser {
        +string FirstName
        +string LastName
        +string RefreshToken
        +bool Banned
        +int LockoutMultiplier
    }
    class ApplicationRole {
        +string Description
    }

    %% Controllers
    class AuthController {
        +Register()
        +Login()
        +ConfirmEmail()
        +ChangePassword()
    }
    class RoleController {
        +AddRole()
        +EditRole()
        +AssignRoleToUser()
        +AddClaimToRole()
    }

    %% Service Interfaces
    class IUserService {
        <<interface>>
        +RegisterAsync()
        +LoginAsync()
        +BanAccountAsync()
    }
    class IRolesService {
        <<interface>>
        +AddRoleAsync()
        +AddUserToRoleAsync()
        +GetRolesHavingClaimAsync()
    }
    class ITokenService {
        <<interface>>
        +GenerateToken()
        +GenerateRefreshToken()
    }

    %% Relationships
    AuthController --> IUserService : Uses
    RoleController --> IRolesService : Uses
    IUserService --> ITokenService : Uses
    IUserService --> ApplicationUser : Manages
    IRolesService --> ApplicationRole : Manages
