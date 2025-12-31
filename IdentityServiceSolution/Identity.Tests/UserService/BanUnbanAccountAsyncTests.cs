using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Core.Domain.Entities;
using Identity.Core.Services;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Identity.Tests.UserService;

public class BanUnbanAccountAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

    private readonly UserrService _sut;

    public BanUnbanAccountAsyncTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null
        );

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>()
        );

        _sut = new UserrService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            Mock.Of<ITokenService>(),
            Mock.Of<IConfiguration>(),
            Mock.Of<AutoMapper.IMapper>(),
            Mock.Of<IEmailService>(),
            Mock.Of<IRolesService>()
        );
    }

    // 1️⃣ BAN – actor has permission
    [Fact]
    public async Task BanAccountAsync_WhenActorHasPermission_ShouldBanUser()
    {
        // Arrange
        var actorId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var actor = new ApplicationUser { Id = Guid.Parse(actorId) };
        var target = new ApplicationUser { Id = Guid.Parse(targetId), Banned = false };

        _userManagerMock.Setup(x => x.FindByIdAsync(actorId)).ReturnsAsync(actor);
        _userManagerMock.Setup(x => x.FindByIdAsync(targetId)).ReturnsAsync(target);

        _userManagerMock.Setup(x => x.GetClaimsAsync(actor))
            .ReturnsAsync(new List<Claim>
            {
                new Claim("user.ban", "true")
            });

        _userManagerMock.Setup(x => x.UpdateAsync(target))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(target))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.BanAccountAsync(targetId, actorId);

        // Assert
        Assert.True(target.Banned);
        _userManagerMock.Verify(x => x.UpdateAsync(target), Times.Once);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(target), Times.Once);
    }

    // 2️⃣ BAN – actor has NO permission
    [Fact]
    public async Task BanAccountAsync_WhenActorHasNoPermission_ShouldThrowUnauthorized()
    {
        // Arrange
        var actorId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var actor = new ApplicationUser { Id = Guid.Parse(actorId) };

        _userManagerMock.Setup(x => x.FindByIdAsync(actorId)).ReturnsAsync(actor);
        _userManagerMock.Setup(x => x.GetClaimsAsync(actor))
            .ReturnsAsync(new List<Claim>()); // no permission

        // Act
        Func<Task> act = async () => await _sut.BanAccountAsync(targetId, actorId);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
    }

    // 3️⃣ BAN – target already banned
    [Fact]
    public async Task BanAccountAsync_WhenUserAlreadyBanned_ShouldThrowException()
    {
        var actorId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var actor = new ApplicationUser { Id = Guid.Parse(actorId) };
        var target = new ApplicationUser { Id = Guid.Parse(targetId), Banned = true };

        _userManagerMock.Setup(x => x.FindByIdAsync(actorId)).ReturnsAsync(actor);
        _userManagerMock.Setup(x => x.FindByIdAsync(targetId)).ReturnsAsync(target);

        _userManagerMock.Setup(x => x.GetClaimsAsync(actor))
            .ReturnsAsync(new List<Claim>
            {
                new Claim("user.ban", "true")
            });

        Func<Task> act = async () => await _sut.BanAccountAsync(targetId, actorId);

        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    // 4️⃣ UNBAN – success
    [Fact]
    public async Task UnbanAccountAsync_WhenActorHasPermission_ShouldUnbanUser()
    {
        var actorId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var actor = new ApplicationUser { Id = Guid.Parse(actorId) };
        var target = new ApplicationUser
        {
            Id = Guid.Parse(targetId),
            Banned = true,
            LockoutEnd = DateTimeOffset.MaxValue
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(actorId)).ReturnsAsync(actor);
        _userManagerMock.Setup(x => x.FindByIdAsync(targetId)).ReturnsAsync(target);

        _userManagerMock.Setup(x => x.GetClaimsAsync(actor))
            .ReturnsAsync(new List<Claim>
            {
                new Claim("user.ban", "true")
            });

        _userManagerMock.Setup(x => x.UpdateAsync(target))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.UnbanAccountAsync(targetId, actorId);

        Assert.False(target.Banned);
        Assert.Null(target.LockoutEnd);
        _userManagerMock.Verify(x => x.UpdateAsync(target), Times.Once);
    }

    // 5️⃣ UNBAN – user not banned
    [Fact]
    public async Task UnbanAccountAsync_WhenUserIsNotBanned_ShouldThrowException()
    {
        var actorId = Guid.NewGuid().ToString();
        var targetId = Guid.NewGuid().ToString();

        var actor = new ApplicationUser { Id = Guid.Parse(actorId) };
        var target = new ApplicationUser { Id = Guid.Parse(targetId), Banned = false };

        _userManagerMock.Setup(x => x.FindByIdAsync(actorId)).ReturnsAsync(actor);
        _userManagerMock.Setup(x => x.FindByIdAsync(targetId)).ReturnsAsync(target);

        _userManagerMock.Setup(x => x.GetClaimsAsync(actor))
            .ReturnsAsync(new List<Claim>
            {
                new Claim("user.ban", "true")
            });

        Func<Task> act = async () => await _sut.UnbanAccountAsync(targetId, actorId);

        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}
