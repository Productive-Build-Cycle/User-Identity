using FluentResults;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;

namespace Identity.Core.ServiceContracts;

public interface IUserService
{
    Task<Result<RergisterResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<UserResponse> GetUserByEmail(string email);

    Task<Result> LogoutAsync(string userId);

    Task<Result> ChangePasswordAsync(ChangePasswordRequest request);

    Task<Result> UpdateAccountAsync(string userId, UpdateUserRequest request);

    Task<Result> DeleteAccountAsync(string userId);

    Task<Result> BanAccountAsync(BanAccountRequest request);

    Task<Result> UnbanAccountAsync(BanAccountRequest request);
}