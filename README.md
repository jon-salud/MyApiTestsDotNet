# Marvel API Automation Suite

This project is an API automation suite built with Playwright, NUnit, and SpecFlow in .NET to test the Marvel Comics API.

## Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for Playwright browser installation, optional)
- VS Code with C# and SpecFlow extensions

## Setup

1. **Clone the Repository**:

   ```bash
   git clone <repo-url>
   cd MyApiTests
   ```

2. **Restore Dependencies**:

    ```bash
    dotnet restore
    ```

3. **Configure API Keys**:

    - Copy Config/appsettings.example.json to Config/appsettings.json.
    - Replace your-public-key-here and your-private-key-here with your Marvel API keys from developer.marvel.com.

4. **Build the Project**:

    ```bash
    dotnet build
    ```

## Running Tests

- Execute all tests:

    ```bash
    dotnet test
    ```

- Run with verbose output:

    ```bash
    dotnet test --logger "console;verbosity=detailed"
    ```

## Project Structure

- Features/: Gherkin feature files defining test scenarios.
- Steps/: Step definitions mapping Gherkin steps to C# code.
- Config/: Configuration files (e.g., `appsettings.json` for API keys).
- TestResults/: Test output (ignored by Git).
- .gitignore: Excludes sensitive and build files.

```bash
MyApiTests/
├── Features/             # Gherkin feature files
│   └── ApiTests.feature
├── Steps/                # Step definitions
│   └── ApiSteps.cs
├── Config/               # Configuration files
│   ├── appsettings.json  # Ignored by .gitignore
│   └── appsettings.example.json
├── TestResults/          # Test output (auto-generated, ignored)
├── bin/                  # Build output (ignored)
├── obj/                  # Build intermediates (ignored)
├── MyApiTests.csproj
├── .gitignore
└── README.md             # Project documentation (added below)
```

## Current Tests

- Scenario: Successfully retrieve comics with valid authentication
    -- Tests the `/comics` endpoint with Marvel API authentication.

## Contributing

- Add new feature files in `Features/` and step definitions in `Steps/`.
- Follow BDD practices with clear Given/When/Then steps.
- Submit pull requests with updated tests.

## Troubleshooting

- 404 Errors: Check BaseUrl in appsettings.json ends with /.
- 401 Errors: Verify API keys and hash generation.
- Binding Issues: Run dotnet clean and dotnet build.
