using FluentValidation;

namespace NovaStore.Application.Common;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(password => password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(8, 100).WithMessage("Password must be between 8 and 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^\da-zA-Z]").WithMessage("Password must contain at least one special character.");
    }
}
