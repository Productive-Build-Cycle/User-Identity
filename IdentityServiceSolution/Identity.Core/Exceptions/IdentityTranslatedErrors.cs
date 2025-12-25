using Microsoft.AspNetCore.Identity;

namespace Identity.Core.Exceptions;

public class IdentityTranslatedErrors : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "خطای ناشناخته‌ای رخ داده است." };

    public override IdentityError ConcurrencyFailure() =>
        new() { Code = nameof(ConcurrencyFailure), Description = "خطا در هماهنگی داده‌ها. لطفاً دوباره تلاش کنید." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "رمز عبور اشتباه است." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "توکن نامعتبر است یا منقضی شده است." };

    public override IdentityError RecoveryCodeRedemptionFailed() =>
        new() { Code = nameof(RecoveryCodeRedemptionFailed), Description = "کد بازیابی معتبر نیست." };

    public override IdentityError LoginAlreadyAssociated() =>
        new()
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = "این حساب کاربری قبلاً با کاربر دیگری مرتبط شده است."
        };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = $"نام کاربری '{userName}' معتبر نیست." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = $"ایمیل '{email}' معتبر نیست." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"نام کاربری '{userName}' قبلاً ثبت شده است." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"ایمیل '{email}' قبلاً ثبت شده است." };

    public override IdentityError InvalidRoleName(string? role) =>
        new() { Code = nameof(InvalidRoleName), Description = $"نام نقش '{role}' معتبر نیست." };

    public override IdentityError DuplicateRoleName(string role) =>
        new() { Code = nameof(DuplicateRoleName), Description = $"نقش '{role}' قبلاً وجود دارد." };

    public override IdentityError UserAlreadyHasPassword() =>
        new() { Code = nameof(UserAlreadyHasPassword), Description = "کاربر از قبل رمز عبور دارد." };

    public override IdentityError UserLockoutNotEnabled() =>
        new() { Code = nameof(UserLockoutNotEnabled), Description = "قفل شدن حساب برای این کاربر فعال نیست." };

    public override IdentityError UserAlreadyInRole(string role) =>
        new() { Code = nameof(UserAlreadyInRole), Description = $"کاربر از قبل عضو نقش '{role}' است." };

    public override IdentityError UserNotInRole(string role) =>
        new() { Code = nameof(UserNotInRole), Description = $"کاربر عضو نقش '{role}' نیست." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"رمز عبور باید حداقل {length} کاراکتر باشد." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        new()
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = $"رمز عبور باید حداقل شامل {uniqueChars} کاراکتر متفاوت باشد."
        };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new()
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "رمز عبور باید شامل حداقل یک کاراکتر غیرحروفی (مثل @ یا #) باشد."
        };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "رمز عبور باید شامل حداقل یک رقم باشد." };

    public override IdentityError PasswordRequiresLower() =>
        new()
        {
            Code = nameof(PasswordRequiresLower),
            Description = "رمز عبور باید شامل حداقل یک حرف کوچک انگلیسی باشد."
        };

    public override IdentityError PasswordRequiresUpper() =>
        new()
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "رمز عبور باید شامل حداقل یک حرف بزرگ انگلیسی باشد."
        };
}