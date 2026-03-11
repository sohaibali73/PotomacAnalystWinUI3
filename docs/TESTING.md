# PotomacAnalyst Testing Guidelines

This document provides comprehensive testing guidelines for the PotomacAnalyst project, covering unit testing, integration testing, UI testing, and testing best practices.

## Table of Contents

1. [Testing Overview](#testing-overview)
2. [Test Structure and Organization](#test-structure-and-organization)
3. [Unit Testing](#unit-testing)
4. [Integration Testing](#integration-testing)
5. [UI Testing](#ui-testing)
6. [Performance Testing](#performance-testing)
7. [Security Testing](#security-testing)
8. [Testing Tools and Frameworks](#testing-tools-and-frameworks)
9. [Test Data Management](#test-data-management)
10. [Continuous Integration Testing](#continuous-integration-testing)
11. [Testing Best Practices](#testing-best-practices)
12. [Test Documentation](#test-documentation)

## Testing Overview

### Testing Philosophy

PotomacAnalyst follows a comprehensive testing strategy that ensures:

- **Quality Assurance**: Every feature is thoroughly tested before release
- **Regression Prevention**: Automated tests prevent existing functionality from breaking
- **Performance Monitoring**: Regular performance testing ensures optimal user experience
- **Security Validation**: Security tests identify and prevent vulnerabilities
- **User Experience**: UI tests ensure consistent and intuitive user interactions

### Testing Levels

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test component interactions and external dependencies
3. **UI Tests**: Test user interface functionality and user workflows
4. **Performance Tests**: Test application performance under various conditions
5. **Security Tests**: Test for security vulnerabilities and data protection

### Testing Principles

- **Test-Driven Development (TDD)**: Write tests before implementing features when possible
- **Automated Testing**: Maximize automated test coverage to reduce manual testing burden
- **Continuous Testing**: Run tests automatically on every code change
- **Test Independence**: Each test should be independent and not rely on other tests
- **Clear Test Names**: Use descriptive test names that explain what is being tested

## Test Structure and Organization

### Test Project Structure

```
Tests/
├── UnitTests/
│   ├── Services/
│   │   ├── ApiServiceTests.cs
│   │   ├── AuthServiceTests.cs
│   │   └── SessionServiceTests.cs
│   ├── ViewModels/
│   │   ├── DashboardViewModelTests.cs
│   │   ├── ChatViewModelTests.cs
│   │   └── BacktestViewModelTests.cs
│   └── Models/
│       ├── UserProfileTests.cs
│       └── DataModelsTests.cs
├── IntegrationTests/
│   ├── DatabaseTests/
│   ├── APITests/
│   └── ServiceIntegrationTests/
├── UITests/
│   ├── NavigationTests/
│   ├── ModuleTests/
│   └── WorkflowTests/
├── PerformanceTests/
│   ├── LoadTests/
│   ├── StressTests/
│   └── MemoryTests/
└── SecurityTests/
    ├── AuthenticationTests/
    ├── AuthorizationTests/
    └── DataProtectionTests/
```

### Test Naming Conventions

#### Unit Tests
- **Class Name**: `[ClassName]Tests.cs`
- **Method Name**: `[MethodName]_[Scenario]_[ExpectedResult]`
- **Example**: `CalculateTotal_WithValidInput_ReturnsCorrectTotal`

#### Integration Tests
- **Class Name**: `[ComponentName]IntegrationTests.cs`
- **Method Name**: `[Scenario]_IntegrationTest`
- **Example**: `DatabaseConnection_IntegrationTest`

#### UI Tests
- **Class Name**: `[FeatureName]UITests.cs`
- **Method Name**: `[UserAction]_UIValidation`
- **Example**: `LoginFlow_UIValidation`

### Test Categories

```csharp
// Unit Tests
[Fact]
public void CalculateTotal_WithValidInput_ReturnsCorrectTotal()
{
    // Test implementation
}

// Integration Tests
[Theory]
[InlineData("valid_connection_string")]
[InlineData("invalid_connection_string")]
public void DatabaseConnection_IntegrationTest(string connectionString)
{
    // Test implementation
}

// UI Tests
[Fact]
[Trait("Category", "UI")]
public void LoginFlow_UIValidation()
{
    // Test implementation
}

// Performance Tests
[Fact]
[Trait("Category", "Performance")]
public void LargeDataSet_Processing_PerformanceTest()
{
    // Test implementation
}
```

## Unit Testing

### Unit Testing Framework

PotomacAnalyst uses **xUnit** as the primary unit testing framework with **Moq** for mocking dependencies.

#### Required Packages
```xml
<PackageReference Include="xunit" Version="2.6.6" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
<PackageReference Include="Moq" Version="4.20.71" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
```

### Unit Testing Patterns

#### AAA Pattern (Arrange, Act, Assert)
```csharp
[Fact]
public void CalculateTotal_WithValidInput_ReturnsCorrectTotal()
{
    // Arrange
    var calculator = new Calculator();
    var items = new List<decimal> { 10.00m, 20.00m, 5.00m };
    var expectedTotal = 35.00m;
    
    // Act
    var result = calculator.CalculateTotal(items);
    
    // Assert
    Assert.Equal(expectedTotal, result);
}
```

#### Mocking Dependencies
```csharp
[Fact]
public async Task GetDataAsync_WithValidRequest_ReturnsData()
{
    // Arrange
    var mockService = new Mock<IApiService>();
    var expectedData = new SomeData { Id = 1, Name = "Test" };
    
    mockService.Setup(s => s.GetDataAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(expectedData);
    
    var viewModel = new SomeViewModel(mockService.Object);
    
    // Act
    await viewModel.LoadDataAsync();
    
    // Assert
    Assert.NotNull(viewModel.Data);
    Assert.Equal(expectedData.Id, viewModel.Data.Id);
}
```

#### Testing Async Methods
```csharp
[Fact]
public async Task ProcessDataAsync_WithLargeDataSet_CompletesSuccessfully()
{
    // Arrange
    var processor = new DataProcessor();
    var largeDataSet = GenerateLargeDataSet();
    
    // Act
    var result = await processor.ProcessDataAsync(largeDataSet);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsProcessed);
}
```

### Unit Testing Best Practices

#### Test Isolation
- Each test should be independent
- Don't share state between tests
- Use fresh test data for each test
- Mock all external dependencies

#### Test Data
```csharp
public class TestDataFactory
{
    public static UserProfile CreateValidUserProfile()
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static List<SomeData> CreateTestDataList(int count)
    {
        return Enumerable.Range(1, count)
                        .Select(i => new SomeData { Id = i, Name = $"Item {i}" })
                        .ToList();
    }
}
```

#### Test Helpers
```csharp
public static class TestHelpers
{
    public static async Task WaitForConditionAsync(
        Func<bool> condition, 
        TimeSpan timeout, 
        TimeSpan pollingInterval = default)
    {
        pollingInterval = pollingInterval == default ? TimeSpan.FromMilliseconds(100) : pollingInterval;
        
        var startTime = DateTime.UtcNow;
        while (!condition() && DateTime.UtcNow - startTime < timeout)
        {
            await Task.Delay(pollingInterval);
        }
        
        if (!condition())
        {
            throw new TimeoutException($"Condition not met within {timeout}");
        }
    }
}
```

## Integration Testing

### Integration Testing Strategy

Integration tests verify that different components work together correctly and that external dependencies function as expected.

#### Types of Integration Tests

1. **Database Integration Tests**: Test database operations and data persistence
2. **API Integration Tests**: Test external API calls and responses
3. **Service Integration Tests**: Test service layer interactions
4. **Configuration Tests**: Test configuration loading and validation

### Database Integration Tests

#### Test Database Setup
```csharp
public class DatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public DatabaseIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task SaveUserProfile_SavesToDatabase()
    {
        // Arrange
        var userProfile = TestDataFactory.CreateValidUserProfile();
        var repository = new UserProfileRepository(_fixture.ConnectionString);
        
        // Act
        await repository.SaveAsync(userProfile);
        
        // Assert
        var savedProfile = await repository.GetByIdAsync(userProfile.Id);
        Assert.NotNull(savedProfile);
        Assert.Equal(userProfile.Email, savedProfile.Email);
    }
}
```

#### Database Fixture
```csharp
public class DatabaseFixture : IDisposable
{
    public string ConnectionString { get; }
    
    public DatabaseFixture()
    {
        // Set up test database
        ConnectionString = CreateTestDatabase();
    }
    
    private string CreateTestDatabase()
    {
        // Create and configure test database
        // Return connection string
    }
    
    public void Dispose()
    {
        // Clean up test database
    }
}
```

### API Integration Tests

#### Mock HTTP Server
```csharp
public class ApiIntegrationTests : IClassFixture<HttpServerFixture>
{
    private readonly HttpServerFixture _fixture;
    
    public ApiIntegrationTests(HttpServerFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetUserData_ValidRequest_ReturnsUserData()
    {
        // Arrange
        var expectedUser = new { Id = 1, Name = "Test User" };
        _fixture.MockServer
                .Given(Request.Create().WithPath("/api/users/1"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(expectedUser));
        
        var apiService = new ApiService(_fixture.MockServer.Urls[0]);
        
        // Act
        var result = await apiService.GetUserAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Name, result.Name);
    }
}
```

### Service Integration Tests

#### Service Dependencies
```csharp
public class ServiceIntegrationTests : IClassFixture<ServiceFixture>
{
    private readonly ServiceFixture _fixture;
    
    public ServiceIntegrationTests(ServiceFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task ProcessUserRegistration_ValidData_CreatesUser()
    {
        // Arrange
        var registrationData = new UserRegistration
        {
            Email = "test@example.com",
            Password = "password123",
            DisplayName = "Test User"
        };
        
        var userService = _fixture.GetService<UserService>();
        
        // Act
        var result = await userService.RegisterAsync(registrationData);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(registrationData.Email, result.User.Email);
    }
}
```

## UI Testing

### UI Testing Framework

PotomacAnalyst uses **WinAppDriver** and **Microsoft.UI.Xaml.Tests** for UI automation testing.

#### Required Packages
```xml
<PackageReference Include="Microsoft.Win32.Registry" Version="5.0.0" />
<PackageReference Include="Microsoft.UI.Xaml" Version="2.8.4" />
<PackageReference Include="WinAppDriver" Version="1.2.1" />
```

### UI Test Structure

#### UI Test Base Class
```csharp
public abstract class UITestBase : IAsyncLifetime
{
    protected WindowsDriver<WindowsElement> Driver { get; private set; }
    protected WebDriverWait Wait { get; private set; }
    
    public async Task InitializeAsync()
    {
        var appCapabilities = new DesiredCapabilities();
        appCapabilities.SetCapability("app", "PotomacAnalyst_abc123!App");
        appCapabilities.SetCapability("deviceName", "WindowsPC");
        
        Driver = new WindowsDriver<WindowsElement>(
            new Uri("http://127.0.0.1:4723"),
            appCapabilities);
        
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    }
    
    public async Task DisposeAsync()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }
    
    protected WindowsElement FindElement(string automationId)
    {
        return Wait.Until(d => d.FindElementByAccessibilityId(automationId));
    }
    
    protected void ClickElement(string automationId)
    {
        var element = FindElement(automationId);
        element.Click();
    }
    
    protected void EnterText(string automationId, string text)
    {
        var element = FindElement(automationId);
        element.Clear();
        element.SendKeys(text);
    }
}
```

### UI Test Examples

#### Login Flow Test
```csharp
public class LoginUITests : UITestBase
{
    [Fact]
    public void LoginFlow_ValidCredentials_NavigatesToDashboard()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        
        // Act
        EnterText("UsernameTextBox", username);
        EnterText("PasswordTextBox", password);
        ClickElement("LoginButton");
        
        // Assert
        var dashboardTitle = FindElement("DashboardTitle");
        Assert.Equal("Dashboard", dashboardTitle.Text);
    }
    
    [Fact]
    public void LoginFlow_InvalidCredentials_ShowsErrorMessage()
    {
        // Arrange
        var username = "invaliduser";
        var password = "wrongpassword";
        
        // Act
        EnterText("UsernameTextBox", username);
        EnterText("PasswordTextBox", password);
        ClickElement("LoginButton");
        
        // Assert
        var errorMessage = FindElement("ErrorMessage");
        Assert.Contains("Invalid credentials", errorMessage.Text);
    }
}
```

#### Module Navigation Test
```csharp
public class NavigationUITests : UITestBase
{
    [Fact]
    public void NavigateToAFLGenerator_OpensAFLModule()
    {
        // Act
        ClickElement("AFLGeneratorButton");
        
        // Assert
        var aflTitle = FindElement("AFLTitle");
        Assert.Equal("AFL Generator", aflTitle.Text);
    }
    
    [Fact]
    public void NavigateBetweenModules_MaintainsState()
    {
        // Arrange
        ClickElement("AFLGeneratorButton");
        EnterText("FormulaTextBox", "test formula");
        
        // Act
        ClickElement("ChatButton");
        ClickElement("AFLGeneratorButton");
        
        // Assert
        var formulaText = FindElement("FormulaTextBox");
        Assert.Equal("test formula", formulaText.Text);
    }
}
```

### UI Test Best Practices

#### Wait Strategies
```csharp
protected void WaitForElement(string automationId, TimeSpan timeout)
{
    Wait.Until(d => d.FindElementByAccessibilityId(automationId));
}

protected void WaitForElementToDisappear(string automationId, TimeSpan timeout)
{
    Wait.Until(d => !d.FindElementsByAccessibilityId(automationId).Any());
}

protected void WaitForCondition(Func<bool> condition, TimeSpan timeout)
{
    Wait.Until(d => 
    {
        try { return condition(); }
        catch { return false; }
    });
}
```

#### Screenshot Capture
```csharp
protected void CaptureScreenshot(string testName)
{
    var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
    var screenshotPath = Path.Combine(
        "TestResults", 
        $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
    screenshot.SaveAsFile(screenshotPath);
}
```

## Performance Testing

### Performance Testing Strategy

Performance tests ensure that PotomacAnalyst maintains optimal performance under various conditions and data loads.

#### Performance Test Categories

1. **Load Testing**: Test application behavior under normal and peak loads
2. **Stress Testing**: Test application behavior beyond normal operational capacity
3. **Memory Testing**: Monitor memory usage and detect memory leaks
4. **Response Time Testing**: Measure response times for critical operations

### Performance Test Implementation

#### Load Testing
```csharp
public class LoadTests
{
    [Fact]
    public async Task ProcessLargeDataSet_CompletesWithinTimeLimit()
    {
        // Arrange
        var processor = new DataProcessor();
        var largeDataSet = TestDataFactory.CreateLargeDataSet(10000);
        var timeLimit = TimeSpan.FromSeconds(30);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await processor.ProcessDataAsync(largeDataSet);
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.Elapsed < timeLimit, 
            $"Processing took {stopwatch.Elapsed}, which exceeds the limit of {timeLimit}");
        Assert.NotNull(result);
    }
}
```

#### Memory Testing
```csharp
public class MemoryTests
{
    [Fact]
    public void ProcessData_DoesNotCauseMemoryLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var processor = new DataProcessor();
        
        // Act
        for (int i = 0; i < 100; i++)
        {
            var dataSet = TestDataFactory.CreateLargeDataSet(1000);
            var result = processor.ProcessData(dataSet);
            // Explicitly dispose of objects
            result = null;
            dataSet = null;
        }
        
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;
        
        // Assert
        Assert.True(memoryIncrease < 1024 * 1024, // Less than 1MB increase
            $"Memory increased by {memoryIncrease} bytes, indicating a potential memory leak");
    }
}
```

#### Response Time Testing
```csharp
public class ResponseTimeTests
{
    [Fact]
    public async Task LoadDashboard_CompletesWithinTimeLimit()
    {
        // Arrange
        var dashboardService = new DashboardService();
        var timeLimit = TimeSpan.FromSeconds(5);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var dashboardData = await dashboardService.LoadDashboardAsync();
        stopwatch.Stop();
        
        // Assert
        Assert.True(stopwatch.Elapsed < timeLimit,
            $"Dashboard loading took {stopwatch.Elapsed}, which exceeds the limit of {timeLimit}");
        Assert.NotNull(dashboardData);
    }
}
```

## Security Testing

### Security Testing Strategy

Security tests ensure that PotomacAnalyst protects user data and prevents security vulnerabilities.

#### Security Test Categories

1. **Authentication Tests**: Verify authentication mechanisms
2. **Authorization Tests**: Verify access control and permissions
3. **Data Protection Tests**: Verify data encryption and protection
4. **Input Validation Tests**: Verify input sanitization and validation

### Security Test Implementation

#### Authentication Tests
```csharp
public class AuthenticationTests
{
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var authService = new AuthService();
        var invalidCredentials = new LoginCredentials
        {
            Username = "invaliduser",
            Password = "wrongpassword"
        };
        
        // Act
        var result = await authService.LoginAsync(invalidCredentials);
        
        // Assert
        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Contains("Invalid", result.ErrorMessage);
    }
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var authService = new AuthService();
        var validCredentials = new LoginCredentials
        {
            Username = "testuser",
            Password = "correctpassword"
        };
        
        // Act
        var result = await authService.LoginAsync(validCredentials);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Token);
    }
}
```

#### Input Validation Tests
```csharp
public class InputValidationTests
{
    [Fact]
    public void ProcessInput_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var processor = new InputProcessor();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            processor.ProcessInput(string.Empty));
        
        Assert.Contains("cannot be empty", exception.Message);
    }
    
    [Fact]
    public void ProcessInput_SQLInjectionAttempt_ThrowsArgumentException()
    {
        // Arrange
        var processor = new InputProcessor();
        var maliciousInput = "'; DROP TABLE Users; --";
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            processor.ProcessInput(maliciousInput));
        
        Assert.Contains("invalid input", exception.Message);
    }
}
```

#### Data Protection Tests
```csharp
public class DataProtectionTests
{
    [Fact]
    public void EncryptData_DataIsEncrypted()
    {
        // Arrange
        var protector = new DataProtector();
        var plainText = "sensitive data";
        
        // Act
        var encrypted = protector.Encrypt(plainText);
        var decrypted = protector.Decrypt(encrypted);
        
        // Assert
        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }
    
    [Fact]
    public void StorePassword_PasswordIsHashed()
    {
        // Arrange
        var passwordService = new PasswordService();
        var plainPassword = "mypassword123";
        
        // Act
        var hashedPassword = passwordService.HashPassword(plainPassword);
        
        // Assert
        Assert.NotEqual(plainPassword, hashedPassword);
        Assert.True(passwordService.VerifyPassword(plainPassword, hashedPassword));
    }
}
```

## Testing Tools and Frameworks

### Testing Frameworks

#### xUnit
- **Purpose**: Primary unit testing framework
- **Features**: Theory support, data-driven tests, parallel execution
- **Usage**: Unit tests, integration tests

#### Moq
- **Purpose**: Mocking framework for dependencies
- **Features**: Fluent API, strict/loose mocking, callback support
- **Usage**: Mocking services, repositories, external dependencies

#### WinAppDriver
- **Purpose**: UI automation testing for WinUI applications
- **Features**: Cross-platform UI testing, element identification
- **Usage**: UI tests, end-to-end tests

### Testing Utilities

#### Test Data Builders
```csharp
public class UserProfileBuilder
{
    private string _email = "test@example.com";
    private string _displayName = "Test User";
    private DateTime _createdAt = DateTime.UtcNow;
    
    public UserProfileBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }
    
    public UserProfileBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }
    
    public UserProfile Build()
    {
        return new UserProfile
        {
            Email = _email,
            DisplayName = _displayName,
            CreatedAt = _createdAt
        };
    }
}
```

#### Test Configuration
```csharp
public class TestConfiguration
{
    public static string TestConnectionString => 
        "Server=localhost;Database=PotomacAnalystTest;Trusted_Connection=true;";
    
    public static string TestApiBaseUrl => "https://api-test.potomacanalyst.com";
    
    public static bool EnableIntegrationTests => 
        Environment.GetEnvironmentVariable("ENABLE_INTEGRATION_TESTS") == "true";
}
```

## Test Data Management

### Test Data Strategies

#### In-Memory Data
```csharp
public class InMemoryDatabaseFixture : IDisposable
{
    private readonly List<UserProfile> _users = new();
    private readonly List<DataRecord> _records = new();
    
    public void AddUser(UserProfile user) => _users.Add(user);
    public void AddRecord(DataRecord record) => _records.Add(record);
    
    public IEnumerable<UserProfile> GetUsers() => _users;
    public IEnumerable<DataRecord> GetRecords() => _records;
    
    public void Dispose()
    {
        _users.Clear();
        _records.Clear();
    }
}
```

#### Test Data Factories
```csharp
public static class TestDataFactory
{
    public static UserProfile CreateTestUser(string email = null, string name = null)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            Email = email ?? $"user{Guid.NewGuid()}@example.com",
            DisplayName = name ?? $"Test User {DateTime.Now.Ticks}",
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static List<DataRecord> CreateTestDataRecords(int count)
    {
        return Enumerable.Range(1, count)
                        .Select(i => new DataRecord
                        {
                            Id = i,
                            Value = $"Value {i}",
                            Timestamp = DateTime.UtcNow.AddDays(-i)
                        })
                        .ToList();
    }
}
```

### Test Data Cleanup
```csharp
public class TestDataCleanup : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    
    public void AddDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }
    
    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            try { disposable.Dispose(); }
            catch { /* Ignore cleanup errors */ }
        }
        _disposables.Clear();
    }
}
```

## Continuous Integration Testing

### CI/CD Pipeline Testing

#### GitHub Actions Workflow
```yaml
name: Test
on: [push, pull_request]
jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Run tests
      run: dotnet test --no-build --verbosity normal
    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: failure()
      with:
        name: test-results
        path: '**/TestResults/**/*.xml'
```

#### Test Categories in CI
```csharp
// Unit tests (always run)
[Fact]
public void UnitTest_Example()
{
    // Implementation
}

// Integration tests (run in CI)
[Fact]
[Trait("Category", "Integration")]
public void IntegrationTest_Example()
{
    // Implementation
}

// Performance tests (run nightly)
[Fact]
[Trait("Category", "Performance")]
public void PerformanceTest_Example()
{
    // Implementation
}
```

### Test Execution Strategies

#### Parallel Test Execution
```csharp
// AssemblyInfo.cs
[assembly: CollectionBehavior(DisableTestParallelization = false)]
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
```

#### Conditional Test Execution
```csharp
[Fact]
[SkipOnAzurePipelines]
public void LocalOnlyTest_Example()
{
    // Test that should only run locally
}

public class SkipOnAzurePipelinesAttribute : FactAttribute
{
    public SkipOnAzurePipelinesAttribute()
    {
        if (Environment.GetEnvironmentVariable("TF_BUILD") == "True")
        {
            Skip = "Skipping test on Azure Pipelines";
        }
    }
}
```

## Testing Best Practices

### Test Organization

#### Test Class Structure
```csharp
public class CalculatorTests
{
    private Calculator _calculator;
    
    public CalculatorTests()
    {
        _calculator = new Calculator();
    }
    
    public void Dispose()
    {
        _calculator?.Dispose();
        _calculator = null;
    }
    
    public class CalculateTotal : CalculatorTests
    {
        [Fact]
        public void WithValidInput_ReturnsCorrectTotal()
        {
            // Test implementation
        }
        
        [Fact]
        public void WithEmptyList_ReturnsZero()
        {
            // Test implementation
        }
    }
    
    public class CalculateAverage : CalculatorTests
    {
        [Fact]
        public void WithValidInput_ReturnsCorrectAverage()
        {
            // Test implementation
        }
    }
}
```

### Test Documentation

#### Test Comments
```csharp
/// <summary>
/// Tests the CalculateTotal method with various input scenarios.
/// </summary>
public class CalculateTotalTests
{
    /// <summary>
    /// Verifies that CalculateTotal returns the correct sum for a list of positive numbers.
    /// This test ensures basic functionality works as expected.
    /// </summary>
    [Fact]
    public void CalculateTotal_WithPositiveNumbers_ReturnsCorrectSum()
    {
        // Test implementation
    }
    
    /// <summary>
    /// Verifies that CalculateTotal handles empty lists by returning zero.
    /// This test ensures edge cases are handled properly.
    /// </summary>
    [Fact]
    public void CalculateTotal_WithEmptyList_ReturnsZero()
    {
        // Test implementation
    }
}
```

### Test Maintenance

#### Test Review Process
1. **Regular Review**: Review tests monthly for relevance and performance
2. **Test Refactoring**: Refactor tests when code changes
3. **Test Removal**: Remove obsolete tests that no longer provide value
4. **Test Documentation**: Keep test documentation up to date

#### Test Metrics
- **Code Coverage**: Maintain minimum 80% code coverage
- **Test Execution Time**: Keep test suite execution under 10 minutes
- **Test Reliability**: Aim for 95% test pass rate
- **Test Maintenance**: Regular review and cleanup of test code

This comprehensive testing strategy ensures that PotomacAnalyst maintains high quality, reliability, and performance throughout its development lifecycle.