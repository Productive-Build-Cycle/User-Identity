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
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null!, null!, null!, null!
        );

        var mapperMock = new Mock<IMapper>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!
        );

        var errorDescriber = new IdentityTranslatedErrors();

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
    public async Task EditRoleAsync_WhenRoleDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        var request = new Identity.Core.Dtos.Roles.UpdateRoleRequest(
            roleId,
            "Admin",
            "Administrator role"
        );

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        var act = async () => await _sut.EditRoleAsync(request);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _roleManagerMock.Verify(
            x => x.UpdateAsync(It.IsAny<ApplicationRole>()),
            Times.Never
        );
    }

    [Fact]
    public async Task EditRoleAsync_WhenUpdateFails_ShouldThrowInvalidOperationException_AndCallUpdateOnce()
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

        _roleManagerMock
            .Setup(x => x.UpdateAsync(existingRole))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Duplicate Role" }
            ));

        // Act
        var act = async () => await _sut.EditRoleAsync(request);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);

        _roleManagerMock.Verify(
            x => x.UpdateAsync(existingRole),
            Times.Once
        );
    }
}
