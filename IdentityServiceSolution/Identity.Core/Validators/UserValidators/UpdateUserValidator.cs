using FluentValidation;
using Identity.Core.DTOs.Users;

namespace Identity.Core.Validators.UserValidators;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(req => req.Id)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد")
            .Must(id => id != Guid.Empty).WithMessage("شناسه کاربر معتبر نیست"); ;

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
    }
}
