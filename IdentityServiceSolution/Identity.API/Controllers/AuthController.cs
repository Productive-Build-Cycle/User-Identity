using Identity.Core.Dtos.Auth;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    public class AuthController(IUserService userService) : BaseController
    {
        private readonly IUserService _userService = userService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var response = await _userService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var response = await _userService.LoginAsync(request);
            return Ok(response);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var authResponse = await _userService.ConfirmEmailAsync(userId, token);
            return Ok(authResponse);
        }

    }
}
