using AutoMapper;
using FluentResults;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Roles;
using Identity.Core.Dtos.Users;
using Identity.Core.Exceptions;
using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;

namespace Identity.Core.Services;

public class UserrService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _conf;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IRolesService _roleService;

    public UserrService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IConfiguration conf,
        IMapper mapper,
        IEmailService emailService,
        IRolesService roleService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _conf = conf;
        _mapper = mapper;
        _emailService = emailService;
        _roleService = roleService;
    }

    public async Task<Result<RergisterResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Fail(Errors.DuplicateEmail(request.Email));

        var user = _mapper.Map<ApplicationUser>(request);

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Fail(createResult.Errors.Select(e =>
                new Error(e.Description)
                    .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));

        await _roleService.AddUserToRoleAsync(
            new AssignRoleToUserRequest(user.Email!, "User"));

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmUrl =
            $"{_conf["BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var body = await _emailService.TurnHtmlToString(
            "EmailConfirmation.html",
            new Dictionary<string, string>
            {
                ["VerificationLink"] = confirmUrl,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            });

        await _emailService.SendEmailAsync(
            new EmailOptions(user.Email!, "تایید حساب کاربری", body));

        return Result.Ok(new RergisterResponse(
            IsEmailConfirmed: false,
            Email: user.Email!,
            Message: "ثبت نام با موفقیت انجام شد. لطفاً ایمیل خود را برای تایید حساب کاربری بررسی کنید."
        ));
    }

    public async Task<Result<AuthResponse>> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return Result.Fail(Errors.InvalidToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Fail(Errors.UserNotFound(userId));

        var decodedToken =
            Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var confirmResult =
            await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!confirmResult.Succeeded)
            return Result.Fail(Errors.InvalidToken);

        var auth = await _tokenService.GenerateToken(user);
        return Result.Ok(auth);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail(Errors.InvalidCredentials);

        if (user.Banned)
            return Result.Fail(Errors.AccountBanned);

        if (!user.EmailConfirmed)
            return Result.Fail(Errors.EmailNotConfirmed);

        if (user.LockoutEnd.HasValue &&
            user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            var remaining = user.LockoutEnd.Value - DateTimeOffset.UtcNow;
            return Result.Fail(Errors.AccountLocked(remaining));
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
            return Result.Fail(Errors.InvalidCredentials);

        if (user.LockoutMultiplier > 1)
        {
            user.LockoutMultiplier = 1;
            await _userManager.UpdateAsync(user);
        }

        var auth = await _tokenService.GenerateToken(user);
        return Result.Ok(auth);
    }

    public async Task<Result> LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
            await _userManager.UpdateSecurityStampAsync(user);

        await _signInManager.SignOutAsync();
        return Result.Ok();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail(Errors.UserNotFound(request.Email));

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(e =>
                new Error(e.Description)
                    .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));

        await _userManager.UpdateSecurityStampAsync(user);
        return Result.Ok();
    }

    public async Task<Result> UpdateAccountAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Fail(Errors.UserNotFound(userId));

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result.Fail(result.Errors.Select(e =>
                new Error(e.Description)
                    .WithMetadata("StatusCode", HttpStatusCode.BadRequest)));

        return Result.Ok();
    }

    public async Task<Result> DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Fail(Errors.UserNotFound(userId));

        await _userManager.DeleteAsync(user);
        return Result.Ok();
    }

    public async Task<Result> BanAccountAsync(BanAccountRequest request)
    {
       
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail(Errors.UserNotFound(request.Email));

        if (user.Banned)
            return Result.Fail(Errors.AlreadyBanned);

        user.Banned = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        return Result.Ok();
    }

    public async Task<Result> UnbanAccountAsync(BanAccountRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Fail(Errors.UserNotFound(request.Email));

        if (!user.Banned)
            return Result.Fail(Errors.NotBanned);

        user.Banned = false;
        user.LockoutEnd = null;

        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        return Result.Ok();
    }

    public async Task<UserResponse> GetUserByEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var mapped = _mapper.Map<UserResponse>(user);
        return mapped;
    }
}
