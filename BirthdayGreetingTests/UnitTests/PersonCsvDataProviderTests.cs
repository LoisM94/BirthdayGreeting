using BirthdayGreeting.Application.Providers;
using BirthdayGreeting.Application.Services;
using CsvHelper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BirthdayGreeting.Tests.UnitTests;

public class PersonCsvDataProviderTests : IDisposable

{
    private readonly string _testFilePath = "test.csv";
    private readonly Mock<ILogger<PersonCsvDataProvider>> _loggerMock = new();

    [Fact]
    public void GetPeople_ShouldReturnEmpty_WhenFileDoesNotExist()
    {
        // Act
        var provider = new PersonCsvDataProvider("nonexistent.csv", _loggerMock.Object);

        // Assert
        var result = provider.GetPeople();

        result.Should().BeEmpty();
        result.Should().BeEmpty();
        _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString() == "The file does not exist."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetPeople_ShouldReturnPeople_WhenCsvIsValid()
    {
        var csvData = "first_name,last_name,email,date_of_birth\nJohn,Doe,john@email.com,1990/01/20\nJane,Doe,jane@email.com,2000/01/20";
        File.WriteAllText(_testFilePath, csvData);

        var provider = new PersonCsvDataProvider(_testFilePath, _loggerMock.Object);
        var result = provider.GetPeople();

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new Person { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1990, 01, 20), Email = "john@email.com" });
        result.Should().ContainEquivalentOf(new Person { FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(2000, 01, 20), Email = "jane@email.com" });
    }

    [Fact]
    public void GetPeople_ShouldReturnEmpty_OnCsvHelperException()
    {
        // Arrange
        var malformedCsvData = "first_name,last_name\nJohn Doe"; // Missing last name
        File.WriteAllText(_testFilePath, malformedCsvData);

        // Act
        var provider = new PersonCsvDataProvider(_testFilePath, _loggerMock.Object);

        // Assert
        var result = provider.GetPeople();

        result.Should().BeEmpty();
        _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) =>
                    @object.ToString() == "Error reading CSV file."),
                It.IsAny<CsvHelperException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}