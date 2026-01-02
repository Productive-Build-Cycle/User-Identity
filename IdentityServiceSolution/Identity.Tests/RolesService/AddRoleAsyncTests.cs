using AutoMapper;
using Identity.Core.Data;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Roles;
using Identity.Core.Exceptions;
using Identity.Core.ServiceContracts;
using Identity.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
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
            dbContext
        );
    }
    //sucess
    [Fact]
    public async Task AddRoleAsync_ShouldReturnSuccessResult_WhenRoleIsValid()
    {
        // Arrange
        var roleName = "admin";

        var request = new AddRoleRequest
        {
            Name = roleName,
            Description = $"{roleName} role"
        };

        _roleManagerMock
            .Setup(r => r.FindByNameAsync(roleName))
            .ReturnsAsync((ApplicationRole)null);

        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        _mapperMock
            .Setup(m => m.Map<ApplicationRole>(It.IsAny<AddRoleRequest>()))
            .Returns(new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                Description = request.Description
            });

        // Act
        var result = await _sut.AddRoleAsync(request);

        // Assert (Result Pattern ✅)
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailed);
        Assert.Empty(result.Errors);

        var response = result.Value;
        Assert.NotNull(response);
        Assert.Equal(roleName, response.Name);
        Assert.Equal($"{roleName} role", response.Description);
        Assert.NotEqual(Guid.Empty, response.Id);

        _roleManagerMock.Verify(
            r => r.CreateAsync(It.Is<ApplicationRole>(x =>
                x.Name == roleName &&
                x.Description == $"{roleName} role")),
            Times.Once);

        _roleManagerMock.VerifyNoOtherCalls();
    }
    //failur
    [Fact]
    public async Task AddRoleAsync_ShouldFail_WhenRoleNameIsInvalid()
    {
        // Arrange
        var request = new AddRoleRequest
        {
            Name = "superadmin",
            Description = "invalid role"
        };

        // Act
        var result = await _sut.AddRoleAsync(request);

        // Assert
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);

        var error = Assert.Single(result.Errors);
        Assert.Contains("مجاز نیست", error.Message);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            error.Metadata[ErrorMetadataKeys.StatusCode]);

        _roleManagerMock.Verify(
            r => r.FindByNameAsync(It.IsAny<string>()),
            Times.Never);

        _roleManagerMock.Verify(
            r => r.CreateAsync(It.IsAny<ApplicationRole>()),
            Times.Never);
    }
    //failur
    [Fact]
    public async Task AddRoleAsync_ShouldFail_WhenRoleAlreadyExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleName = "admin";

        var existingRole = new ApplicationRole
        {
            Id = roleId,
            Name = roleName
        };

        _roleManagerMock
            .Setup(r => r.FindByNameAsync(roleName))
            .ReturnsAsync(existingRole);

        var request = new AddRoleRequest
        {
            Name = roleName,
            Description = "duplicate role"
        };

        // Act
        var result = await _sut.AddRoleAsync(request);

        // Assert
        Assert.True(result.IsFailed);

        var error = Assert.Single(result.Errors);
        Assert.Contains("قبلاً ثبت شده", error.Message);
        Assert.Equal(
            HttpStatusCode.Conflict,
            error.Metadata[ErrorMetadataKeys.StatusCode]);

        _roleManagerMock.Verify(
            r => r.CreateAsync(It.IsAny<ApplicationRole>()),
            Times.Never);
    }
    //failur
    [Fact]
    public async Task AddRoleAsync_ShouldFail_WhenIdentityCreateFails()
    {
        // Arrange
        var roleName = "admin";

        _roleManagerMock
            .Setup(r => r.FindByNameAsync(roleName))
            .ReturnsAsync((ApplicationRole)null);

        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Identity error" }));

        _mapperMock
            .Setup(m => m.Map<ApplicationRole>(It.IsAny<AddRoleRequest>()))
            .Returns(new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName
            });

        var request = new AddRoleRequest
        {
            Name = roleName,
            Description = "test role"
        };

        // Act
        var result = await _sut.AddRoleAsync(request);

        // Assert
        Assert.True(result.IsFailed);

        var error = Assert.Single(result.Errors);
        Assert.Equal("Identity error", error.Message);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            error.Metadata[ErrorMetadataKeys.StatusCode]);
    }





}
