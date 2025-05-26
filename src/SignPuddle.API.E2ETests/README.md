# End-to-End Tests for SignPuddle API

This project contains end-to-end tests for the SignPuddle API. The tests are designed to verify the functionality of various API endpoints across different controllers.

## Project Structure

- **Controllers**: Contains test classes for each controller in the API.
  - `SignControllerTests.cs`: Tests for the SignController.
  - `FormatControllerTests.cs`: Tests for the FormatController.
  - `RenderControllerTests.cs`: Tests for the RenderController.
  - `UserControllerTests.cs`: Tests for the UserController.

- **Fixtures**: Contains setup classes for the test environment.
  - `TestWebApplicationFactory.cs`: Configures the test web application.

- **Helpers**: Contains utility classes and methods for testing.
  - `TestDataBuilder.cs`: Builds mock data for tests.
  - `HttpClientExtensions.cs`: Provides extension methods for HttpClient.

- **Configuration**:
  - `appsettings.Test.json`: Configuration settings for the testing environment.

## Running the Tests

To run the end-to-end tests, follow these steps:

1. Ensure you have the .NET SDK installed on your machine.
2. Open a terminal and navigate to the `SignPuddle.API.E2ETests` directory.
3. Run the following command to execute the tests:

   ```
   dotnet test
   ```

## Additional Information

- The tests are built using xUnit, a popular testing framework for .NET applications.
- Ensure that the API is running and accessible during the tests, or configure the test project to start the API automatically.
- Review the individual test classes for specific test cases and expected outcomes.