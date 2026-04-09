using FinVault.IdentityService.Application.Commands.LoginUser;
using FluentValidation;

namespace FinVault.IdentityService.Application.Validators;

// Runs automatically before LoginUserCommandHandler
// Checks that email and password are not empty
// before we even touch the database
public class LoginUserValidator
    : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        // Email must not be empty and must look like an email
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");

        // Password must not be empty
        // We don't check length here — that's only on register
        // On login we just check if something was typed
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}