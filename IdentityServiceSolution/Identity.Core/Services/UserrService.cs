using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;
using Identity.Core.Exceptions;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Services;

public class UserrService(UserManager<ApplicationUser> userManagerr,
    SignInManager<ApplicationUser> signInManager,    ITokenService _tokenService,
    IMapper mapper) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManagerr;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ITokenService _tokenServiceInstance = _tokenService;
    private readonly IMapper _mapper = mapper;


    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            var error = new IdentityTranslatedErrors().DuplicateEmail(request.Email);
            throw new InvalidOperationException(error.Description);
        }

        var user = _mapper.Map<ApplicationUser>(request);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" | ", result.Errors.Select(e => e.Description)));

        return await _tokenServiceInstance.GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            throw new UnauthorizedAccessException("کاربری با این ایمیل وجود ندارد");

        if (user.LockoutEnabled &&
            user.LockoutEnd.HasValue &&
            user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            var remaining = user.LockoutEnd.Value - DateTimeOffset.UtcNow;
            throw new InvalidOperationException(
                $"دسترسی شما به اکانت به مدت {remaining.Minutes} دقیقه محدود شده است");
        }

        if(user.Banned)
            throw new InvalidOperationException("اکانت شما مسدود شده است. برای اطلاعات بیشتر با پشتیبانی تماس بگیرید.");

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                var lockoutMinutes = 5 * user.LockoutMultiplier;

                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes);
                user.LockoutMultiplier *= 2;
                await _userManager.UpdateAsync(user);

                throw new InvalidOperationException(
                    $"رمز عبور نامعتبر. دسترسی شما به مدت {lockoutMinutes} دقیقه محدود شد");
            }

            throw new UnauthorizedAccessException("رمز عبور یا نام کاربری نامعتبر است");
        }

        if (user.LockoutMultiplier > 1)
        {
            user.LockoutMultiplier = 1;
            await _userManager.UpdateAsync(user);
        }

        return await _tokenServiceInstance.GenerateToken(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await _userManager.UpdateSecurityStampAsync(user); // نکته: این کد واسه منقضی کدرن توکن های کاربر نوشته شده
        }

        await _signInManager.SignOutAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(request.UserId).Description);

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" | ", result.Errors.Select(e => e.Description)));

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task UpdateAccountAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(userId).Description);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" | ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(userId).Description);

        await _userManager.DeleteAsync(user);
    }

    public async Task BanAccountAsync(string targetUserId, string actorUserId)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId);
        if (actor is null)
            throw new UnauthorizedAccessException("کاربر اقدام‌کننده یافت نشد");

        var claims = await _userManager.GetClaimsAsync(actor);
        if (!claims.Any(c => c.Type == "user.ban"))
            throw new UnauthorizedAccessException("شما اجازه بستن حساب کاربری را ندارید");

        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(targetUserId).Description);

        if (user.Banned)
            throw new InvalidOperationException("کاربر مورد نظر درحال حاضر مسدود شده است");

        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.Banned = true;
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task UnbanAccountAsync(string targetUserId, string actorUserId)
    {
        var actor = await _userManager.FindByIdAsync(actorUserId);
        if (actor is null)
            throw new UnauthorizedAccessException("کاربر اقدام‌کننده یافت نشد");

        var claims = await _userManager.GetClaimsAsync(actor);
        if (!claims.Any(c => c.Type == "user.ban"))
            throw new UnauthorizedAccessException("شما اجازه باز کردن حساب کاربری را ندارید");

        var user = await _userManager.FindByIdAsync(targetUserId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(targetUserId).Description);

        if (!user.Banned)
            throw new InvalidOperationException("کاربر مورد نظر درحال حاضر مسدود نشده");

        user.LockoutEnd = null;
        user.Banned = false;
        await _userManager.UpdateAsync(user);
    }
}
