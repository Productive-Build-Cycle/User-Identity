using FluentValidation;
using Identity.Core.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Validators.AuthValidators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(req => req.Email)
            .NotEmpty().WithMessage("ایمیل نمیتواند خالی باشد")
            .EmailAddress().WithMessage("فرمت ایمیل وارد شده معتبر نیست")
            .Must(CustomRequestValidator.BeValidEmail).WithMessage("ایمیل وارد شده متعلق به دامنه معتبری نیست");

        RuleFor(req => req.CurrentPassword)
            .NotEmpty().WithMessage("وارد کردن رمز عبور فعلی الزامی است");

        RuleFor(req => req.NewPassword)
            .NotEmpty().WithMessage("وارد کردن رمز عبور جدید الزامی است");

        RuleFor(req => req.ConfirmNewPassword)
            .NotEmpty().WithMessage("وارد کردن تکرار رمز عبور جدید الزامی است")
            .Equal(req => req.NewPassword).WithMessage("تکرار رمز عبور جدید با رمز عبور جدید مطابقت ندارد");
    }
}
