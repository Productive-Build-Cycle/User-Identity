namespace Identity.Core.Dtos.Users;

// ---------- Requests ----------
public record AddUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
);

public record UpdateUserRequest(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive
);

// ---------- Responses ----------
public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive,
    List<string> Roles
);
