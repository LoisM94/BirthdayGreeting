using System.Globalization;
using BirthdayGreeting.Application.Services;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace BirthdayGreeting.Application.Providers;

/*
  consider adding another implementation of IPersonDataProvider that fetches data from a database or an API.
        * This increases the security and performance of the application.  
        * This way we don't have potential sensitive data like name and email in plain text.
        * Testability is also improved.  We can mock the database or API and test the application without relying on a CSV file.
    */
public class PersonCsvDataProvider : IPersonDataProvider
{
    private readonly string _filePath;
    private readonly ILogger<PersonCsvDataProvider> _logger;

    public PersonCsvDataProvider(string filePath, ILogger<PersonCsvDataProvider> logger)
    {
        _filePath = filePath;
        _logger = logger;
    }

    public IEnumerable<Person> GetPeople()
    {
        // consider injecting a caching service like redis.  Instead of reading the csv every time, you can read it once and store it in cache.  This will improve performance and reduce the number of file reads.
        if (!File.Exists(_filePath))
        {
            _logger.LogError("The file does not exist.");
            return [];
        }

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = header => header.Header.Trim(),
            TrimOptions = TrimOptions.Trim,
            WhiteSpaceChars = [(char)32, (char)160]
        });
        
        List<Person> people;
        try
        {
            people = csv.GetRecords<Person>().ToList();
        }
        catch (CsvHelperException ex)
        {
            _logger.LogError(ex, "Error reading CSV file.");
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return [];
        }

        return people;
    }
}