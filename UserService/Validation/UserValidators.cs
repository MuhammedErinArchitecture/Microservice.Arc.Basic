using FluentValidation;

namespace UserService.Api.Validation;

public sealed class UserCreateRequestValidator : AbstractValidator<Contracts.UserCreateRequest>
{
    public UserCreateRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().Length(2, 100);
        RuleFor(x => x.LastName).NotEmpty().Length(2, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public sealed class UserUpdateRequestValidator : AbstractValidator<Contracts.UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Role).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
    }
}
