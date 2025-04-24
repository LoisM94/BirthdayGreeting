# Birthday Greeting Service

The Birthday Greeting Service is a .NET application designed to send personalised birthday emails to people. It reads data from a CSV file, validates the information, and sends emails using the SendGrid email service.

---

## Table of Contents
1. [Features](#features)
2. [Technologies](#technologies)
3. [Setup and Installation](#setup-and-installation)
4. [Configuration](#configuration)
5. [Testing](#testing)

---

## Features

- **Scheduled Birthday Greetings**:
  - Automatically sends birthday emails on a daily schedule at 8:00AM UTC via Azure Functions Timer Trigger.

- **CSV Data Support**:
  - Reads person details, including email addresses and birthdays, from a CSV file.

- **Validation**:
  - Ensures data integrity using FluentValidation.

- **Retry Mechanism**:
  - Implements Polly to handle transient failures when sending emails.

- **Extensible and Testable Design**:
  - Follows clean architecture principles to ensure modularity and testability.

---

## Technologies

- **C# .NET 8**
- **Azure Functions** (Timer Trigger)
- **SendGrid API** (for sending emails)
- **Polly** (for retry policies)
- **FluentValidation** (for input validation)
- **xUnit, Moq** (for unit and integration tests)
- **Application Insights** (for logging and monitoring)

---

## Setup and Installation

### Prerequisites
- .NET SDK 8.0 or later
- Azure Function Core Tools (if running locally)
- SendGrid API Key
- Access to an SMTP server or email credentials (via SendGrid)

### Steps to Run Locally
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/LoisM94/BirthdayGreeting.git
   cd birthday-greeting-service

### Testing
Unit/Integration tests are provided for core components:
 - Navigate to the test project directory.
 - Run the tests using the .NET CLI: `dotnet test`
