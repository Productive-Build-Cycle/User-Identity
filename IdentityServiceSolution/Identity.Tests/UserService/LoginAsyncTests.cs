using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;
using Identity.Core.Dtos.Roles;
using Identity.Core.ServiceContracts;
using Identity.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Identity.Tests.UserService;

public class LoginAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;

    private readonly UserrService _sut;

    public LoginAsyncTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();

        _sut = new UserrService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenServiceMock.Object,
            _configurationMock.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IEmailService>(),
            Mock.Of<IRolesService>()
        );
    }

    // 1️⃣ SUCCESS
    //[Fact]
    //public async Task LoginAsync_WhenCredentialsAreCorrect_ShouldReturnToken()
    //{
    //    // Arrange
    //    var request = new LoginRequest(
    //        "test@example.com",
    //        "CorrectPassword123!",
    //        false
    //    );

    //    var user = new ApplicationUser
    //    {
    //        Email = request.Email,
    //        EmailConfirmed = true,
    //        Banned = false,
    //        LockoutEnabled = true,
    //        LockoutMultiplier = 1,
    //        LockoutEnd = null
    //    };

    //    _userManagerMock
    //        .Setup(x => x.FindByEmailAsync(request.Email))
    //        .ReturnsAsync(user);

    //    _signInManagerMock
    //        .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
    //        .ReturnsAsync(SignInResult.Success);
    //    var authResponse = new AuthResponse(
    //        "ACCESS_TOKEN",
    //        DateTime.UtcNow.AddMinutes(30),
    //        new UserResponse(
    //            Guid.NewGuid(),
    //            request.Email,
    //            "Test",
    //            "User",
    //            null,
    //            true,
    //            new List<string>()
    //        )
    //    );

    //    _tokenServiceMock
    //        .Setup(x => x.GenerateToken(user))
    //        .ReturnsAsync(authResponse);

    //    // Act
    //    var result = await _sut.LoginAsync(request);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal("ACCESS_TOKEN", result.AccessToken);

    //    _tokenServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
    //}

    // 2️⃣ WRONG EMAIL
    [Fact]
    public async Task LoginAsync_WhenEmailDoesNotExist_ShouldThrowUnauthorized()
    {
        var request = new LoginRequest(
            "notfound@test.com",
            "AnyPassword123!",
            false
        );

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<ApplicationUser>()), Times.Never);
    }

    // 3️⃣ WRONG PASSWORD (not locked yet)
    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ShouldThrowUnauthorized()
    {
        var request = new LoginRequest(
            "test@example.com",
            "WrongPassword123!",
            false
        );

        var user = new ApplicationUser
        {
            Email = request.Email,
            EmailConfirmed = true,
            Banned = false,
            LockoutEnabled = true,
            LockoutMultiplier = 1
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);

        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<ApplicationUser>()), Times.Never);
    }

    // 4️⃣ LOCKED-OUT RESULT (multiplier increases)
    [Fact]
    public async Task LoginAsync_WhenUserIsLockedOut_ShouldIncreaseLockoutMultiplierAndThrow()
    {
        var request = new LoginRequest(
            "test@example.com",
            "WrongPassword123!",
            false
        );

        var user = new ApplicationUser
        {
            Email = request.Email,
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutMultiplier = 1
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.LockedOut);

        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(act);

        Assert.True(user.LockoutMultiplier > 1);
        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    // 5️⃣ BANNED USER
    [Fact]
    public async Task LoginAsync_WhenUserIsBanned_ShouldThrowInvalidOperationException()
    {
        var request = new LoginRequest(
            "banned@test.com",
            "AnyPassword123!",
            false
        );

        var user = new ApplicationUser
        {
            Email = request.Email,
            EmailConfirmed = true,
            Banned = true
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    // 6️⃣ ACCOUNT LOCKED BY TIME
    [Fact]
    public async Task LoginAsync_WhenAccountIsLockedByTime_ShouldThrowInvalidOperationException()
    {
        var request = new LoginRequest(
            "locked@test.com",
            "AnyPassword123!",
            false
        );

        var user = new ApplicationUser
        {
            Email = request.Email,
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
