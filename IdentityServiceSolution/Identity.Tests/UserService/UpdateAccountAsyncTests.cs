using System;
using System.Threading.Tasks;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Users;
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

public class UpdateAccountAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;

    private readonly UserrService _sut;

    public UpdateAccountAsyncTests()
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

    // 1️⃣ SUCCESS – user exists
    [Fact]
    public async Task UpdateAccountAsync_WhenUserExists_ShouldUpdateUserInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIdString = userId.ToString();

        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "Old",
            LastName = "Name",
            Email = "old@test.com",
            UserName = "old@test.com",
            PhoneNumber = "0000000000",
           
        };

        var request = new UpdateUserRequest(
            userId,                  // Id (REQUIRED)
            "New",
            "Name",
            "new@test.com",
            "1111111111",
            true                     // IsActive
        );

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userIdString))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.UpdateAccountAsync(userIdString, request);

        // Assert
        Assert.Equal("New", user.FirstName);
        Assert.Equal("Name", user.LastName);
        Assert.Equal("new@test.com", user.Email);
        Assert.Equal("new@test.com", user.UserName);
        Assert.Equal("1111111111", user.PhoneNumber);

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    // 2️⃣ FAILURE – user does not exist
    [Fact]
    public async Task UpdateAccountAsync_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIdString = userId.ToString();

        var request = new UpdateUserRequest(
            userId,
            "New",
            "Name",
            "new@test.com",
            "1111111111",
            true
        );

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userIdString))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        Func<Task> act = async () => await _sut.UpdateAccountAsync(userIdString, request);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);

        _userManagerMock.Verify(
            x => x.UpdateAsync(It.IsAny<ApplicationUser>()),
            Times.Never
        );
    }
}

