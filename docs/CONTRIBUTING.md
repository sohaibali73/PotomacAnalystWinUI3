# Contributing to PotomacAnalyst

Thank you for considering contributing to PotomacAnalyst! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Documentation](#documentation)
- [Issue Reporting](#issue-reporting)
- [Pull Request Process](#pull-request-process)
- [Feature Requests](#feature-requests)
- [Security](#security)
- [License](#license)

## Code of Conduct

We are committed to providing a welcoming and inclusive environment for all contributors. Please be respectful in all interactions and follow these guidelines:

- Be respectful of differing viewpoints and experiences
- Accept constructive criticism gracefully
- Focus on what is best for the community
- Show empathy towards other community members
- Do not engage in personal attacks or harassment

## How to Contribute

There are many ways to contribute to PotomacAnalyst:

### 🐛 Bug Reports
Help us identify and fix bugs by reporting issues you encounter.

### 🚀 Feature Requests
Suggest new features or improvements to existing functionality.

### 💻 Code Contributions
Submit pull requests with bug fixes, new features, or improvements.

### 📖 Documentation
Improve or expand the documentation.

### 🐛 Testing
Test new features and report any issues you find.

### 💬 Community Support
Help answer questions in discussions and issues.

## Development Setup

Before contributing code, please follow our [Development Guide](DEVELOPMENT.md) to set up your development environment.

### Prerequisites

- Visual Studio 2022 (Community, Professional, or Enterprise)
- Windows 10/11
- Git
- .NET 8.0 SDK

### Getting Started

1. **Fork the Repository**
   ```bash
   git clone https://github.com/your-username/PotomacAnalystWinUI3.git
   cd PotomacAnalystWinUI3
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Project**
   ```bash
   dotnet build
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

## Coding Standards

We follow strict coding standards to maintain code quality and consistency.

### C# Standards

#### Naming Conventions
- **Classes**: PascalCase (`DashboardViewModel`)
- **Methods**: PascalCase (`LoadDataAsync`)
- **Properties**: PascalCase (`UserName`)
- **Private Fields**: camelCase with underscore prefix (`_userName`)
- **Constants**: PascalCase (`MaxRetryCount`)
- **Interfaces**: PascalCase with "I" prefix (`IAuthService`)

#### Code Organization
- Use regions for logical grouping
- Keep methods focused and under 50 lines when possible
- Use meaningful names for variables, methods, and classes
- Follow SOLID principles

#### Async/Await Pattern
```csharp
// Good
public async Task<SomeResult> GetDataAsync(CancellationToken ct = default)
{
    try
    {
        return await _apiService.GetDataAsync(ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get data");
        throw;
    }
}

// Bad
public Task<SomeResult> GetDataAsync()
{
    return _apiService.GetDataAsync();
}
```

### XAML Standards

#### Resource Organization
- Use resource dictionaries for styles
- Follow naming conventions: `ControlTypeStyle` (e.g., `ButtonStyle`)
- Use appropriate key names

#### Data Binding
- Use `x:Bind` for better performance when possible
- Use `Mode=OneWay` for read-only data
- Use `Mode=TwoWay` for editable data
- Always specify the binding mode explicitly

#### Layout
- Use appropriate layout panels
- Consider responsive design
- Use margins and padding consistently
- Avoid hardcoded sizes when possible

### Code Comments

#### XML Documentation
```csharp
/// <summary>
/// Gets or sets the user's display name.
/// </summary>
/// <value>
/// The display name of the user.
/// </value>
/// <remarks>
/// This property is used throughout the application for user identification.
/// </remarks>
public string DisplayName { get; set; }
```

#### Inline Comments
```csharp
// Use a dictionary for O(1) lookup performance
private readonly Dictionary<string, Type> _pageMap = new();

// Validate input before processing to prevent errors
if (string.IsNullOrWhiteSpace(input))
    throw new ArgumentException("Input cannot be null or empty", nameof(input));
```

## Testing

We maintain high code quality through comprehensive testing.

### Test Structure
```
Tests/
├── UnitTests/              # Unit tests for business logic
├── IntegrationTests/       # Integration tests for services
└── UITests/               # UI automation tests
```

### Unit Testing Guidelines

#### Use AAA Pattern
```csharp
[Fact]
public void CalculateTotal_ShouldReturnCorrectTotal()
{
    // Arrange
    var calculator = new Calculator();
    var items = new List<decimal> { 10.00m, 20.00m, 5.00m };
    
    // Act
    var result = calculator.CalculateTotal(items);
    
    // Assert
    Assert.Equal(35.00m, result);
}
```

#### Mock Dependencies
```csharp
[Fact]
public async Task GetDataAsync_ShouldReturnData()
{
    // Arrange
    var mockService = new Mock<IApiService>();
    mockService.Setup(s => s.GetDataAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new SomeData());
    
    var viewModel = new SomeViewModel(mockService.Object);
    
    // Act
    await viewModel.LoadDataAsync();
    
    // Assert
    Assert.NotNull(viewModel.Data);
}
```

### Integration Testing
- Test service layer integration
- Use test databases or mock services
- Test authentication flows
- Test data persistence

### UI Testing
- Use WinAppDriver for UI automation
- Test navigation flows
- Test user interactions
- Test error scenarios

## Documentation

Good documentation is crucial for both users and developers.

### Code Documentation
- Use XML documentation for public APIs
- Include examples in documentation
- Keep documentation up-to-date with code changes

### README Updates
- Update README.md for major features
- Include usage examples
- Document breaking changes

### API Documentation
- Document all public APIs
- Include parameter descriptions
- Provide usage examples

## Issue Reporting

### Before Reporting an Issue

1. **Check Existing Issues**: Search to see if the issue has already been reported
2. **Check Documentation**: Review the documentation to see if it addresses your issue
3. **Update to Latest Version**: Ensure you're using the latest version

### How to Report an Issue

1. **Use the Issue Template**: Use the provided issue templates
2. **Provide Clear Description**: Describe the issue clearly and concisely
3. **Steps to Reproduce**: Include step-by-step instructions to reproduce the issue
4. **Expected vs Actual Behavior**: Clearly state what you expected vs what happened
5. **Environment Information**: Include OS, .NET version, and other relevant details
6. **Screenshots**: Include screenshots or videos when helpful

### Issue Template Example
```markdown
**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected behavior**
A clear and concise description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Environment (please complete the following information):**
 - OS: [e.g. Windows 11]
 - .NET Version: [e.g. 8.0]
 - Application Version: [e.g. 1.0.0]

**Additional context**
Add any other context about the problem here.
```

## Pull Request Process

### Before Creating a Pull Request

1. **Discuss First**: For significant changes, discuss in an issue first
2. **Check Existing PRs**: Ensure your change doesn't duplicate existing work
3. **Create Feature Branch**: Work on a feature branch, not main
4. **Write Tests**: Include tests for new functionality
5. **Update Documentation**: Update documentation as needed

### Creating a Pull Request

1. **Use the PR Template**: Fill out the pull request template
2. **Clear Title**: Use a clear, descriptive title
3. **Detailed Description**: Explain what the PR does and why
4. **Checklist**: Complete all checklist items
5. **Screenshots**: Include screenshots for UI changes

### PR Template
```markdown
## Summary
Brief description of the changes.

## Test plan
[ ] Unit tests pass
[ ] Integration tests pass
[ ] Manual testing completed
[ ] Documentation updated

## Test plan
Describe how you tested these changes.

## Documentation
- [ ] README updated (if applicable)
- [ ] API documentation updated (if applicable)
- [ ] Changelog updated (if applicable)

## Breaking Changes
- [ ] No breaking changes
- [ ] Breaking changes (list them here)

## Additional Notes
Add any additional information about the PR.
```

### Review Process

1. **Automated Checks**: All automated checks must pass
2. **Code Review**: At least one maintainer must approve
3. **Testing**: All tests must pass
4. **Documentation**: Documentation must be updated

### After Merging

- Your changes will be merged into the main branch
- You will be notified of the merge
- Changes will be included in the next release

## Feature Requests

### Submitting Feature Requests

1. **Create an Issue**: Use the feature request template
2. **Provide Details**: Include use cases, benefits, and implementation ideas
3. **Community Feedback**: Engage with community feedback
4. **Implementation**: Be prepared to implement the feature yourself

### Feature Request Template
```markdown
**Is your feature request related to a problem? Please describe.**
A clear and concise description of what the problem is. Ex. I'm always frustrated when [...]

**Describe the solution you'd like**
A clear and concise description of what you want to happen.

**Describe alternatives you've considered**
A clear and concise description of any alternative solutions or features you've considered.

**Additional context**
Add any other context or screenshots about the feature request here.
```

## Security

### Reporting Security Issues

If you discover a security vulnerability, please report it privately:

- **Email**: security@potomacanalyst.com
- **PGP Key**: Available on request

Do not report security issues in public issues.

### Security Best Practices

- Never commit secrets or API keys to the repository
- Use environment variables for sensitive configuration
- Follow secure coding practices
- Report security issues immediately

## License

By contributing to this project, you agree that your contributions will be licensed under the same license as the project (MIT License).

## Questions?

If you have questions about contributing, please:

- Check the [documentation](README.md)
- Search existing [issues](https://github.com/sohaibali73/PotomacAnalystWinUI3/issues)
- Join our [discussions](https://github.com/sohaibali73/PotomacAnalystWinUI3/discussions)
- Contact the maintainers

## Recognition

We appreciate all contributions and recognize contributors in our release notes. Major contributors may be invited to become maintainers.

Thank you for contributing to PotomacAnalyst! 🚀