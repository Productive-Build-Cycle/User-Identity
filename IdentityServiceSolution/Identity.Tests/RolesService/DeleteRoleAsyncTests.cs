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

public class DeleteRoleAsyncTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly RolesService _sut;

    public DeleteRoleAsyncTests()
    {
        // RoleManager mock
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null
        );

        // Other deps (mocked)
        var mapperMock = new Mock<IMapper>();

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        var errorDescriber = new IdentityTranslatedErrors();

        // ✅ DbContext REAL instance, but with empty options (NO provider, NO extra packages)
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
    public async Task DeleteRoleAsync_WhenRoleDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        _roleManagerMock
            .Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        Func<Task> act = async () => await _sut.DeleteRoleAsync(roleId);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Contains("نقش", ex.Message);

        _roleManagerMock.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationRole>()),
            Times.Never
        );
    }
}
