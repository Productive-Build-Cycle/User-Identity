using FluentValidation;
using Identity.Core.Dtos.Auth;

namespace Identity.Core.Validators.AuthValidators;

public class LoginRequestValidator : AbstractValidator<LoginRequest> 
{
    public LoginRequestValidator()
    {
        RuleFor(req => req.Email)
            .NotEmpty().WithMessage("وارد کردن ایمیل الزامی است")
            .EmailAddress().WithMessage("ایمیل وارد شده معتبر نمی‌باشد");

        RuleFor(req => req.Password)
            .NotEmpty().WithMessage("وارد کردن رمز عبور الزامی است");
    }
}
