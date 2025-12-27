namespace Identity.Core.DTOs.Users;

// ---------- Requests ----------
public record AddUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
)
{
    public AddUserRequest() : this(default, default, default, default, default)
    {
    }
};

public record UpdateUserRequest(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive
)
{
    public UpdateUserRequest() : this(default, default, default, default, default, default)
    {
    }
};

// ---------- Responses ----------
public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool IsActive,
    List<string> Roles
)
{
    public UserResponse() : this(default, default, default, default, default, default, default)
    {
    }
};
