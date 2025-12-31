using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Users;
using Identity.Core.Options;
using Identity.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Tests;

public class TokenServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly JwtTokenOptions _jwtOptions;

    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            Mock.Of<IRoleStore<ApplicationRole>>(),
            null, null, null, null);

        _mapperMock = new Mock<IMapper>();

        _jwtOptions = new JwtTokenOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "A3F04D08-35A6-4F7E-BD3F-860A1DBFA91B",
            ExpieryInMinutes = 60
        };

        _sut = new TokenService(
            Options.Create(_jwtOptions),
            _mapperMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object
        );
    }

    [Fact]
    public async Task GenerateToken_Should_Return_Valid_Jwt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        _mapperMock
            .Setup(x => x.Map<UserResponse>(user))
            .Returns(new UserResponse { Email = user.Email });

        // Act
        var result = await _sut.GenerateToken(user);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.AccessToken);

        Assert.Equal(_jwtOptions.Issuer, token.Issuer);
        Assert.Contains(_jwtOptions.Audience, token.Audiences);
    }

    [Fact]
    public async Task GenerateToken_Should_Include_Role_And_RoleClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com"
        };

        var roleName = "Admin";
        var role = new ApplicationRole { Name = roleName };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { roleName });

        _roleManagerMock
            .Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _roleManagerMock
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim>
            {
            new Claim("permission", "users.delete"),
            new Claim("permission", "users.update")
            });

        _mapperMock
            .Setup(x => x.Map<UserResponse>(user))
            .Returns(new UserResponse { Email = user.Email });

        // Act
        var result = await _sut.GenerateToken(user);

        // Assert
        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(result.AccessToken);

        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == roleName);
        Assert.Contains(token.Claims, c => c.Type == "permission" && c.Value == "users.delete");
        Assert.Contains(token.Claims, c => c.Type == "permission" && c.Value == "users.update");
    }

    [Fact]
    public async Task GenerateToken_Should_Not_Have_Duplicate_Claims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "dup@test.com"
        };

        var roleName = "Admin";
        var role = new ApplicationRole { Name = roleName };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { roleName });

        _roleManagerMock
            .Setup(x => x.FindByNameAsync(roleName))
            .ReturnsAsync(role);

        _roleManagerMock
            .Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim>
            {
            new Claim("permission", "same"),
            new Claim("permission", "same")
            });

        _mapperMock
            .Setup(x => x.Map<UserResponse>(user))
            .Returns(new UserResponse());

        // Act
        var result = await _sut.GenerateToken(user);

        // Assert
        var token = new JwtSecurityTokenHandler()
            .ReadJwtToken(result.AccessToken);

        var permissionClaims = token.Claims
            .Where(c => c.Type == "permission" && c.Value == "same")
            .ToList();

        Assert.Single(permissionClaims);
    }

    [Fact]
    public async Task GenerateToken_Should_Set_Expiration_Correctly()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "expire@test.com"
        };

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        _mapperMock
            .Setup(x => x.Map<UserResponse>(user))
            .Returns(new UserResponse());

        // Act
        var result = await _sut.GenerateToken(user);

        // Assert
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_NonEmpty_Base64_String()
    {
        // Act
        var refreshToken = _sut.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        var bytes = Convert.FromBase64String(refreshToken);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateRefreshToken_Should_Generate_Unique_Tokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GetClaimsFromToken_Should_Return_ClaimsPrincipal_For_Valid_Token()
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };

        var token = CreateJwtToken(claims, DateTime.UtcNow.AddMinutes(5));

        // Act
        var principal = _sut.GetClaimsFromToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal("test@test.com",
            principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.True(principal.IsInRole("Admin"));
    }

    [Fact]
    public void GetClaimsFromToken_Should_Allow_Expired_Token()
    {
        // Arrange
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
            };

        var expiredToken = CreateJwtToken(
            claims,
            DateTime.UtcNow.AddMinutes(-10)
        );

        // Act
        var principal = _sut.GetClaimsFromToken(expiredToken);

        // Assert
        Assert.NotNull(principal);
    }

    private string CreateJwtToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }



}