using System;
using System.Threading.Tasks;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
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

public class ChangePasswordAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

    private readonly UserrService _sut;

    public ChangePasswordAsyncTests()
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

    // 1️⃣ SUCCESS – current password correct
    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsCorrect_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        var request = new ChangePasswordRequest(
            userId,
            "OldPassword123!",
            "NewPassword123!",
            "NewPassword123!"   // ConfirmNewPassword
        );

        var user = new ApplicationUser
        {
            Id = Guid.Parse(userId)
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.ChangePasswordAsync(request);

        // Assert
        _userManagerMock.Verify(
            x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword),
            Times.Once
        );

        _userManagerMock.Verify(
            x => x.UpdateSecurityStampAsync(user),
            Times.Once
        );
    }

    // 2️⃣ FAILURE – current password wrong
    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsWrong_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        var request = new ChangePasswordRequest(
            userId,
            "WrongPassword!",
            "NewPassword123!",
            "NewPassword123!"
        );

        var user = new ApplicationUser
        {
            Id = Guid.Parse(userId)
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Invalid current password" }
            ));

        // Act
        Func<Task> act = async () => await _sut.ChangePasswordAsync(request);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);

        _userManagerMock.Verify(
            x => x.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()),
            Times.Never
        );
    }

    // 3️⃣ FAILURE – user does not exist
    [Fact]
    public async Task ChangePasswordAsync_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        var request = new ChangePasswordRequest(
            userId,
            "OldPassword123!",
            "NewPassword123!",
            "NewPassword123!"
        );

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        Func<Task> act = async () => await _sut.ChangePasswordAsync(request);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);

        _userManagerMock.Verify(
            x => x.ChangePasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never
        );
    }
}
