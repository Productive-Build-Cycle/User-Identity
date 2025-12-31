using AutoMapper;
using Identity.Core.Domain.Entities;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;
using Identity.Core.Exceptions;
using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Text;
using Identity.Core.Dtos.Roles;

namespace Identity.Core.Services;

public class UserrService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService, 
    IConfiguration conf,
    IMapper mapper, 
    IEmailService emailService, 
    IRolesService roleService) : IUserService
{
    
    public async Task<RergisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            var error = new IdentityTranslatedErrors().DuplicateEmail(request.Email);
            throw new InvalidOperationException(error.Description);
        }

        var user = mapper.Map<ApplicationUser>(request);

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" | ", result.Errors.Select(e => e.Description)));

        await roleService.AddUserToRoleAsync(new AssignRoleToUserRequest(user.Id, "User"));

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var backLink = $"{conf["BaseUrl"]}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var body = await emailService.TurnHtmlToString("EmailConfirmation.html",
                new Dictionary<string, string>
                {
                    ["VerificationLink"] = backLink,
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                });

        await emailService.SendEmailAsync(new EmailOptions(user.Email, "تایید حساب کاربری", body));

        return new RergisterResponse(IsEmailConfirmed: false, Email: user.Email,
            Message: "ثبت نام با موفقیت انجام شد. لطفاً ایمیل خود را برای تایید حساب کاربری بررسی کنید.");
    }

    public async Task<AuthResponse> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("شناسه کاربر یا توکن ارسال نشده است.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException("کاربری با این شناسه وجود ندارد.");

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            throw new InvalidOperationException("توکن نامعتبر، منقضی یا قبلاً استفاده شده است.");

        await signInManager.SignInAsync(user, isPersistent: false);

        var authResponse = await tokenService.GenerateToken(user);

        return authResponse;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

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

        if (!user.EmailConfirmed)
            throw new InvalidOperationException("لطفا حساب کاربری خود را تایید حنید");

        if (user.Banned)
            throw new InvalidOperationException("اکانت شما مسدود شده است. برای اطلاعات بیشتر با پشتیبانی تماس بگیرید.");

        var result = await signInManager.CheckPasswordSignInAsync(
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
                await userManager.UpdateAsync(user);

                throw new InvalidOperationException(
                    $"رمز عبور نامعتبر. دسترسی شما به مدت {lockoutMinutes} دقیقه محدود شد");
            }

            throw new UnauthorizedAccessException("رمز عبور یا نام کاربری نامعتبر است");
        }

        if (user.LockoutMultiplier > 1)
        {
            user.LockoutMultiplier = 1;
            await userManager.UpdateAsync(user);
        }

        return await tokenService.GenerateToken(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await userManager.UpdateSecurityStampAsync(user); // نکته: این کد واسه منقضی کدرن توکن های کاربر نوشته شده
        }

        await signInManager.SignOutAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(request.UserId).Description);

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" | ", result.Errors.Select(e => e.Description)));

        await userManager.UpdateSecurityStampAsync(user);
    }

    public async Task UpdateAccountAsync(string userId, UpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(userId).Description);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join(" | ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeleteAccountAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(userId).Description);

        await userManager.DeleteAsync(user);
    }

    public async Task BanAccountAsync(string targetUserId, string actorUserId)
    {
        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
            throw new UnauthorizedAccessException("کاربر اقدام‌کننده یافت نشد");

        var claims = await userManager.GetClaimsAsync(actor);
        if (!claims.Any(c => c.Type == "user.ban"))
            throw new UnauthorizedAccessException("شما اجازه بستن حساب کاربری را ندارید");

        var user = await userManager.FindByIdAsync(targetUserId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(targetUserId).Description);

        if (user.Banned)
            throw new InvalidOperationException("کاربر مورد نظر درحال حاضر مسدود شده است");

        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.Banned = true;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);
    }

    public async Task UnbanAccountAsync(string targetUserId, string actorUserId)
    {
        var actor = await userManager.FindByIdAsync(actorUserId);
        if (actor is null)
            throw new UnauthorizedAccessException("کاربر اقدام‌کننده یافت نشد");

        var claims = await userManager.GetClaimsAsync(actor);
        if (!claims.Any(c => c.Type == "user.ban"))
            throw new UnauthorizedAccessException("شما اجازه باز کردن حساب کاربری را ندارید");

        var user = await userManager.FindByIdAsync(targetUserId);
        if (user is null)
            throw new InvalidOperationException(
                new IdentityTranslatedErrors().UserNotFound(targetUserId).Description);

        if (!user.Banned)
            throw new InvalidOperationException("کاربر مورد نظر درحال حاضر مسدود نشده");

        user.LockoutEnd = null;
        user.Banned = false;
        await userManager.UpdateAsync(user);
    }
}
