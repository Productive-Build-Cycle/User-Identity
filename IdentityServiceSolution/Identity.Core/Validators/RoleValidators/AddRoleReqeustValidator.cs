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

        RuleFor(req => req.Description)
            .NotEmpty().WithMessage("توضیحات نقش نمیتواند خالی باشد")
            .MaximumLength(200).WithMessage("توضیحات نقش نمیتواند بیشتر از 200 کاراکتر باشد")
            .MinimumLength(3).WithMessage("توضیحات نقش نمیتواند کمتر از 3 کاراکتر باشد");

    }
}
