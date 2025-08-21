using ContentService.Api.Contracts;
using FluentValidation;

namespace ContentService.Api.Validation;

public sealed class ContentCreateRequestValidator : AbstractValidator<ContentCreateRequest>
{
    public ContentCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 200);
        RuleFor(x => x.Body).NotEmpty().MinimumLength(10);
        RuleFor(x => x.AuthorId).NotEmpty();
    }
}

public sealed class ContentUpdateRequestValidator : AbstractValidator<ContentUpdateRequest>
{
    private static readonly string[] Allowed = ["Draft", "Published", "Archived"];

    public ContentUpdateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(3, 200);
        RuleFor(x => x.Body).NotEmpty().MinimumLength(10);
        RuleFor(x => x.Status)
          .NotEmpty()
          .Must(s => Allowed.Contains(s))
          .WithMessage("Status must be one of: Draft, Published, Archived.");
    }
}