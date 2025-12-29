using Identity.Core.Dtos.Auth;
using Microsoft.AspNetCore.Identity.Data;
using AuthDTOs = Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;

namespace Identity.Core.ServiceContracts;

public interface IUserService
{
    Task<AuthResponse> RegisterAsync(AuthDTOs.RegisterRequest request);

    Task<AuthResponse> LoginAsync(AuthDTOs.LoginRequest request);

    Task LogoutAsync(string userId);

    Task ChangePasswordAsync(ChangePasswordRequest request);

    Task UpdateAccountAsync(string userId, UpdateUserRequest request);

    Task DeleteAccountAsync(string userId);

    Task BanAccountAsync(string targetUserId, string actorUserId);

    Task UnbanAccountAsync(string targetUserId, string actorUserId);
}
