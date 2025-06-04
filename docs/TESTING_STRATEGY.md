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

### 4. Frontend Unit Tests (`src\signpuddle-web\__tests__`)

**Purpose**: Test Svelte components, stores, and utilities in isolation with Jest and Svelte Testing Library.

**Key Principles**:
- ✅ **No mocking of user interactions**
- ✅ Test component behavior through user events
- ✅ Use real Svelte store implementations
- ✅ Focus on component contract testing
- ✅ Fast execution for TDD workflows

**What to Test**:
- Component rendering and props
- User interactions (clicks, form inputs)
- Store state management
- Utility functions
- Client-side validation logic

**Example Structure**:
```javascript
import { render, fireEvent, screen } from '@testing-library/svelte';
import SignEditor from '../src/components/sign/SignEditor.svelte';
import { signStore } from '../src/stores/signStore.js';

describe('SignEditor', () => {
  beforeEach(() => {
    // Reset store to clean state
    signStore.reset();
  });

  test('renders with initial FSW value', () => {
    const initialFSW = 'M525x535S2e748483x510S10011501x466S2e704510x500S10019476x475';
    
    render(SignEditor, { props: { initialFSW } });
    
    expect(screen.getByRole('textbox', { name: /fsw/i })).toHaveValue(initialFSW);
  });

  test('updates sign when FSW input changes', async () => {
    render(SignEditor);
    
    const fswInput = screen.getByRole('textbox', { name: /fsw/i });
    await fireEvent.input(fswInput, { target: { value: 'M500x500S10000480x480' } });
    
    expect(signStore.getCurrentSign().fsw).toBe('M500x500S10000480x480');
  });
});
```

### 5. Frontend End-to-End Tests (`src\signpuddle-web\cypress`)

**Purpose**: Test complete user workflows through real browser interactions using Cypress.

**Key Principles**:
- ✅ **No mocking - test real user scenarios**
- ✅ Test cross-component integration
- ✅ Verify complete user journeys
- ✅ Test API integration points
- ✅ Visual regression testing capability

**What to Test**:
- User authentication flows
- Sign creation and editing workflows
- Dictionary browsing and searching
- Data persistence and retrieval
- Responsive design behavior
- Accessibility features

**Example Structure**:
```javascript
describe('Sign Maker Workflow', () => {
  beforeEach(() => {
    cy.visit('/sign-maker');
  });

  it('creates a new sign and saves it', () => {
    // Start with empty sign editor
    cy.get('[data-cy=sign-editor]').should('be.visible');
    
    // Add symbols to create a sign
    cy.get('[data-cy=symbol-palette]').within(() => {
      cy.get('[data-cy=symbol-S10000]').click();
      cy.get('[data-cy=symbol-S20000]').click();
    });
    
    // Verify FSW is generated
    cy.get('[data-cy=fsw-input]').should('contain.value', 'M');
    
    // Save the sign
    cy.get('[data-cy=save-sign]').click();
    cy.get('[data-cy=sign-name-input]').type('Test Sign');
    cy.get('[data-cy=confirm-save]').click();
    
    // Verify save success
    cy.get('[data-cy=success-message]').should('contain', 'Sign saved successfully');
  });

  it('loads existing sign for editing', () => {
    const existingFSW = 'M525x535S2e748483x510S10011501x466';
    
    cy.visit(`/sign-maker?fsw=${encodeURIComponent(existingFSW)}`);
    
    cy.get('[data-cy=fsw-input]').should('have.value', existingFSW);
    cy.get('[data-cy=sign-canvas]').should('contain.html', 'svg');
  });
});
```

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

4. **Don't mock user interactions in frontend tests**
   - Test real user behaviors (clicks, typing, navigation)
   - Use real browser events rather than simulated ones

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

5. **Test component behavior, not implementation**
   - Focus on user-facing behavior in frontend tests
   - Test what users see and interact with

6. **Use real browser environments for E2E tests**
   - Test in actual browsers with Cypress
   - Verify cross-browser compatibility when needed

## Project Structure

