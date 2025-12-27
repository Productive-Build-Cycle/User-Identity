using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.DTOs.Users;
using Identity.Core.Options;
using Identity.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

}