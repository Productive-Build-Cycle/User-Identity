using FluentValidation;
using Identity.Core.Dtos.Auth;

namespace Identity.Core.Validators.AuthValidators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(req => req.Password)
            .NotEmpty().WithMessage("وارد کردن رمز عبور جدید الزامی است");

        RuleFor(req => req.ConfirmPassword)
            .NotEmpty().WithMessage("وارد کردن تکرار رمز عبور جدید الزامی است")
            .Equal(req => req.Password).WithMessage("رمز عبور جدید و تکرار آن مطابقت ندارند");
    }
}
