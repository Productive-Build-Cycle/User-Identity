using System;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Core.Data;
using Identity.Core.Domain.Entities;
using Identity.Core.Exceptions;
using Identity.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Identity.Tests.RoleTests;

public class EditRoleAsyncTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly RolesService _sut;

    public EditRoleAsyncTests()
    {
        // RoleManager mock
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null
        );

        // Other required deps (mocked)
        var mapperMock = new Mock<IMapper>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        var errorDescriber = new IdentityTranslatedErrors();

        // No provider needed for these tests (EditRoleAsync doesn't use _context)
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContext = new ApplicationDbContext(options);

        _sut = new RolesService(
            mapperMock.Object,
            userManagerMock.Object,
            _roleManagerMock.Object,
            errorDescriber,
            dbContext
        );
    }

    [Fact]
    public async Task EditRoleAsync_WhenRoleDoesNotExist_ShouldThrowKeyNotFoundException_WithPersianMessage()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // ✅ If your UpdateRoleRequest has (Guid Id, string Name, string Description),
        // add the third parameter here.
        var request = new Identity.Core.Dtos.Roles.UpdateRoleRequest(
            roleId,
            "Admin",
            "Administrator role"
        );

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        Func<Task> act = async () => await _sut.EditRoleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("نقش", ex.Message);

        _roleManagerMock.Verify(
            x => x.UpdateAsync(It.IsAny<ApplicationRole>()),
            Times.Never
        );
    }

    [Fact]
    public async Task EditRoleAsync_WhenUpdateFailsDueToDuplicateRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        var request = new Identity.Core.Dtos.Roles.UpdateRoleRequest(
            roleId,
            "Admin",
            "Administrator role"
        );

        var existingRole = new ApplicationRole
        {
            Id = roleId,
            Name = "User"
        };

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(existingRole);

        // Simulate Identity rejecting the update because "Admin" already exists
        _roleManagerMock
            .Setup(x => x.UpdateAsync(existingRole))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Duplicate Role" }
            ));

        // Act
        Func<Task> act = async () => await _sut.EditRoleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Duplicate Role", ex.Message);

        _roleManagerMock.Verify(x => x.UpdateAsync(existingRole), Times.Once);
    }
}
