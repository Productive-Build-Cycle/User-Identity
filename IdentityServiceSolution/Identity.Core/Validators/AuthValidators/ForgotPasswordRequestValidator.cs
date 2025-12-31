using FluentValidation;
using Identity.Core.Dtos.Auth;

namespace Identity.Core.Validators.AuthValidators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(req => req.Email)
            .NotEmpty().WithMessage("وارد کردن ایمیل الزامی است")
            .EmailAddress().WithMessage("ایمیل وارد شده نامعتبر است")
            .Must(CustomRequestValidator.BeValidEmail).WithMessage("دامنه ایمیل وارد شده معتبر نیست");

        RuleFor(req => req.CallbackUri)
            .NotEmpty().WithMessage("وارد کردن آدرس الزامی است");
    }
}
