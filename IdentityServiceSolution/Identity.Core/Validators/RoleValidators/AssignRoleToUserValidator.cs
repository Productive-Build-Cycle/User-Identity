using FluentValidation;
using Identity.Core.Dtos.Roles;

namespace Identity.Core.Validators.RoleValidators;

public class AssignRoleToUserValidator : AbstractValidator<AssignRoleToUserRequest>
{
    public AssignRoleToUserValidator()
    {
        RuleFor(req => req.UserEmail)
            .NotEmpty().WithMessage("ایمیل نمیتواند خالی باشد")
            .EmailAddress().WithMessage("فرمت ایمیل وارد شده معتبر نیست")
            .Must(CustomRequestValidator.BeValidEmail).WithMessage("ایمیل وارد شده متعلق به دامنه معتبری نیست");

        RuleFor(req => req.RoleName)
            .NotEmpty().WithMessage("نام نقش نمیتواند خالی باشد")
            .MaximumLength(50).WithMessage("نام نقش نمیتواند بیشتر از 50 کاراکتر باشد")
            .MinimumLength(3).WithMessage("نام نقش نمیتواند کمتر از 3 کاراکتر باشد");
    }
}
