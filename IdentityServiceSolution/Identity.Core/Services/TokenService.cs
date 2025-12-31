using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;
using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Core.Services;

public class TokenService : ITokenService
{
    private readonly JwtTokenOptions _tokenOptions;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public TokenService(
        IOptions<JwtTokenOptions> tokenOptions,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _tokenOptions = tokenOptions.Value;
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var range = RandomNumberGenerator.Create();
        range.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task<AuthResponse> GenerateToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Email)
        };

        foreach(var roel in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roel));

            var roleEntity = await _roleManager.FindByNameAsync(roel);
            if (roleEntity != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                claims.AddRange(roleClaims);
            }

        }

        claims = claims
            .GroupBy(c => new { c.Type, c.Value })
            .Select(g => g.First())
            .ToList();


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.Key));
        var signInCreds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_tokenOptions.ExpieryInMinutes),
            signingCredentials: signInCreds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var mappedUser = _mapper.Map<UserResponse>(user) with { Roles = roles.ToList() };

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = jwtToken.ValidTo,
            User = mappedUser
        };

    }

    public ClaimsPrincipal GetClaimsFromToken(string token)
    {
        var tokenValidator = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // نکته : توکنی که ما الان داریم میگیریم منقضی شده پس نیازی به این نیست
            ValidIssuer = _tokenOptions.Issuer,
            ValidAudience = _tokenOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.Key))
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, tokenValidator, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
