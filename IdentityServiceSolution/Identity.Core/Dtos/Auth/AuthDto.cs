using Identity.Core.Dtos.Users;

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

public record ChangePasswordRequest(
    string UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
)
{
    public ChangePasswordRequest() : this(default, default, default, default)
    {
    }
};

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