# PotomacAnalyst Code Style Guide

This document defines the coding standards and style guidelines for the PotomacAnalyst project to ensure consistency, readability, and maintainability across the codebase.

## Table of Contents

1. [C# Coding Standards](#c-coding-standards)
2. [XAML Standards](#xaml-standards)
3. [File Organization](#file-organization)
4. [Naming Conventions](#naming-conventions)
5. [Code Comments and Documentation](#code-comments-and-documentation)
6. [Error Handling](#error-handling)
7. [Performance Guidelines](#performance-guidelines)
8. [Security Guidelines](#security-guidelines)
9. [Testing Standards](#testing-standards)
10. [Git and Version Control](#git-and-version-control)

## C# Coding Standards

### General Guidelines

#### Code Formatting
- **Indentation**: Use 4 spaces for indentation (no tabs)
- **Line Length**: Keep lines under 120 characters when possible
- **Braces**: Use Allman style braces (new line for opening braces)
- **Spacing**: Use single spaces around operators and after commas

```csharp
// Good
if (condition)
{
    DoSomething();
}

// Bad
if(condition){DoSomething();}
```

#### Using Statements
- Organize using statements alphabetically
- Separate system usings from project usings with a blank line
- Remove unused using statements

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using PotomacAnalyst.Services;
using PotomacAnalyst.ViewModels;
```

#### Regions
Use regions to organize code logically, but avoid excessive nesting:

```csharp
#region Constructors
// Constructor implementations
#endregion

#region Public Methods
// Public method implementations
#endregion

#region Private Methods
// Private method implementations
#endregion
```

### Async/Await Patterns

#### Async Method Naming
- Always suffix async methods with "Async"
- Return Task or Task<T> for async methods

```csharp
public async Task<SomeResult> GetDataAsync(CancellationToken ct = default)
{
    // Implementation
}

public async Task ProcessDataAsync(IEnumerable<DataItem> items)
{
    // Implementation
}
```

#### Async Best Practices
- Use `ConfigureAwait(false)` in library code
- Always provide cancellation tokens for long-running operations
- Handle exceptions appropriately in async methods

```csharp
public async Task<SomeResult> GetDataAsync(CancellationToken ct = default)
{
    try
    {
        return await _httpClient.GetAsync(url, ct)
            .ConfigureAwait(false);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to get data");
        throw;
    }
}
```

### Null Handling

#### Null Checks
- Use null-conditional operators when appropriate
- Use null-coalescing operators for default values
- Validate method parameters for null values

```csharp
// Good
public void ProcessData(string input)
{
    if (string.IsNullOrEmpty(input))
        throw new ArgumentException("Input cannot be null or empty", nameof(input));
    
    var result = input?.Trim() ?? string.Empty;
}

// Bad
public void ProcessData(string input)
{
    var result = input.Trim(); // Potential NullReferenceException
}
```

#### Nullable Reference Types
Enable nullable reference types and use them consistently:

```csharp
#nullable enable

public class UserService
{
    public User? GetUser(int id) => // May return null
        _userRepository.FindById(id);
    
    public string GetUserName(int id) => // Never returns null
        GetUser(id)?.Name ?? "Unknown";
}
```

## XAML Standards

### XAML Formatting

#### Element Structure
- Use proper indentation (4 spaces)
- Place attributes on separate lines for complex elements
- Use self-closing tags for empty elements

```xml
<!-- Good -->
<Grid>
    <TextBlock
        x:Name="TitleTextBlock"
        Text="{x:Bind ViewModel.Title, Mode=OneWay}"
        Style="{StaticResource TitleTextBlockStyle}"
        Margin="16" />
    
    <Button
        x:Name="ActionButton"
        Content="Click Me"
        Command="{x:Bind ViewModel.ActionCommand}"
        HorizontalAlignment="Center"
        VerticalAlignment="Center" />
</Grid>

<!-- Bad -->
<Grid><TextBlock x:Name="TitleTextBlock" Text="{x:Bind ViewModel.Title, Mode=OneWay}" Style="{StaticResource TitleTextBlockStyle}" Margin="16" /></Grid>
```

#### Resource Organization
- Define resources in appropriate resource dictionaries
- Use consistent naming for resources
- Group related resources together

```xml
<!-- In App.xaml or dedicated resource dictionary -->
<Style x:Key="TitleTextBlockStyle" TargetType="TextBlock">
    <Setter Property="FontSize" Value="24" />
    <Setter Property="FontWeight" Value="SemiBold" />
    <Setter Property="Margin" Value="0,0,0,16" />
</Style>

<Style x:Key="SubtitleTextBlockStyle" TargetType="TextBlock">
    <Setter Property="FontSize" Value="16" />
    <Setter Property="Foreground" Value="{ThemeResource TextFillColorSecondaryBrush}" />
    <Setter Property="Margin" Value="0,0,0,24" />
</Style>
```

### Data Binding

#### Binding Modes
- Use `OneWay` for read-only data
- Use `TwoWay` for editable data
- Use `OneTime` for static data

```xml
<!-- Read-only data -->
<TextBlock Text="{x:Bind ViewModel.DisplayName, Mode=OneWay}" />

<!-- Editable data -->
<TextBox Text="{x:Bind ViewModel.UserName, Mode=TwoWay}" />

<!-- Static data -->
<TextBlock Text="{x:Bind ViewModel.StaticText, Mode=OneTime}" />
```

#### Binding Validation
- Use appropriate converters for data transformation
- Implement validation in view models
- Handle binding errors gracefully

```xml
<!-- With converter -->
<TextBlock Text="{x:Bind ViewModel.CreationDate, Mode=OneWay, Converter={StaticResource DateTimeConverter}}" />

<!-- With validation -->
<TextBox
    Text="{x:Bind ViewModel.Email, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    PlaceholderText="Enter email address">
    <TextBox.Resources>
        <Style TargetType="TextBlock">
            <Setter Property="Foreground" Value="Red" />
            <Setter Property="FontSize" Value="12" />
        </Style>
    </TextBox.Resources>
</TextBox>
```

## File Organization

### Project Structure
Follow the established project structure:

```
PotomacAnalyst/
├── Views/
│   ├── Auth/
│   │   ├── LoginPage.xaml
│   │   ├── LoginPage.xaml.cs
│   │   └── ...
│   └── Pages/
│       ├── DashboardPage.xaml
│       ├── DashboardPage.xaml.cs
│       └── ...
├── ViewModels/
│   ├── Auth/
│   │   ├── LoginViewModel.cs
│   │   └── ...
│   └── Pages/
│       ├── DashboardViewModel.cs
│       └── ...
├── Services/
│   ├── ApiService.cs
│   ├── AuthService.cs
│   └── ...
├── Models/
│   ├── UserProfile.cs
│   └── ...
└── Controls/
    ├── CustomControl.xaml
    └── ...
```

### File Naming
- Use PascalCase for all file names
- Use descriptive names that reflect the file's purpose
- Group related files in appropriate directories

### Class Organization
Within each file, organize members in this order:
1. Constants
2. Static fields
3. Instance fields
4. Constructors
5. Properties
6. Events
7. Public methods
8. Protected methods
9. Private methods

```csharp
public class ExampleClass
{
    // Constants
    private const string DefaultName = "Example";
    
    // Static fields
    private static int _instanceCount;
    
    // Instance fields
    private readonly ILogger _logger;
    private string _name;
    
    // Constructors
    public ExampleClass(ILogger logger)
    {
        _logger = logger;
        _instanceCount++;
    }
    
    // Properties
    public string Name
    {
        get => _name ?? DefaultName;
        set => _name = value;
    }
    
    // Events
    public event EventHandler? NameChanged;
    
    // Public methods
    public void DoSomething()
    {
        // Implementation
    }
    
    // Protected methods
    protected virtual void OnNameChanged()
    {
        NameChanged?.Invoke(this, EventArgs.Empty);
    }
    
    // Private methods
    private void InternalMethod()
    {
        // Implementation
    }
}
```

## Naming Conventions

### General Rules
- Use PascalCase for public members and types
- Use camelCase for private members and parameters
- Use meaningful, descriptive names
- Avoid abbreviations unless they're widely understood
- Use nouns for classes and properties
- Use verbs for methods

### Class Names
- Use nouns or noun phrases
- Use descriptive names that indicate the class's purpose
- Avoid suffixes like "Manager", "Processor", etc. unless truly necessary

```csharp
// Good
public class UserProfile
public class DataProcessor
public class AuthenticationManager

// Avoid
public class UserProfileClass
public class ProcessData
public class Manager
```

### Method Names
- Use verb-noun pairs
- Use descriptive names that indicate the method's action
- Use past tense for methods that return a result

```csharp
// Good
public void SaveData()
public void ProcessRequest()
public User GetUserById(int id)
public bool ValidateInput(string input)

// Avoid
public void Data()
public void Request()
public User Get(int id)
public bool Input(string input)
```

### Property Names
- Use nouns or noun phrases
- Use PascalCase
- Avoid prefixes like "m_" or "_" for private fields

```csharp
// Good
public string DisplayName { get; set; }
public DateTime CreatedAt { get; set; }
public bool IsActive { get; set; }

// Avoid
public string m_displayName;
public string _displayName;
public string displayName;
```

### Variable Names
- Use camelCase
- Use descriptive names
- Use meaningful names for loop variables

```csharp
// Good
private string userName;
private int userCount;
private readonly List<User> users;

foreach (var user in users)
{
    // Process user
}

// Avoid
private string un;
private int uc;
private readonly List<User> u;

foreach (var u in users)
{
    // Process user
}
```

### Constants and Enums
- Use PascalCase for constants and enum values
- Use descriptive names
- Group related constants in partial classes or static classes

```csharp
// Good
public static class Constants
{
    public const string DefaultTheme = "Dark";
    public const int MaxRetryCount = 3;
}

public enum UserRole
{
    Administrator,
    Moderator,
    User
}

// Avoid
public const string defaultTheme = "Dark";
public const int max_retry_count = 3;

public enum user_role
{
    admin,
    mod,
    user
}
```

## Code Comments and Documentation

### XML Documentation
Use XML documentation for all public APIs:

```csharp
/// <summary>
/// Represents a user profile with authentication and preference data.
/// </summary>
/// <remarks>
/// This class is used throughout the application for user identification
/// and personalization features.
/// </remarks>
public class UserProfile
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    /// <value>
    /// The user's unique identifier.
    /// </value>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the email address is invalid.
    /// </exception>
    public string Email
    {
        get => _email;
        set => _email = ValidateEmail(value);
    }
    
    /// <summary>
    /// Validates an email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when email is null or empty.</exception>
    private bool ValidateEmail(string email)
    {
        // Implementation
    }
}
```

### Inline Comments
Use inline comments sparingly and only when necessary:

```csharp
// Good - explains complex logic
// Use binary search for O(log n) performance instead of O(n) linear search
int index = Array.BinarySearch(sortedArray, targetValue);

// Bad - states the obvious
int result = CalculateTotal(); // Calculate the total

// Good - explains business logic
// Apply discount only if user has premium membership and purchase exceeds minimum amount
if (user.IsPremium && purchaseAmount > MinimumDiscountAmount)
{
    ApplyDiscount();
}
```

### TODO and FIXME Comments
Use TODO and FIXME comments for tracking work:

```csharp
// TODO: Implement caching mechanism for improved performance
// FIXME: Handle edge case where data source is temporarily unavailable
// NOTE: This method will be deprecated in version 2.0
```

## Error Handling

### Exception Handling Strategy
- Use specific exception types
- Provide meaningful error messages
- Log exceptions appropriately
- Don't catch and ignore exceptions

```csharp
// Good
public async Task<User> GetUserAsync(int userId)
{
    try
    {
        return await _userRepository.GetByIdAsync(userId);
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "User {UserId} not found", userId);
        throw new UserNotFoundException($"User with ID {userId} was not found", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error retrieving user {UserId}", userId);
        throw new ServiceException("An error occurred while retrieving the user", ex);
    }
}

// Bad
public async Task<User> GetUserAsync(int userId)
{
    try
    {
        return await _userRepository.GetByIdAsync(userId);
    }
    catch
    {
        return null; // Silent failure
    }
}
```

### Custom Exceptions
Create custom exception types for domain-specific errors:

```csharp
public class UserNotFoundException : Exception
{
    public int UserId { get; }
    
    public UserNotFoundException(int userId) 
        : base($"User with ID {userId} was not found")
    {
        UserId = userId;
    }
    
    public UserNotFoundException(int userId, Exception innerException)
        : base($"User with ID {userId} was not found", innerException)
    {
        UserId = userId;
    }
}
```

### Validation
Validate inputs at method boundaries:

```csharp
public void ProcessData(string input, int count)
{
    if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input cannot be null or empty", nameof(input));
    
    if (count <= 0)
        throw new ArgumentException("Count must be greater than zero", nameof(count));
    
    // Method implementation
}
```

## Performance Guidelines

### Memory Management
- Implement IDisposable for classes that use unmanaged resources
- Use using statements for disposable objects
- Avoid unnecessary object creation in loops

```csharp
// Good
using var httpClient = new HttpClient();
var response = await httpClient.GetAsync(url);

// Bad
var httpClient = new HttpClient();
var response = await httpClient.GetAsync(url);
// httpClient never disposed

// Good - reuse objects when possible
private readonly StringBuilder _stringBuilder = new();
public string BuildString(IEnumerable<string> items)
{
    _stringBuilder.Clear();
    foreach (var item in items)
    {
        _stringBuilder.Append(item);
    }
    return _stringBuilder.ToString();
}

// Bad - creates new StringBuilder in loop
public string BuildString(IEnumerable<string> items)
{
    var result = string.Empty;
    foreach (var item in items)
    {
        result += item; // Creates new string each time
    }
    return result;
}
```

### Async Patterns
- Use async/await for I/O operations
- Avoid async void methods
- Use ValueTask for hot paths when appropriate

```csharp
// Good
public async Task ProcessFileAsync(string filePath)
{
    using var stream = File.OpenRead(filePath);
    await ProcessStreamAsync(stream);
}

// Bad
public async void ProcessFileAsync(string filePath) // Fire and forget
{
    using var stream = File.OpenRead(filePath);
    await ProcessStreamAsync(stream);
}
```

### Collections
- Use appropriate collection types for the use case
- Pre-size collections when the size is known
- Use LINQ judiciously

```csharp
// Good - use HashSet for lookups
private readonly HashSet<string> _validCodes = new();

// Good - pre-size when size is known
var users = new List<User>(expectedCount);

// Good - use LINQ appropriately
var activeUsers = users.Where(u => u.IsActive).ToList();

// Bad - use List for lookups
private readonly List<string> _validCodes = new();

// Bad - unnecessary ToList()
var query = users.Where(u => u.IsActive); // Deferred execution
```

## Security Guidelines

### Input Validation
- Validate all user inputs
- Use parameterized queries to prevent SQL injection
- Sanitize output to prevent XSS

```csharp
// Good - parameterized query
public async Task<User> GetUserByEmailAsync(string email)
{
    var query = "SELECT * FROM Users WHERE Email = @Email";
    return await _db.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
}

// Bad - string concatenation (SQL injection risk)
public async Task<User> GetUserByEmailAsync(string email)
{
    var query = $"SELECT * FROM Users WHERE Email = '{email}'";
    return await _db.QueryFirstOrDefaultAsync<User>(query);
}
```

### Authentication and Authorization
- Use secure authentication mechanisms
- Implement proper authorization checks
- Never store passwords in plain text

```csharp
// Good - use secure password hashing
public bool VerifyPassword(string password, string hashedPassword)
{
    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}

// Bad - plain text password comparison
public bool VerifyPassword(string password, string storedPassword)
{
    return password == storedPassword;
}
```

### Data Protection
- Encrypt sensitive data at rest and in transit
- Use secure random number generation
- Implement proper session management

```csharp
// Good - use secure random generation
private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
public byte[] GenerateSecureToken(int length)
{
    var token = new byte[length];
    _rng.GetBytes(token);
    return token;
}

// Bad - use insecure random generation
private static readonly Random _random = new Random();
public byte[] GenerateToken(int length)
{
    var token = new byte[length];
    _random.NextBytes(token);
    return token;
}
```

## Testing Standards

### Test Organization
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Group related tests in the same class

```csharp
public class UserServiceTests
{
    [Fact]
    public void GetUserById_ValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "Test User" };
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(r => r.GetById(userId)).Returns(expectedUser);
        
        var userService = new UserService(mockRepository.Object);
        
        // Act
        var result = userService.GetUserById(userId);
        
        // Assert
        Assert.Equal(expectedUser, result);
    }
}
```

### Test Data
- Use test data builders for complex objects
- Create realistic test data
- Clean up test data after tests

```csharp
public static class TestDataFactory
{
    public static User CreateUser(string name = "Test User", string email = null)
    {
        return new User
        {
            Id = 1,
            Name = name,
            Email = email ?? $"{name.ToLower().Replace(" ", ".")}@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Mocking
- Use mocking frameworks for dependencies
- Verify interactions when necessary
- Avoid over-mocking

```csharp
[Fact]
public async Task ProcessOrder_ValidOrder_CallsRepository()
{
    // Arrange
    var order = TestDataFactory.CreateOrder();
    var mockRepository = new Mock<IOrderRepository>();
    var orderService = new OrderService(mockRepository.Object);
    
    // Act
    await orderService.ProcessOrderAsync(order);
    
    // Assert
    mockRepository.Verify(r => r.SaveAsync(order), Times.Once);
}
```

## Git and Version Control

### Commit Messages
- Use clear, descriptive commit messages
- Follow conventional commit format when possible
- Reference issue numbers when applicable

```
feat: add user authentication module

- Implement login/logout functionality
- Add JWT token management
- Include unit tests for authentication service

Closes #123
```

### Branching Strategy
- Use feature branches for new development
- Keep branches short-lived
- Use descriptive branch names

```
feature/user-authentication
bugfix/fix-login-validation
hotfix/critical-security-issue
```

### Code Review
- Request reviews for all non-trivial changes
- Address all review comments before merging
- Use pull request templates for consistency

This code style guide ensures consistency and quality across the PotomacAnalyst codebase. All contributors should follow these guidelines to maintain code quality and readability.