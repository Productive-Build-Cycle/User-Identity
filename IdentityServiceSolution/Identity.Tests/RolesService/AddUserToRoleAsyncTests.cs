using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Core.Data;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Roles;
using Identity.Core.Exceptions;
using Identity.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Identity.Tests.RoleTests;

public class AddUserToRoleAsyncTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly RolesService _sut;

    public AddUserToRoleAsyncTests()
    {
        var mapperMock = new Mock<IMapper>();
        var errorDescriber = new IdentityTranslatedErrors();

        // UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        // RoleManager mock
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null
        );

        // No extra packages: DbContext exists but not used here
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContext = new ApplicationDbContext(dbOptions);

        _sut = new RolesService(
            mapperMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            errorDescriber,
            dbContext
        );
    }

    [Fact]
    public async Task AddUserToRoleAsync_WhenUserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var request = new AssignRoleToUserRequest(
            UserId: Guid.NewGuid(),
            RoleName: "User"
        );

        _userManagerMock
            .Setup(x => x.FindByIdAsync(request.UserId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        Func<Task> act = async () => await _sut.AddUserToRoleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("کاربر", ex.Message); // Persian message (loose check)

        _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddUserToRoleAsync_WhenRoleDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid() };

        var request = new AssignRoleToUserRequest(
            UserId: user.Id,
            RoleName: "NotExistingRole"
        );

        _userManagerMock
            .Setup(x => x.FindByIdAsync(request.UserId.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(request.RoleName))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _sut.AddUserToRoleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Role", ex.Message, StringComparison.OrdinalIgnoreCase); // could be Persian or English depending on describer

        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddUserToRoleAsync_WhenUserAndRoleExist_ShouldSucceed()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid() };

        var request = new AssignRoleToUserRequest(
            UserId: user.Id,
            RoleName: "User"
        );

        _userManagerMock
            .Setup(x => x.FindByIdAsync(request.UserId.ToString()))
            .ReturnsAsync(user);

        _roleManagerMock
            .Setup(x => x.RoleExistsAsync(request.RoleName))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.AddToRoleAsync(user, request.RoleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _sut.AddUserToRoleAsync(request);

        // Assert
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, request.RoleName), Times.Once);
    }
}
