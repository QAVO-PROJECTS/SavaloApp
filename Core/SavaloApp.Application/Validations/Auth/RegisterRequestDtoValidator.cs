using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;
using SavaloApp.Application.Dtos.UserAuth;


namespace SavaloApp.Application.Validations.Auth
{
   
    public class RegisterRequestDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("EMAIL_REQUIRED")
                .EmailAddress().WithMessage("EMAIL_INVALID")
                .MaximumLength(256).WithMessage("EMAIL_TOO_LONG");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("PASSWORD_REQUIRED")
                .MinimumLength(8).WithMessage("PASSWORD_TOO_SHORT")
                .Matches("[A-Z]").WithMessage("PASSWORD_NEEDS_UPPERCASE")
                .Matches("[a-z]").WithMessage("PASSWORD_NEEDS_LOWERCASE")
                .Matches("[0-9]").WithMessage("PASSWORD_NEEDS_NUMBER")
                .Matches("[^a-zA-Z0-9]").WithMessage("PASSWORD_NEEDS_SPECIAL_CHAR");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FULLNAME_REQUIRED")
                .MaximumLength(100).WithMessage("FIRSTNAME_TOO_LONG");


            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("PHONE_REQUIRED")
                .Matches(@"^\+994[0-9]{9}$").WithMessage("PHONE_INVALID_FORMAT");
        }
    }
}