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

public class AddRoleAsyncTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RolesService _sut;

    public AddRoleAsyncTests()
    {
        // RoleManager mock
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null
        );

        // Other required deps (mocked)
        _mapperMock = new Mock<IMapper>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        var errorDescriber = new IdentityTranslatedErrors();

        // No extra packages: just create DbContext with empty options (not used in AddRoleAsync)
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var dbContext = new ApplicationDbContext(dbOptions);

        _sut = new RolesService(
            _mapperMock.Object,
            userManagerMock.Object,
            _roleManagerMock.Object,
            errorDescriber,
            dbContext
        );
    }

    [Fact]
    public async Task AddRoleAsync_WhenRoleIsDuplicate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        // ✅ If your AddRoleRequest constructor is (string Name, string Description), keep this:
        var request = new AddRoleRequest(
            Name: "Admin",
            Description: "Administrator role"
        );

        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Duplicate Role" }));

        // Act
        Func<Task> act = async () => await _sut.AddRoleAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains("Duplicate Role", ex.Message);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Mentor")]
    [InlineData("User")]
    public async Task AddRoleAsync_WhenRoleIsValid_ShouldReturnRoleResponse(string roleName)
    {
        // Arrange
        var request = new AddRoleRequest(
            Name: roleName,
            Description: $"{roleName} role"
        );

        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Mapper should return RoleResponse based on created role
        _mapperMock
            .Setup(m => m.Map<RoleResponse>(It.IsAny<ApplicationRole>()))
            .Returns<ApplicationRole>(role =>
            {
                // ✅ If your RoleResponse is (Guid Id, string Name, string Description), keep this:
                return new RoleResponse(role.Id, role.Name!, role.Description);
            });

        // Act
        var result = await _sut.AddRoleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleName, result.Name);

        _roleManagerMock.Verify(r =>
            r.CreateAsync(It.Is<ApplicationRole>(x =>
                x.Name == roleName &&
                x.Description == $"{roleName} role")),
            Times.Once);
    }
}
