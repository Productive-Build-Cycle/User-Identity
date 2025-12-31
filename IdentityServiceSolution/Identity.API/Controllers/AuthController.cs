using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("test-auth")]
    public IActionResult TestAuth()
    {
        return Ok(new
        {
            User.Identity?.IsAuthenticated,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _userService.RegisterAsync(request);
        return FromResult<RergisterResponse>(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _userService.LoginAsync(request);
        return FromResult<AuthResponse>(result);
    }

    [AllowAnonymous]
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _userService.ConfirmEmailAsync(userId, token);
        return FromResult<AuthResponse>(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromQuery] string userId)
    {
        var result = await _userService.LogoutAsync(userId);
        return FromResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _userService.ChangePasswordAsync(request);
        return FromResult(result);
    }

    [HttpPut("update")]
    [Authorize(Policy = "user.update")]
    public async Task<IActionResult> UpdateAccount([FromQuery] string userId, [FromBody] UpdateUserRequest request)
    { 
        var user = await _userService.GetUserByEmail(User.Identity.Name);

        if (request.Id != user.Id || !User.IsInRole("Admin"))
            return Problem("دسترسی نامعتبر", "شما دسترسی لازم برای انجام این عملیات را ندارید",
                statusCode: StatusCodes.Status403Forbidden);

        var result = await _userService.UpdateAccountAsync(userId, request);
        return FromResult(result);
    }

    [HttpDelete("delete")]
    [Authorize(Policy = "user.delete")]
    public async Task<IActionResult> DeleteAccount([FromQuery] string userId)
    {
        var user = await _userService.GetUserByEmail(User.Identity.Name);

        if (Guid.Parse(userId) != user.Id || !User.IsInRole("Admin"))
            return Problem("دسترسی نامعتبر", "شما دسترسی لازم برای انجام این عملیات را ندارید",
                statusCode: StatusCodes.Status403Forbidden);

        var result = await _userService.DeleteAccountAsync(userId);
        return FromResult(result);
    }

    [HttpPost("ban")]
    [Authorize(Policy = "user.ban")]
    public async Task<IActionResult> BanAccount([FromBody] BanAccountRequest request)
    {
        var result = await _userService.BanAccountAsync(request);
        return FromResult(result);
    }

    [HttpPost("unban")]
    [Authorize(Policy = "user.unban")]
    public async Task<IActionResult> UnbanAccount([FromBody] BanAccountRequest request)
    {
        var result = await _userService.UnbanAccountAsync(request);
        return FromResult(result);
    }
}
