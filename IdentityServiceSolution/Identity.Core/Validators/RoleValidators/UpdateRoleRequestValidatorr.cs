using FluentValidation;
using Identity.Core.Dtos.Roles;


namespace Identity.Core.Validators.RoleValidators;

public class UpdateRoleRequestValidatorr : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidatorr()
    {
        RuleFor(req => req.Id)
            .NotEmpty().WithMessage("شناسه نقش نمیتواند خالی باشد")
            .Must(id => id != Guid.Empty).WithMessage("شناسه نقش معتبر نیست");

        RuleFor(req => req.Name)
            .NotEmpty().WithMessage("نام نقش نمیتواند خالی باشد")
            .MaximumLength(50).WithMessage("نام نقش نمیتواند بیشتر از 50 کاراکتر باشد")
            .MinimumLength(3).WithMessage("نام نقش نمیتواند کمتر از 3 کاراکتر باشد");
    }
}
