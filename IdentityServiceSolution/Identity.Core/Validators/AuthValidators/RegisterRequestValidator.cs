using FluentValidation;
using Identity.Core.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Validators.AuthValidators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(req => req.FirstName)
            .NotEmpty().WithMessage("نام نمیتواند خالی باشد")
            .MaximumLength(50).WithMessage("نام نمیتواند بیشتر از 50 کاراکتر باشد")
            .MinimumLength(3).WithMessage("نام نمیتواند کمتر از 3 کاراکتر باشد");

        RuleFor(req => req.LastName)
            .NotEmpty().WithMessage("نام خانوادگی نمیتواند خالی باشد")
            .MaximumLength(50).WithMessage("نام خانوادگی نمیتواند بیشتر از 50 کاراکتر باشد")
            .MinimumLength(3).WithMessage("نام خانوادگی نمیتواند کمتر از 3 کاراکتر باشد");

        RuleFor(req => req.Email)
            .NotEmpty().WithMessage("ایمیل نمیتواند خالی باشد")
            .EmailAddress().WithMessage("فرمت ایمیل وارد شده معتبر نیست")
            .Must(CustomRequestValidator.BeValidEmail).WithMessage("ایمیل وارد شده متعلق به دامنه معتبری نیست");

        RuleFor(req => req.PhoneNumber)
            .NotEmpty().WithMessage("شماره تلفن نمیتواند خالی باشد")
            .Length(11).WithMessage("شماره تلفن باید 11 کاراکتر باشد")
            .Matches("09[0-9]{9}").WithMessage("شماره تلفن معتبر نیست");

        RuleFor(req => req.Password)
            .NotEmpty().WithMessage("رمز عبور نمیتواند خالی باشد");

        RuleFor(req => req.ConfirmPassword)
            .NotEmpty().WithMessage("تکرار رمز عبور نمیتواند خالی باشد")
            .Equal(req => req.Password).WithMessage("رمز عبور و تکرار آن مطابقت ندارند");

    }
}
