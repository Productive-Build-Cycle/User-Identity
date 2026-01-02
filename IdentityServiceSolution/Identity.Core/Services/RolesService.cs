using AutoMapper;
using Azure.Core;
using FluentResults;
using Identity.Core.Data;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Roles;
using Identity.Core.Dtos.Users;
using Identity.Core.Exceptions;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Net;
using System.Security.Claims;


namespace Identity.Core.Services
{
    public class RolesService : IRolesService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public RolesService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // -------------------- Roles CRUD --------------------
        public async Task<Result<RoleResponse>> AddRoleAsync(AddRoleRequest request)
        {
            // 1. Validate role name
            if (!SystemRoles.Allowed.Contains(request.Name))
            {
                return Result.Fail(RoleErrors.InvalidRoleName(request.Name));
            }
            // 2. Check duplicate role by name
            var existingRole = await _roleManager.FindByNameAsync(request.Name);
            if (existingRole is not null)
                return Result.Fail(RoleErrors.DuplicateRole(existingRole.Id));

            // 3. Map request to domain model
            var role = _mapper.Map<ApplicationRole>(request);

            // 3. Create role
            var createResult = await _roleManager.CreateAsync(role);

            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest));

                return Result.Fail(errors);
            }

            // 4. Map response
            var response = new RoleResponse(
                Id: role.Id,
                Name: role.Name,
                Description: role.Description
            );

            return Result.Ok(response);
        }
        public async Task<Result> DeleteRoleAsync(Guid roleId)
        {
            // 1. Finde role by name
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return Result.Fail(RoleErrors.RoleNotFound(roleId.ToString()));
            }
            // 2. Check assigned users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                return Result.Fail(RoleErrors.RoleHasAssignedUsers(roleId));
            }
            // 3. Check role by claims
            var claims = await _roleManager.GetClaimsAsync(role);
            if (claims.Any())
            {
                return Result.Fail(RoleErrors.RoleHasClaims(roleId));
            }
            // 4. Delete role
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return Result.Fail(result.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));
            return Result.Ok();

        }

        public async Task<Result<RoleResponse>> EditRoleAsync(string RoleName, UpdateRoleRequest request)
        {
            // 1. Finde role by name
            var role = await _roleManager.FindByIdAsync(RoleName);
            if (role is null)
            {
                return Result.Fail(RoleErrors.RoleNotFound(RoleName));
            }
            // 2. Validate new role name
            if (!SystemRoles.Allowed.Contains(request.Name))
            {
                return Result.Fail(RoleErrors.InvalidRoleName(request.Name));
            }

            // 3. Prevent renaming to existing role
            var duplicate = await _roleManager.FindByNameAsync(request.Name);
            if (duplicate is not null && duplicate.Id != role.Id)
            {
                return Result.Fail(RoleErrors.DuplicateRole(duplicate.Id));
            }
            // 4. Edit  role
            role.Name = request.Name;
            role.Description = request.Description;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return Result.Fail(result.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));

            return Result.Ok();
        }

        public async Task<Result<RoleResponse>> GetRoleByIdAsync(Guid roleId)
        {
            // 1. Finde role by id
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return Result.Fail(RoleErrors.RoleNotFound(roleId));
            }
            // 2. Map role to response
            var response = _mapper.Map<RoleResponse>(role);
            return Result.Ok(response);
        }

        public async Task<Result<IEnumerable<RoleResponse>>> GetAllRolesAsync()
        {
            // 1. Fetch all roles
            var roles = await _roleManager.Roles.ToListAsync();

            // 2. Map roles to response
            var response = _mapper.Map<IEnumerable<RoleResponse>>(roles);
            return Result.Ok(response);
        }
        // -------------------- Users & Roles --------------------
        public async Task<Result> AddUserToRoleAsync(AssignRoleToUserRequest request)
        {
            // 1. Finde user by userId
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                return Result.Fail(Errors.UserNotFound(request.UserId.ToString()));
            }
            // 2. Finde role by name
            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                return Result.Fail(RoleErrors.RoleNotFound(request.RoleName));
            }
            // 3. Assign role to user
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
                return Result.Fail(result.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));

            return Result.Ok();
        }

        public async Task<Result> RemoveUserFromRoleAsync(AssignRoleToUserRequest request)
        {
            // 1. Finde user by userId
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                return Result.Fail(Errors.UserNotFound(request.UserId.ToString()));
            }
            // 2. Remove role from user
            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
                return Result.Fail(result.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));

            return Result.Ok();
        }
        public async Task<Result<IEnumerable<UserResponse>>> GetAllUsersOfRoleAsync(Guid roleId)
        {
            // 1. Find role by id
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return Result.Fail(RoleErrors.RoleNotFound(roleId.ToString()));
            }

            // 2. Get users in role
            var users = await _userManager.GetUsersInRoleAsync(role.Name!);

            // 3. Map users to response
            var response = _mapper.Map<IEnumerable<UserResponse>>(users);

            return Result.Ok(response);
        }
        // -------------------- Claims --------------------
        public async Task<Result<IEnumerable<RoleResponse>>> GetRolesHavingClaimAsync(string claimType, string claimValue)
        {
            // 1. Fetch all roles by claims
            var roles = await _roleManager.Roles
                .Where(r => _context.RoleClaims.Any(c =>
                    c.RoleId == r.Id &&
                    c.ClaimType == claimType &&
                    c.ClaimValue == claimValue))
                .ToListAsync();

            // 2. Map roles to response
            var response = _mapper.Map<IEnumerable<RoleResponse>>(roles);

            return Result.Ok(response);
        }
        public async Task<Result> AddClaimToRoleAsync(Guid roleId,string type,string value)
        {
            // 1. Find role by id
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return Result.Fail(RoleErrors.RoleNotFound(roleId.ToString()));
            }
            // 2. Check duplicate claim
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            if (existingClaims.Any(c => c.Type == type && c.Value == value))
            {
                return Result.Fail(RoleErrors.ClaimAlreadyExistsForRole(roleId, type, value));
            }

            // 3. Add claim
            var result = await _roleManager.AddClaimAsync(role, new Claim(type, value));
            if (!result.Succeeded)
            {
                return Result.Fail(result.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));
            }

            return Result.Ok();
        }

        public async Task<Result> RemoveClaimFromRoleAsync(Guid roleId, string type, string value)
        {
            // 1. Find role by id
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                return Result.Fail(RoleErrors.RoleNotFound(roleId.ToString()));
            }

            // 2. Get existing claims
            var claims = await _roleManager.GetClaimsAsync(role);

            var claim = claims.FirstOrDefault(c => c.Type == type && c.Value == value);
            if (claim is null)
            {
                return Result.Fail(RoleErrors.ClaimNotFoundForRole(roleId, type, value));
            }

            // 3. Remove claim
            var result = await _roleManager.RemoveClaimAsync(role, claim);
            if (!result.Succeeded)
            {
                return Result.Fail(result.Errors.Select(e =>
                    new Error(e.Description)
                        .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));
            }

            return Result.Ok();
        }
        


    }
}
