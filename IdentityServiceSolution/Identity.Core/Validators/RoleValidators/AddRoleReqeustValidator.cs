using FluentValidation;
using Identity.Core.Dtos.Roles;

namespace Identity.Core.Validators.RoleValidators;

public class AddRoleReqeustValidator : AbstractValidator<AddRoleRequest>
{
    public AddRoleReqeustValidator()
    {
        RuleFor(req => req.Name)
            .NotEmpty().WithMessage("نام نقش نمیتواند خالی باشد")
            .MaximumLength(50).WithMessage("نام نقش نمیتواند بیشتر از 50 کاراکتر باشد")
            .MinimumLength(3).WithMessage("نام نقش نمیتواند کمتر از 3 کاراکتر باشد");
    }
}
