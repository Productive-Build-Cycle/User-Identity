using System;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Core.Data;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Roles;
using Identity.Core.Services;
using Identity.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Identity.Tests.RoleTests;

public class GetRoleByIdAsyncTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RolesService _sut;

    public GetRoleByIdAsyncTests()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null!, null!, null!, null!
        );

        _mapperMock = new Mock<IMapper>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!
        );

        var errorDescriber = new IdentityTranslatedErrors();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContext = new ApplicationDbContext(options);

        _sut = new RolesService(
            _mapperMock.Object,
            userManagerMock.Object,
            _roleManagerMock.Object,
            errorDescriber,
            dbContext
        );
    }

    [Fact]
    public async Task GetRoleByIdAsync_WhenRoleDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        var act = async () => await _sut.GetRoleByIdAsync(roleId);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _mapperMock.Verify(
            m => m.Map<RoleResponse>(It.IsAny<ApplicationRole>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetRoleByIdAsync_WhenRoleExists_ShouldReturnRoleResponse()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        var role = new ApplicationRole
        {
            Id = roleId,
            Name = "Admin",
            Description = "Administrator role"
        };

        var response = new RoleResponse(role.Id, role.Name!, role.Description);

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _mapperMock
            .Setup(m => m.Map<RoleResponse>(role))
            .Returns(response);

        // Act
        var result = await _sut.GetRoleByIdAsync(roleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleId, result.Id);
        Assert.Equal("Admin", result.Name);

        _mapperMock.Verify(
            m => m.Map<RoleResponse>(role),
            Times.Once
        );
    }
}
