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
);

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
);

public record RefreshTokenResponse(
    string AccessToken,
    DateTime ExpiresAt
);
