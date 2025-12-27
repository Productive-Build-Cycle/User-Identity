using Identity.Core.DTOs.Users;

namespace Identity.Core.Dtos.Auth;

// ---------- Requests ----------
public record RegisterRequest(
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName
)
{
    public RegisterRequest() : this(default, default, default, default, default, default)
    {
    }
};

public record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false
);

public record RefreshTokenRequest(
    string RefreshToken
);

// ---------- Responses ----------
public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    UserResponse User
)
{
    public AuthResponse() : this(default, default, default)
    {
    }
};

public record RefreshTokenResponse(
    string AccessToken,
    DateTime ExpiresAt
);