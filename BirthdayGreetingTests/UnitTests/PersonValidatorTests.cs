using FluentValidation.TestHelper;
using BirthdayGreeting.Application.Validators;
using BirthdayGreeting.Application.Services;

namespace BirthdayGreeting.Tests.UnitTests;

public class PersonValidatorTests
{
    private readonly PersonValidator _validator;

    public PersonValidatorTests()
    {
        _validator = new PersonValidator();
    }

    [Fact]
    public void When_FirstNameIsEmpty_Then_ShouldReturnErrorMessage()
    {
        // Arrange
        var person = new Person { FirstName = "", Email = "valid@example.com", DateOfBirth = DateTime.Today };

        // Act
        var result = _validator.TestValidate(person);

        // Assert
        result.ShouldHaveValidationErrorFor(f => f.FirstName)
            .WithErrorMessage("First Name is required.");
    }

    [Fact]
    public void When_EmailIsEmpty_Then_ShouldReturnErrorMessage()
    {
        // Arrange
        var person = new Person { FirstName = "John", Email = "", DateOfBirth = DateTime.Today };

        // Act
        var result = _validator.TestValidate(person);

        // Assert
        result.ShouldHaveValidationErrorFor(f => f.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void When_EmailIsInvalid_Then_ShouldReturnErrorMessage()
    {
        // Arrange
        var person = new Person { FirstName = "John", Email = "invalid-email", DateOfBirth = DateTime.Today };

        // Act
        var result = _validator.TestValidate(person);

        // Assert
        result.ShouldHaveValidationErrorFor(f => f.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Fact]
    public void When_PayloadIsValid_Then_NoValidationErrors()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John Doe",
            Email = "john.doe@example.com",
            DateOfBirth = new DateTime(1990, 5, 10)
        };

        // Act
        var result = _validator.TestValidate(person);

        // Assert
        result.ShouldNotHaveValidationErrorFor(f => f.FirstName);
        result.ShouldNotHaveValidationErrorFor(f => f.Email);
        result.ShouldNotHaveValidationErrorFor(f => f.DateOfBirth);
    }
}