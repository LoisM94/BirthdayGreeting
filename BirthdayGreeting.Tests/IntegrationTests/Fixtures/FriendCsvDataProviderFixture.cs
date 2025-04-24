using BirthdayGreeting.Application.Providers;
using Microsoft.Extensions.Logging;
using Moq;

namespace BirthdayGreeting.Tests.IntegrationTests.Fixtures;

public class FriendCsvDataProviderFixture : IDisposable
{
    public IPersonDataProvider FriendDataProvider { get; }
    public Mock<ILogger<PersonCsvDataProvider>> LoggerMock { get; }
    public string TestCsvFilePath { get; }

    public FriendCsvDataProviderFixture()
    {
        // Create a sample CSV file for testing
        TestCsvFilePath = "test_friends.csv";
        File.WriteAllText(TestCsvFilePath, SampleCsvContent);

        LoggerMock = new Mock<ILogger<PersonCsvDataProvider>>();


        FriendDataProvider = new PersonCsvDataProvider(TestCsvFilePath, LoggerMock.Object);
    }

    private string SampleCsvContent =>
        "first_name,last_name,email,date_of_birth\n" +
        "John,Doe,johndoe@example.com,1990-04-22\n" +
        "Jane,Doe,janedoe@example.com,1985-05-10\n";

    public void Dispose()
    {
        if (File.Exists(TestCsvFilePath))
        {
            File.Delete(TestCsvFilePath);
        }
    }
}