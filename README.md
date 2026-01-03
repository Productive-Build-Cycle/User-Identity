## 🏗 Architecture

The project follows a **Clean Architecture** pattern separating concerns between the API (Presentation), Core (Business Logic), and Data access.

# Identity Service Solution

A Clean Architecture Identity Provider built with ASP.NET Core. This solution handles secure user authentication (JWT), role management, dynamic permissions, and account security features like locking and banning.
⚠️ This project is for practice & learning only and has no personal or commercial benefit for anyone.
## 🚀 Features

* **Core Identity**: Built on **Microsoft ASP.NET Core Identity** for robust, secure user and role management.
* **Authentication**: JWT-based login with Refresh Tokens.
* **Role Management**: Create, Edit, Delete roles and assign them to users.
* **Dynamic Permissions**: Granular permission claims (e.g., `user.create`, `role.assign`) enforced via Policy-based authorization.
* **Error Handling**: Implements the **Result Pattern** using **FluentResults** to replace exceptions with explicit success/failure returns.
* **Logging**: Structured logging configured with **Serilog**, supporting Console and Seq sinks.
* **Security**:
    * **Account Locking**: Exponential lockout time based on `LockoutMultiplier` after failed attempts.
    * **Banning**: Admin capability to ban/unban users indefinitely.
    * **Email Confirmation**: HTML email templates for account verification.
* **Validation**: FluentValidation for all incoming requests.

## 🛠 Prerequisites

Before running the application, ensure you have the following installed:

1.  **.NET SDK**: Version **10.0** (Recommended).
2.  **SQL Server**: LocalDB, Express, or Docker container.
3.  **IDE**: Visual Studio 2026 or VS Code.

## ⚙️ Setup & Configuration

### 1. Fix Target Framework
Open the `.csproj` files for both **Identity.API** and **Identity.Core** and update the framework version to a latest release:

```xml
<TargetFramework>net10.0</TargetFramework>
```
### 2. Configure App Settings
Navigate to `Identity.API/appsettings.json` and update the following settings:
* **ConnectionStrings**: Point `Default` to your local SQL Server instance.
* **JwtTokenOptions**: Provide a secure, long string (at least 32 chars) for the `Key`.

```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SERVER;Database=PDSIdentity;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtTokenOptions": {
    "Issuer": "https://localhost:7183",
    "Audience": "https://localhost:7183",
    "Key": "REPLACE_THIS_WITH_A_VERY_LONG_SECURE_RANDOM_STRING_AT_LEAST_32_CHARS",
    "ExpieryInMinutes": 60,
    "RefreshTokenExpieryInDays": 5
  },
  "BaseUrl": "https://localhost:7183"
}
```
### 3. Configure Email Service
**Important:** The current email service uses hardcoded credentials.
* **For Production:** Refactor `EmailService.cs` to read credentials from `appsettings.json`.
* **For Development:** You can temporarily update the hardcoded credentials in `Identity.Core/Services/EmailService.cs` with your own test SMTP settings.

### 4. Database Initialization
Run the Entity Framework migrations to create the database and tables:

```bash
# Open terminal in the solution root folder
dotnet ef database update --project Identity.Core --startup-project Identity.API
```
The application includes a `RoleInitializer` that will automatically seed default roles (Admin, Mentor, User) and permissions when you first run the app.

## 🏃‍♂️ How to Run

### Option A: Using Visual Studio
1. Open `IdentityServiceSolution.sln` or `IdentityServiceSolution.slnx`.
2. Set **Identity.API** as the Startup Project.
3. Press **F5** or click **Run**.

### Option B: Using CLI
1. Open a terminal in the solution root.
2. Run the API project:

```bash
dotnet run --project Identity.API
```
The application will start, typically on `https://localhost:7183`.

## 📖 API Documentation
Once the application is running, open your browser to the Swagger UI to interact with the endpoints:

```plaintext
https://localhost:7183/swagger
```
## 🧪 Running Tests
The solution includes unit tests covering Services and Logic using xUnit and Moq.

```bash
dotnet test Identity.Tests/Identity.Tests.csproj
```
## 🔑 Default Roles & Permissions

| Role | Key Permissions |
| :--- | :--- |
| **Admin** | `user.*`, `role.*` (Full Access) |
| **Mentor** | `user.ban`, `user.unban`, `user.update` |
| **User** | `user.delete`, `user.update` (Self management) |
