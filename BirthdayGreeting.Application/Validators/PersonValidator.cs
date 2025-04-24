using BirthdayGreeting.Application.Services;
using FluentValidation;

namespace BirthdayGreeting.Application.Validators;

public class PersonValidator : AbstractValidator<Person>
{
    public PersonValidator()
    {
        RuleFor(person => person.FirstName)
            .NotEmpty()
            .WithMessage("First Name is required.");

        RuleFor(person => person.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");
    }
}