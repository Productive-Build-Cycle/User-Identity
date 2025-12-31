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

public class DeleteAccountAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

    private readonly UserrService _sut;

    public DeleteAccountAsyncTests()
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

    // 1️⃣ SUCCESS – user exists, delete succeeds
    [Fact]
    public async Task DeleteAccountAsync_WhenUserExists_ShouldDeleteUser()
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
            .Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.DeleteAccountAsync(userId);

        // Assert
        _userManagerMock.Verify(
            x => x.DeleteAsync(user),
            Times.Once
        );
    }

    // 2️⃣ FAILURE – user does not exist
    [Fact]
    public async Task DeleteAccountAsync_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteAccountAsync(userId);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);

        _userManagerMock.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Never
        );
    }
}
