using AutoMapper;
using Azure.Core;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Roles;
using Identity.Core.Dtos.Users;
using Identity.Core.Exceptions;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Identity.Core.Services
{
    public class RolesService : IRolesService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IdentityTranslatedErrors _errorDescriber;
        public RolesService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IdentityTranslatedErrors errorDescriber)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _errorDescriber = errorDescriber;
        }
        public async Task<RoleResponse> AddRoleAsync(AddRoleRequest request)
        {
            var role = new ApplicationRole
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _roleManager.CreateAsync(role);
            HandleIdentityResult(result);
            return _mapper.Map<RoleResponse>(role);
        }
        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                throw new KeyNotFoundException(_errorDescriber.RoleNotFound(roleId.ToString()).Description);
            }

            var result = await _roleManager.DeleteAsync(role);
            HandleIdentityResult(result);

        }
        public async Task<RoleResponse> EditRoleAsync(UpdateRoleRequest request)
        {
            var role = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (role == null)
            {
                throw new KeyNotFoundException(_errorDescriber.RoleNotFound(request.Id.ToString()).Description);
            }

            role.Name = request.Name;
            role.Description = request.Description;

            var result = await _roleManager.UpdateAsync(role);
            HandleIdentityResult(result);
            return _mapper.Map<RoleResponse>(role);
        }

        public async Task AddUserToRoleAsync(AssignRoleToUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException(_errorDescriber.UserNotFound(request.UserId.ToString()).Description);
            }

            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                // Using the ErrorDescriber to get the translated error message for consistency
                var error = _errorDescriber.InvalidRoleName(request.RoleName);
                throw new InvalidOperationException(error.Description);
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            HandleIdentityResult(result);
        }

        public async Task RemoveUserFromRoleAsync(AssignRoleToUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException(_errorDescriber.UserNotFound(request.UserId.ToString()).Description);
            }

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);
            HandleIdentityResult(result);
        }

        public async Task<IEnumerable<RoleResponse>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return _mapper.Map<IEnumerable<RoleResponse>>(roles);
        }

        public async Task<RoleResponse> GetRoleByIdAsync(Guid roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
            {
                throw new KeyNotFoundException(_errorDescriber.RoleNotFound(roleId.ToString()).Description);
            }
            return _mapper.Map<RoleResponse>(role);
        }

        public async Task<IEnumerable<RoleResponse>> GetRolesHavingClaimAsync(string claimType, string claimValue)
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            var rolesWithClaim = new List<ApplicationRole>();

            foreach (var role in allRoles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                if (claims.Any(c => c.Type == claimType && c.Value == claimValue))
                {
                    rolesWithClaim.Add(role);
                }
            }

            return _mapper.Map<IEnumerable<RoleResponse>>(rolesWithClaim);
        }
        private void HandleIdentityResult(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errorMessage);
            }
        }


    }
}
