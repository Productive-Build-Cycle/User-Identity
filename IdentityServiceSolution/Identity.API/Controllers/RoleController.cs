using Identity.API.Controllers;
using Identity.Core.Dtos.Roles;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {
        private readonly IRolesService _roleService;

        public RoleController(IRolesService roleService)
        {
            _roleService = roleService;
        }

        // -------------------- Roles CRUD --------------------

        [HttpPost]
        [Authorize(Policy = "role.create")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleRequest request)
        {
            var result = await _roleService.AddRoleAsync(request);
            return FromResult(result);
        }

        [HttpPut("{roleId}")]
        [Authorize(Policy = "role.update")]
        public async Task<IActionResult> EditRole(
            [FromRoute] string roleId,
            [FromBody] UpdateRoleRequest request)
        {
            var result = await _roleService.EditRoleAsync(roleId, request);
            return FromResult(result);
        }

        [HttpDelete("{roleId:guid}")]
        [Authorize(Policy = "role.delete")]
        public async Task<IActionResult> DeleteRole([FromRoute] Guid roleId)
        {
            var result = await _roleService.DeleteRoleAsync(roleId);
            return FromResult(result);
        }

        [HttpGet("{roleId:guid}")]
        public async Task<IActionResult> GetRoleById([FromRoute] Guid roleId)
        {
            var result = await _roleService.GetRoleByIdAsync(roleId);
            return FromResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _roleService.GetAllRolesAsync();
            return FromResult(result);
        }

        // -------------------- Users & Roles --------------------

        [HttpPost("assign")]
        [Authorize(Policy = "role.assign")]
        public async Task<IActionResult> AssignRoleToUser(
            [FromBody] AssignRoleToUserRequest request)
        {
            var result = await _roleService.AddUserToRoleAsync(request);
            return FromResult(result);
        }

        [HttpPost("remove")]
        [Authorize(Policy = "role.assign")]
        public async Task<IActionResult> RemoveRoleFromUser(
            [FromBody] AssignRoleToUserRequest request)
        {
            var result = await _roleService.RemoveUserFromRoleAsync(request);
            return FromResult(result);
        }
        [HttpGet("{roleId:guid}/users")]
        public async Task<IActionResult> GetAllUsersOfRole([FromRoute] Guid roleId)
        {
            var result = await _roleService.GetAllUsersOfRoleAsync(roleId);
            return FromResult(result);
        }


        // -------------------- Claims --------------------

        [HttpPost("{roleId:guid}/claims")]
        [Authorize(Policy = "role.create")]
        public async Task<IActionResult> AddClaimToRole(
            [FromRoute] Guid roleId,
            [FromQuery] string type,
            [FromQuery] string value)
        {
            var result = await _roleService.AddClaimToRoleAsync(roleId, type, value);
            return FromResult(result);
        }

        [HttpGet("by-claim")]
        public async Task<IActionResult> GetRolesByClaim(
            [FromQuery] string claimType,
            [FromQuery] string claimValue)
        {
            var result = await _roleService.GetRolesHavingClaimAsync(claimType, claimValue);
            return FromResult(result);
        }

        [HttpDelete("{roleId:guid}/claims")]
        [Authorize(Policy = "role.create")]
        public async Task<IActionResult> RemoveClaimFromRole(
            [FromRoute] Guid roleId,
            [FromQuery] string type,
            [FromQuery] string value)
        {
            var result = await _roleService.RemoveClaimFromRoleAsync(roleId, type, value);
            return FromResult(result);
        }
    }
}
