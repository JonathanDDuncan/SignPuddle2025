# SignPuddle Testing Strategy

## Overview

The SignPuddle application uses a comprehensive testing approach with **no mocking libraries** (such as Moq) to ensure real-world reliability and maintainability.

## Testing Architecture

### 1. API Unit Tests (`src\SignPuddle.API.Tests`)

**Purpose**: Test business logic, services, and utilities in isolation without mocking dependencies.

**Key Principles**:
- ✅ **No Moq or other mocking libraries**
- ✅ Test real implementations with lightweight dependencies
- ✅ Use actual file I/O for testing file-based services
- ✅ Focus on business logic verification
- ✅ Fast execution for TDD workflows

**What to Test**:
- Service layer logic (`SpmlImportService`, etc.)
- Data transformations and mappings
- Validation logic
- Utility functions
- Model behavior and calculations

**Example Structure**:
```csharp
public class SpmlImportServiceTests
{
    private readonly SpmlImportService _spmlImportService;
    private readonly string _testDataPath;

    public SpmlImportServiceTests()
    {
        // Use real service instance - no mocking
        _spmlImportService = new SpmlImportService();
        _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sgn4-small.spml");
    }

    [Fact]
    public async Task ParseSpmlFromFileAsync_WithValidFile_ShouldReturnSpmlDocument()
    {
        // Test with real file I/O
        var result = await _spmlImportService.ParseSpmlFromFileAsync(_testDataPath);
        
        Assert.NotNull(result);
        Assert.Equal("sgn", result.Type);
    }
}
```

### 2. End-to-End Controller Tests (`src\SignPuddle.API.E2ETests`)

**Purpose**: Test complete API endpoints through HTTP requests to ensure full integration works correctly.

**Key Principles**:
- ✅ **No mocking - test actual controller endpoints**
- ✅ Use `TestWebApplicationFactory` for in-memory test server
- ✅ Test HTTP request/response cycles
- ✅ Verify complete request processing pipeline
- ✅ Test authentication and authorization flows

**What to Test**:
- Controller action methods via HTTP calls
- Request/response serialization
- HTTP status codes and headers
- Error handling and validation responses
- Authentication and authorization

**Example Structure**:
```csharp
public class UserControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 999;

        // Act - Real HTTP call to controller
        var response = await _client.GetAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

### 3. Integration Tests (`src\SignPuddle.API.Tests\Integration`)

**Purpose**: Test multiple components working together with real dependencies.

**Key Principles**:
- ✅ **No mocking - use real component integration**
- ✅ Test data flows between services
- ✅ Verify end-to-end business scenarios
- ✅ Use real file systems, but isolated test data

**What to Test**:
- Multi-service workflows
- Data persistence and retrieval
- File processing pipelines
- Complex business operations

## Testing Guidelines

### ❌ What NOT to Do

1. **Don't use Moq or other mocking frameworks**
   - Mocks can hide integration issues
   - Tests become brittle and coupled to implementation
   - Real bugs may not be caught

2. **Don't test controllers in unit tests**
   - Controllers should be tested via HTTP calls in E2E tests
   - HTTP-specific logic requires full request pipeline

3. **Don't mock file systems unnecessarily**
   - Use real files with controlled test data
   - Provides more realistic testing scenarios

### ✅ What TO Do

1. **Use real implementations in unit tests**
   - Test actual service logic with lightweight dependencies
   - Use in-memory databases for data-dependent tests when needed

2. **Create focused test data**
   - Maintain small, controlled test files
   - Design test data to cover edge cases

3. **Test happy path and error scenarios**
   - Verify both successful operations and failure modes
   - Test validation and error handling

4. **Use descriptive test names**
   - Follow pattern: `MethodName_Scenario_ExpectedResult`

## Project Structure

```
src/
├── SignPuddle.API.Tests/           # Unit tests for services and business logic
│   ├── Services/                   # Service layer tests
│   ├── Integration/                # Integration tests
│   ├── Helpers/                    # Test utilities
│   └── Data/                       # Test data files
│
└── SignPuddle.API.E2ETests/        # Controller endpoint tests
    ├── Controllers/                # HTTP-based controller tests
    ├── Fixtures/                   # Test setup and configuration
    └── Helpers/                    # E2E test utilities
```

## Test Data Management

### Unit Test Data
- Store in `src\SignPuddle.API.Tests\Data\`
- Use small, focused datasets
- Include edge cases and error scenarios

### E2E Test Data
- Create through API calls when possible
- Use test database or in-memory providers
- Clean up after tests to maintain isolation

## Running Tests

### Unit Tests
```bash
cd src\SignPuddle.API.Tests
dotnet test
```

### E2E Tests
```bash
cd src\SignPuddle.API.E2ETests
dotnet test
```

### All Tests
```bash
cd src\SignPuddle.API
dotnet test SignPuddle.API.sln
```

## Continuous Integration

The testing strategy supports CI/CD pipelines:
- Fast unit tests run on every commit
- E2E tests run on pull requests and releases
- No external dependencies required for mocking frameworks
- Reliable, deterministic test results

## Benefits of This Approach

1. **Real-world reliability**: Tests exercise actual code paths
2. **Simpler maintenance**: No mock setup/verification code
3. **Better refactoring support**: Tests don't break when implementation details change
4. **Easier debugging**: Test failures point to real issues
5. **Documentation value**: Tests show how components actually work together

## Migration from Moq

If migrating existing tests that use Moq:

1. **Remove Moq dependencies** from project files
2. **Replace mocked services** with real instances
3. **Provide real test data** instead of stubbed responses
4. **Move controller tests** to E2E test project
5. **Focus unit tests** on business logic verification

This approach ensures robust, maintainable tests that provide confidence in the system's behavior.
