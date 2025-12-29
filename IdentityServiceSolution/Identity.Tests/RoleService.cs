using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Roles;
using Identity.Core.Exceptions;
using Identity.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace Identity.Tests
{
    public class RoleServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly RolesService _rolesService;
        private readonly IdentityTranslatedErrors _errorDescriber; // Real instance needed for translation checks

        public RoleServiceTests()
        {
            // 1. Setup UserStore and UserManager Mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // 2. Setup RoleStore and RoleManager Mock
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            Mock.Of<IRoleStore<ApplicationRole>>(),
            null, null, null, null);

            // 3. Setup Mapper Mock
            _mapperMock = new Mock<IMapper>();

            // 4. Setup Error Describer (Use real instance to test actual Persian strings)
            _errorDescriber = new IdentityTranslatedErrors();

            // 5. Initialize the Service with the new dependency
            _rolesService = new RolesService(
                _mapperMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _errorDescriber
            );

        }
        [Fact]
        public async Task DeleteRoleAsync_ShouldThrowKeyNotFoundException_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            // Mock FindByIdAsync to return null (Role not found)
            _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
                .ReturnsAsync((ApplicationRole)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _rolesService.DeleteRoleAsync(roleId);
            });

            // Verify the exception message matches the Persian translation
            var expectedMessage = _errorDescriber.RoleNotFound(roleId.ToString()).Description;
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task EditRoleAsync_ShouldThrowKeyNotFoundException_WhenRoleDoesNotExist()
        {
            // Arrange
            var request = new UpdateRoleRequest { Id = Guid.NewGuid(), Name = "NewName" };

            // Mock FindByIdAsync to return null
            _roleManagerMock.Setup(x => x.FindByIdAsync(request.Id.ToString()))
                .ReturnsAsync((ApplicationRole)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _rolesService.EditRoleAsync(request);
            });

            Assert.Equal($"نقش با شناسه '{request.Id}' یافت نشد.", exception.Message);
        }

        [Fact]
        public async Task AddRoleAsync_ShouldReturnRoleResponse_WhenCreateIsSuccessful()
        {
            // Arrange
            var request = new AddRoleRequest { Name = "Admin", Description = "Admin Role" };
            var role = new ApplicationRole { Name = "Admin", Description = "Admin Role" };

            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResult.Success);

            _mapperMock.Setup(m => m.Map<RoleResponse>(It.IsAny<ApplicationRole>()))
                .Returns(new RoleResponse { Name = "Admin" });

            // Act
            var result = await _rolesService.AddRoleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.Name);
        }

        [Fact]
        public async Task AddUserToRoleAsync_ShouldThrowKeyNotFound_WhenUserNotFound()
        {
            // Arrange
            var request = new AssignRoleToUserRequest { UserId = Guid.NewGuid(), RoleName = "Admin" };
            _userManagerMock.Setup(u => u.FindByIdAsync(request.UserId.ToString()))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _rolesService.AddUserToRoleAsync(request));

            Assert.Equal(_errorDescriber.UserNotFound(request.UserId.ToString()).Description, ex.Message);
        }

        [Fact]
        public async Task AddUserToRoleAsync_ShouldThrowInvalidOperation_WhenRoleDoesNotExist()
        {
            // Arrange
            var request = new AssignRoleToUserRequest { UserId = Guid.NewGuid(), RoleName = "FakeRole" };
            var user = new ApplicationUser { Id = request.UserId };

            _userManagerMock.Setup(u => u.FindByIdAsync(request.UserId.ToString()))
                .ReturnsAsync(user);

            // Mock RoleExistsAsync to return false
            _roleManagerMock.Setup(r => r.RoleExistsAsync(request.RoleName))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _rolesService.AddUserToRoleAsync(request));

            // Verify it uses the translated error for "Invalid Role Name"
            Assert.Equal($"نام نقش '{request.RoleName}' معتبر نیست.", ex.Message);
        }

        [Fact]
        public async Task AddUserToRoleAsync_ShouldSucceed_WhenUserAndRoleExist()
        {
            // Arrange
            var request = new AssignRoleToUserRequest { UserId = Guid.NewGuid(), RoleName = "Admin" };
            var user = new ApplicationUser { Id = request.UserId };

            _userManagerMock.Setup(u => u.FindByIdAsync(request.UserId.ToString())).ReturnsAsync(user);
            _roleManagerMock.Setup(r => r.RoleExistsAsync(request.RoleName)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, request.RoleName))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _rolesService.AddUserToRoleAsync(request);

            // Assert
            _userManagerMock.Verify(u => u.AddToRoleAsync(user, request.RoleName), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFromRoleAsync_ShouldThrowKeyNotFound_WhenUserNotFound()
        {
            // Arrange
            var request = new AssignRoleToUserRequest { UserId = Guid.NewGuid(), RoleName = "Admin" };
            _userManagerMock.Setup(u => u.FindByIdAsync(request.UserId.ToString()))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _rolesService.RemoveUserFromRoleAsync(request));

            Assert.Equal(_errorDescriber.UserNotFound(request.UserId.ToString()).Description, ex.Message);
        }

        [Fact]
        public async Task RemoveUserFromRoleAsync_ShouldSucceed_WhenUserFound()
        {
            // Arrange
            var request = new AssignRoleToUserRequest { UserId = Guid.NewGuid(), RoleName = "Admin" };
            var user = new ApplicationUser { Id = request.UserId };

            _userManagerMock.Setup(u => u.FindByIdAsync(request.UserId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.RemoveFromRoleAsync(user, request.RoleName))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _rolesService.RemoveUserFromRoleAsync(request);

            // Assert
            _userManagerMock.Verify(u => u.RemoveFromRoleAsync(user, request.RoleName), Times.Once);
        }

        [Fact]
        public async Task GetRoleByIdAsync_ShouldReturnNull_WhenRoleNotFound()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            _roleManagerMock.Setup(r => r.FindByIdAsync(roleId.ToString()))
                .ReturnsAsync((ApplicationRole)null);

            // Act
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _rolesService.GetRoleByIdAsync(roleId));

            // Assert
            Assert.Equal(_errorDescriber.RoleNotFound(roleId.ToString()).Description, ex.Message);
        }

        [Fact]
        public async Task GetRoleByIdAsync_ShouldReturnRole_WhenFound()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new ApplicationRole { Id = roleId, Name = "Admin" };
            var roleResponse = new RoleResponse { Id = roleId, Name = "Admin" };

            _roleManagerMock.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
            _mapperMock.Setup(m => m.Map<RoleResponse>(role)).Returns(roleResponse);

            // Act
            var result = await _rolesService.GetRoleByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.Name);
        }
 
    }
}
