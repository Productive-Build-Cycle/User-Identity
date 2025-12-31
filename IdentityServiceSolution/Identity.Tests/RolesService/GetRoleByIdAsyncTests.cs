using System;
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

public class GetRoleByIdAsyncTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RolesService _sut;

    public GetRoleByIdAsyncTests()
    {
        _mapperMock = new Mock<IMapper>();

        // RoleManager mock
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null
        );

        // UserManager (required by constructor, not used here)
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        var errorDescriber = new IdentityTranslatedErrors();

        // DbContext exists but is NOT used in this method
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
        Func<Task> act = async () => await _sut.GetRoleByIdAsync(roleId);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("نقش", ex.Message); // Persian message (loose check)
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

        var expectedResponse = new RoleResponse(
            role.Id,
            role.Name!,
            role.Description
        );

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        _mapperMock
            .Setup(m => m.Map<RoleResponse>(role))
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetRoleByIdAsync(roleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Description, result.Description);
    }
}
