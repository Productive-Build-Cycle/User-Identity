using FluentResults;
using Identity.Core.Dtos.Roles;
using Identity.Core.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.ServiceContracts
{
    public interface IRolesService
    {
        // -------------------- Roles CRUD --------------------
        /// Adds a new role based on the AddRoleRequest.
        Task<Result<RoleResponse>> AddRoleAsync(AddRoleRequest request);

        /// Deletes a role by its unique Identifier.
        Task<Result> DeleteRoleAsync(Guid roleId);

        /// Updates an existing role based on the UpdateRoleRequest.
        Task<Result<RoleResponse>> EditRoleAsync(string RoleName, UpdateRoleRequest request);

        /// Retrieves a specific role by Id.
        Task<Result<RoleResponse>> GetRoleByIdAsync(Guid roleId);

        /// Retrieves all roles.
        Task<Result<IEnumerable<RoleResponse>>> GetAllRolesAsync();

        // -------------------- Users & Roles --------------------
        /// Adds a user to a role using the AssignRoleToUserRequest (UserId and RoleName).
        Task<Result> AddUserToRoleAsync(AssignRoleToUserRequest request);

        /// Retrieves all users that possess a specific role.
        Task<Result<IEnumerable<UserResponse>>> GetAllUsersOfRoleAsync(Guid roleId);

        /// Removes a user from a role using the AssignRoleToUserRequest (UserId and RoleName).
        Task<Result> RemoveUserFromRoleAsync(AssignRoleToUserRequest request);

        // -------------------- Claims --------------------
        /// Retrieves all roles that possess a specific claim type and value.
        Task<Result<IEnumerable<RoleResponse>>> GetRolesHavingClaimAsync(string claimType, string claimValue);

        /// Add new claim by type and value to a role.
        Task<Result> AddClaimToRoleAsync(Guid roleId, string type, string value);

        /// Removes a claim from a role using the type and value.
        Task<Result> RemoveClaimFromRoleAsync(Guid roleId, string type, string value);

        

    }
}