```
src/
├── SignPuddle.API.Tests/           # Unit tests for services and business logic
│   ├── Services/                   # Service layer tests
│   ├── Integration/                # Integration tests
│   ├── Helpers/                    # Test utilities
│   └── Data/                       # Test data files
│
├── SignPuddle.API.E2ETests/        # Controller endpoint tests
│   ├── Controllers/                # HTTP-based controller tests
│   ├── Fixtures/                   # Test setup and configuration
│   └── Helpers/                    # E2E test utilities
│
└── signpuddle-web/                 # Frontend application and tests
    ├── __tests__/                  # Jest unit tests for components and stores
    │   ├── components/             # Component unit tests
    │   ├── stores/                 # Store unit tests
    │   └── utils/                  # Utility function tests
    └── cypress/                    # Cypress E2E tests
        ├── e2e/                    # End-to-end test scenarios
        ├── component/              # Component integration tests
        └── support/                # Test helpers and utilities
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

### Frontend Test Data
- Use mock stores for component isolation
- Create test fixtures for complex data scenarios
- Maintain separation between unit and E2E test data

## Running Tests

### Backend Unit Tests
```bash
cd src\SignPuddle.API.Tests
dotnet test
```

### Backend E2E Tests
```bash
cd src\SignPuddle.API.E2ETests
dotnet test
```

### Frontend Unit Tests
```bash
cd src\signpuddle-web
npm test
# or for watch mode
npm run test:watch
```

### Frontend E2E Tests
```bash
cd src\signpuddle-web
# Run tests headlessly
npm run cypress:run
# or open Cypress UI
npm run cypress:open
```

### All Backend Tests
```bash
cd src\SignPuddle.API
dotnet test SignPuddle.API.sln
```

### Test Scripts
The project includes convenient batch scripts for Windows:

**`runWebTests.cmd`** - Runs Jest unit tests in watch mode:
```cmd
cd src\signpuddle-web
npm test --watch
```

**`runwebCypress.cmd`** - Runs Cypress E2E tests:
```cmd
cd src\signpuddle-web
npx cypress run
```

## Continuous Integration

The testing strategy supports CI/CD pipelines:
- Fast unit tests (backend and frontend) run on every commit
- E2E tests run on pull requests and releases
- No external dependencies required for mocking frameworks
- Reliable, deterministic test results
- Cypress Cloud integration for visual testing and debugging

### Frontend Testing Pipeline
```yaml
# Example GitHub Actions workflow
- name: Install frontend dependencies
  run: cd src/signpuddle-web && npm ci

- name: Run frontend unit tests
  run: cd src/signpuddle-web && npm test

- name: Start dev server and run E2E tests
  run: cd src/signpuddle-web && npm run test:e2e
```

## Benefits of This Approach

1. **Real-world reliability**: Tests exercise actual code paths
2. **Simpler maintenance**: No mock setup/verification code
3. **Better refactoring support**: Tests don't break when implementation details change
4. **Easier debugging**: Test failures point to real issues
5. **Documentation value**: Tests show how components actually work together
6. **Cross-platform consistency**: Same tests work across different environments
7. **User-focused testing**: Frontend tests verify actual user experiences

## Migration from Moq

If migrating existing tests that use Moq:

1. **Remove Moq dependencies** from project files
2. **Replace mocked services** with real instances
3. **Provide real test data** instead of stubbed responses
4. **Move controller tests** to E2E test project
5. **Focus unit tests** on business logic verification

## Frontend Testing Best Practices

### Component Testing
- Test component contracts, not implementation details
- Use data-cy attributes for reliable element selection
- Test user workflows rather than isolated functions

### Store Testing
- Test state transitions and side effects
- Verify persistence and hydration behavior
- Test error handling and recovery scenarios

### E2E Testing
- Start each test from a clean state
- Use page object patterns for maintainable tests
- Test critical user journeys end-to-end
- Include accessibility testing in your flows

### Test Organization
```javascript
// Good: Organize tests by user behavior
describe('Sign Creation Workflow', () => {
  describe('when user creates new sign', () => {
    test('should allow symbol selection');
    test('should generate valid FSW');
    test('should save to server');
  });
});

// Avoid: Organizing tests by implementation
describe('SignEditor component', () => {
  describe('addSymbol method', () => {
    // Implementation-focused tests
  });
});
```

This approach ensures robust, maintainable tests that provide confidence in the system's behavior.
