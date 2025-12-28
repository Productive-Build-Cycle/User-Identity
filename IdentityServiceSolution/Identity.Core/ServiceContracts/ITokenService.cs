using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using System.Security.Claims;

namespace Identity.Core.ServiceContracts;

public interface ITokenService
{
    Task<AuthResponse> GenerateToken(ApplicationUser user);
    ClaimsPrincipal GetClaimsFromToken(string token);
    string GenerateRefreshToken();
}
