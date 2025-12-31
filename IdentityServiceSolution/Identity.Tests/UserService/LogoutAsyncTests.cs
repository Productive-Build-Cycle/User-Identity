using System;
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

public class LogoutAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

    private readonly UserrService _sut;

    public LogoutAsyncTests()
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

    [Fact]
    public async Task LogoutAsync_WhenUserExists_ShouldUpdateSecurityStampAndSignOut()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = Guid.Parse(userId)
        };

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _signInManagerMock
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogoutAsync(userId);

        // Assert
        _userManagerMock.Verify(
            x => x.UpdateSecurityStampAsync(user),
            Times.Once
        );

        _signInManagerMock.Verify(
            x => x.SignOutAsync(),
            Times.Once
        );
    }
}
