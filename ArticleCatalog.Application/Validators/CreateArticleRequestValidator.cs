using ArticleCatalog.Application.DTOs;
using FluentValidation;

namespace ArticleCatalog.Application.Validators;

/// <summary>
/// Валидатор для CreateArticleRequest
/// </summary>
public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(256)
            .WithMessage("Title too long (maximum 256 characters)");

        RuleFor(x => x.Tags)
            .NotNull()
            .WithMessage("Tags are required")
            .Must(tags => tags.Count <= 256)
            .WithMessage("Too many tags (maximum 256 tags)")
            .Must(tags => tags.Distinct(StringComparer.OrdinalIgnoreCase).Count() == tags.Count)
            .WithMessage("Duplicate tags are not allowed");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .WithMessage("Tag name cannot be empty");
    }
}

