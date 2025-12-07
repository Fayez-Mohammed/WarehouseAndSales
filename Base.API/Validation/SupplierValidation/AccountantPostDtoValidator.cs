using Base.API.DTOs;
using FluentValidation;

namespace BaseAPI.Validation.SupplierValidation;


public class AccountantPostDtoValidator : AbstractValidator<AccountantPostDto>
{
    public AccountantPostDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}