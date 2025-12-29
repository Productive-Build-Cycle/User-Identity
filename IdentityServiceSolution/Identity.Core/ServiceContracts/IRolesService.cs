using Identity.Core.Dtos.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.ServiceContracts
{
    public interface IRolesService
    {

        /// Adds a new role based on the AddRoleRequest.
        Task<RoleResponse> AddRoleAsync(AddRoleRequest request);

        /// Deletes a role by its unique Identifier.
        Task DeleteRoleAsync(Guid roleId);

        /// Updates an existing role based on the UpdateRoleRequest.
        Task<RoleResponse> EditRoleAsync(UpdateRoleRequest request);

        /// Adds a user to a role using the AssignRoleToUserRequest (UserId and RoleName).
        Task AddUserToRoleAsync(AssignRoleToUserRequest request);

        /// Removes a user from a role using the AssignRoleToUserRequest (UserId and RoleName).
        Task RemoveUserFromRoleAsync(AssignRoleToUserRequest request);

        /// Retrieves all roles.
        Task<IEnumerable<RoleResponse>> GetAllRolesAsync();

        /// Retrieves a specific role by Id.
        Task<RoleResponse> GetRoleByIdAsync(Guid roleId);

        /// Retrieves all roles that possess a specific claim type and value.
        Task<IEnumerable<RoleResponse>> GetRolesHavingClaimAsync(string claimType, string claimValue);
    }
}
