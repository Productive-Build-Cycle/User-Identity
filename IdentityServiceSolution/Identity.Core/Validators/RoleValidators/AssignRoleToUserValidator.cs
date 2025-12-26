using FluentValidation;
using Identity.Core.Dtos.Roles;

namespace Identity.Core.Validators.RoleValidators;

public class AssignRoleToUserValidator : AbstractValidator<AssignRoleToUserRequest>
{
    public AssignRoleToUserValidator()
    {
        RuleFor(req => req.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد")
            .Must(id => id != Guid.Empty).WithMessage("شناسه کاربر معتبر نیست");

        RuleFor(req => req.RoleName)
            .NotEmpty().WithMessage("نام نقش نمیتواند خالی باشد")
            .MaximumLength(50).WithMessage("نام نقش نمیتواند بیشتر از 50 کاراکتر باشد")
            .MinimumLength(3).WithMessage("نام نقش نمیتواند کمتر از 3 کاراکتر باشد");
    }
}
