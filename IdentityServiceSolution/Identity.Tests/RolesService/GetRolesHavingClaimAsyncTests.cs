using System;
using System.Collections.Generic;
using System.Linq;
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

public class GetRolesHavingClaimAsyncTests
{
    [Fact]
    public async Task GetRolesHavingClaimAsync_ShouldReturnOnlyRolesMatchingClaimTypeAndValue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Seed roles
        var roleMatch = new ApplicationRole { Id = Guid.NewGuid(), Name = "Admin", Description = "Admin role" };
        var roleWrongValue = new ApplicationRole { Id = Guid.NewGuid(), Name = "Mentor", Description = "Mentor role" };
        var roleWrongType = new ApplicationRole { Id = Guid.NewGuid(), Name = "User", Description = "User role" };

        context.Roles.AddRange(roleMatch, roleWrongValue, roleWrongType);

        // Seed claims (RoleClaims table)
        context.RoleClaims.AddRange(
            new IdentityRoleClaim<Guid> { RoleId = roleMatch.Id, ClaimType = "perm", ClaimValue = "x" },       // ✅ match
            new IdentityRoleClaim<Guid> { RoleId = roleWrongValue.Id, ClaimType = "perm", ClaimValue = "y" },  // ❌ wrong value
            new IdentityRoleClaim<Guid> { RoleId = roleWrongType.Id, ClaimType = "other", ClaimValue = "x" }   // ❌ wrong type
        );

        await context.SaveChangesAsync();

        // Mock RoleManager to expose IQueryable Roles from the context
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null, null, null, null
        );
        roleManagerMock.SetupGet(r => r.Roles).Returns(context.Roles);

        // UserManager (required by ctor)
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null
        );

        // Mapper for IEnumerable<RoleResponse>
        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(m => m.Map<IEnumerable<RoleResponse>>(It.IsAny<IEnumerable<ApplicationRole>>()))
            .Returns<IEnumerable<ApplicationRole>>(roles =>
                roles.Select(r => new RoleResponse(r.Id, r.Name!, r.Description)).ToList()
            );

        var errorDescriber = new IdentityTranslatedErrors();

        var sut = new RolesService(
            mapperMock.Object,
            userManagerMock.Object,
            roleManagerMock.Object,
            errorDescriber,
            context
        );

        // Act
        var result = (await sut.GetRolesHavingClaimAsync("perm", "x")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(roleMatch.Id, result[0].Id);
        Assert.Equal("Admin", result[0].Name);
    }
}
