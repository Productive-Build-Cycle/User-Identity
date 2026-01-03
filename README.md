## 🏗 Architecture

The project follows a **Clean Architecture** pattern separating concerns between the API (Presentation), Core (Business Logic), and Data access.

# Identity Service Solution

A Clean Architecture Identity Provider built with ASP.NET Core. This solution handles secure user authentication (JWT), role management, dynamic permissions, and account security features like locking and banning.

⚠️ **This project is for practice & learning only and has no personal or commercial benefit for anyone.**

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
* 
## Result Pattern Showcase "Why this Architecture?"
❌ Traditional Approach (Try/Catch hell):

```C#

try {
   _userManager.CreateAsync(user);
}
catch (DuplicateEmailException ex) {
   return BadRequest(ex.Message);
}
```

✅ Our Approach (FluentResults): Clean, readable, and type-safe control flow.

```C#

var result = await _roleService.AddRoleAsync(request);
if (result.IsFailed)
    return FromResult(result); // Automatically maps errors to HTTP 400/404/409
return Ok(result.Value);
```
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

Navigate to `Identity.API/appsettings.json` and add or update the **MailCredits** section as shown below:

* **MailAddress**: The sender email address (e.g. Gmail)
* **MailTitle**: The display name used in outgoing emails
* **StmpServer**: SMTP server address (for Gmail: `smtp.gmail.com`)
* **StmpPort**: SMTP port (for TLS: `587`)
* **StmpPassword**: The SMTP password or app-specific password

```json
{
  "MailCredits": {
    "MailAddress": "YOUR_EMAIL@gmail.com",
    "MailTitle": "PBC - Identity",
    "StmpServer": "smtp.gmail.com",
    "StmpPort": 587,
    "StmpPassword": "REPLACE_WITH_YOUR_SMTP_PASSWORD"
  }
}
```

#### Notes

* For **Gmail**, you must use an **App Password**, not your regular account password.
* Never commit real SMTP credentials to source control.
* In production, ensure secrets are stored securely (e.g. environment variables or a secret manager).

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
## 📮 How to Use the Postman Collection

This repository includes a pre-configured Postman collection to help you test the API endpoints immediately.

### 1. Import the Collection
1.  Open **Postman**.
2.  Click the **Import** button (top left).
3.  Drag and drop the file `IdentityService.postman_collection.json`, or select it from the file dialog.

### 2. Environment Configuration
The collection comes with **Collection Variables** pre-configured, so you don't strictly need a separate Environment.
* **baseUrl**: Default is set to `https://localhost:7183`. If your port differs, click on the collection name > **Variables** tab and update `baseUrl`.

### 3. Automatic Token Handling 🪄
You **do not** need to manually copy-paste JWT tokens!
* The **Login** request includes a **Test Script** that automatically captures the `accessToken` from the response.
* It saves this token to the `accessToken` collection variable.
* All other requests (like "Get All Roles", "Update Account") are configured to inherit this token automatically from the variable.

### 4. Running the Flow
1.  **Register**: Run the `Auth > Register` request first to create a user (or use the default admin if seeded).
2.  **Login**: Run the `Auth > Login` request. Check the **Test Results** tab to see "Access Token stored successfully".
3.  **Authenticated Requests**: Now you can run any protected endpoint (e.g., `Roles > Get All Roles`), and it will work instantly.

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


