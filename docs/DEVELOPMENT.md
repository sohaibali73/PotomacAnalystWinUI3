# PotomacAnalyst Development Guide

This guide provides comprehensive information for developers contributing to the PotomacAnalyst project.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Development Environment Setup](#development-environment-setup)
3. [Project Architecture](#project-architecture)
4. [Development Workflow](#development-workflow)
5. [Module Development](#module-development)
6. [API Development](#api-development)
7. [UI Development](#ui-development)
8. [Testing](#testing)
9. [Debugging](#debugging)
10. [Performance Optimization](#performance-optimization)
11. [Security Considerations](#security-considerations)
12. [Deployment](#deployment)
13. [Contributing Guidelines](#contributing-guidelines)

## Getting Started

### Prerequisites

Before you begin developing PotomacAnalyst, ensure you have the following installed:

- **Visual Studio 2022** (Community Edition or higher)
- **.NET 8.0 SDK**
- **Windows App SDK 1.5** or later
- **Git** for version control
- **Windows 10 version 1809** or **Windows 11**

### Quick Start

1. **Clone the Repository**
   ```bash
   git clone https://github.com/sohaibali73/PotomacAnalystWinUI3.git
   cd PotomacAnalystWinUI3
   ```

2. **Open in Visual Studio**
   ```bash
   # Open the solution file
   PotomacAnalyst.sln
   ```

3. **Build the Solution**
   - In Visual Studio: `Build > Build Solution` (Ctrl+Shift+B)
   - Or from command line: `dotnet build`

4. **Run the Application**
   - In Visual Studio: `Debug > Start Debugging` (F5)
   - Or from command line: `dotnet run --project PotomacAnalyst`

## Development Environment Setup

### Visual Studio Configuration

#### Required Extensions
Install these Visual Studio extensions for optimal development experience:

- **ReSharper** (optional but recommended)
- **XAML Styler** for XAML formatting
- **GitLens** for enhanced Git integration
- **C# Extensions** for additional C# tooling

#### Project Templates
Install WinUI 3 project templates:
```bash
dotnet new install WinUI.Template.CSharp
```

#### Debugging Setup
1. **Enable .NET Framework Source Stepping**
   - Go to `Tools > Options > Debugging > General`
   - Check "Enable .NET Framework source stepping"

2. **Configure Exception Settings**
   - Open `Debug > Windows > Exception Settings`
   - Enable common exception types for better debugging

### Development Tools

#### Code Analysis
- **Roslyn Analyzers**: Built-in code analysis
- **StyleCop**: Code style enforcement
- **SonarQube**: Quality gate integration (optional)

#### Package Management
- **NuGet**: Primary package manager
- **PackageReference**: Modern package management format
- **Central Package Management**: For solution-wide package versions

#### Build Tools
- **MSBuild**: Primary build system
- **MSBuild SDK Extras**: Additional build targets
- **ILRepack**: Assembly merging (if needed)

## Project Architecture

### Architecture Overview

PotomacAnalyst follows a layered architecture with the following key principles:

- **MVVM Pattern**: Model-View-ViewModel for separation of concerns
- **Dependency Injection**: Using Microsoft.Extensions.DependencyInjection
- **Modular Design**: Each module is self-contained
- **Async/Await**: Consistent async programming model
- **Event-Driven**: Using MediatR for communication between modules

### Project Structure

```
PotomacAnalyst/
├── PotomacAnalyst/              # Main application project
│   ├── App.xaml                 # Application entry point
│   ├── App.xaml.cs              # Application logic
│   ├── Views/                   # XAML views
│   ├── ViewModels/              # View models
│   ├── Services/                # Business services
│   ├── Models/                  # Data models
│   ├── Helpers/                 # Utility classes
│   └── Resources/               # App resources
├── PotomacAnalyst.Core/         # Core library
│   ├── Services/                # Core services
│   ├── Models/                  # Core models
│   ├── Extensions/              # Extension methods
│   └── Constants/               # Application constants
├── PotomacAnalyst.Tests/        # Unit tests
├── PotomacAnalyst.IntegrationTests/ # Integration tests
└── docs/                        # Documentation
```

### Key Architectural Patterns

#### MVVM (Model-View-ViewModel)
```csharp
// Model
public class UserProfile
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
}

// ViewModel
public class UserProfileViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private UserProfile _user;
    
    public UserProfileViewModel(IUserService userService)
    {
        _userService = userService;
        LoadUserCommand = new RelayCommand(async () => await LoadUserAsync());
    }
    
    public UserProfile User
    {
        get => _user;
        set => SetProperty(ref _user, value);
    }
    
    public IRelayCommand LoadUserCommand { get; }
    
    private async Task LoadUserAsync()
    {
        User = await _userService.GetCurrentUserAsync();
    }
}

// View (XAML)
<TextBlock Text="{x:Bind ViewModel.User.DisplayName, Mode=OneWay}" />
<Button Command="{x:Bind ViewModel.LoadUserCommand}" Content="Load User" />
```

#### Dependency Injection
```csharp
// In App.xaml.cs
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    var services = ConfigureServices();
    var serviceProvider = services.BuildServiceProvider();
    
    // Register services
    services.AddSingleton<IUserService, UserService>();
    services.AddSingleton<INavigationService, NavigationService>();
    services.AddTransient<DashboardViewModel>();
    
    // Resolve main window
    var mainWindow = serviceProvider.GetService<MainWindow>();
    mainWindow.Activate();
}

private IServiceCollection ConfigureServices()
{
    var services = new ServiceCollection();
    
    // Configure services
    services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
    
    return services;
}
```

#### Event-Driven Architecture
```csharp
// Event definition
public record UserLoggedInEvent(Guid UserId, string Email);

// Event handler
public class UserLoggedInHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly ILogger<UserLoggedInHandler> _logger;
    
    public UserLoggedInHandler(ILogger<UserLoggedInHandler> logger)
    {
        _logger = logger;
    }
    
    public Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User {UserId} logged in", notification.UserId);
        // Handle the event
        return Task.CompletedTask;
    }
}

// Publishing events
await _mediator.Publish(new UserLoggedInEvent(userId, email), cancellationToken);
```

## Development Workflow

### Git Workflow

PotomacAnalyst uses a feature branch workflow:

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Changes**
   - Follow coding standards
   - Write tests for new functionality
   - Update documentation as needed

3. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: add user authentication module
   
   - Implement login/logout functionality
   - Add JWT token management
   - Include unit tests for authentication service
   
   Closes #123"
   ```

4. **Push to Remote**
   ```bash
   git push -u origin feature/your-feature-name
   ```

5. **Create Pull Request**
   - Use the GitHub web interface
   - Fill out the PR template
   - Request reviews from maintainers

### Code Review Process

1. **Automated Checks**
   - All tests must pass
   - Code coverage must meet minimum requirements
   - Static analysis must pass

2. **Manual Review**
   - Code quality and style
   - Architecture and design patterns
   - Security considerations
   - Performance implications

3. **Merge Requirements**
   - At least one approval from maintainers
   - All automated checks passing
   - No merge conflicts

### Development Best Practices

#### Code Organization
- Use meaningful names for classes, methods, and variables
- Keep methods focused and small (Single Responsibility Principle)
- Use regions to organize large classes
- Follow the established project structure

#### Error Handling
- Use structured exception handling
- Log errors appropriately
- Provide meaningful error messages
- Implement graceful degradation

#### Performance
- Use async/await for I/O operations
- Implement caching where appropriate
- Avoid blocking calls on the UI thread
- Monitor memory usage and prevent leaks

## Module Development

### Creating New Modules

Each module in PotomacAnalyst should follow this structure:

```
Modules/
└── YourModuleName/
    ├── Views/
    │   ├── YourModulePage.xaml
    │   ├── YourModulePage.xaml.cs
    │   └── Controls/
    │       └── YourCustomControl.xaml
    ├── ViewModels/
    │   ├── YourModuleViewModel.cs
    │   └── YourModuleSettingsViewModel.cs
    ├── Services/
    │   ├── IYourModuleService.cs
    │   └── YourModuleService.cs
    ├── Models/
    │   ├── YourModuleData.cs
    │   └── YourModuleSettings.cs
    └── Resources/
        ├── Strings/
        └── Styles/
```

### Module Registration

1. **Register Services**
   ```csharp
   // In App.xaml.cs or module initializer
   services.AddSingleton<IYourModuleService, YourModuleService>();
   services.AddTransient<YourModuleViewModel>();
   ```

2. **Add Navigation**
   ```csharp
   // In NavigationService or similar
   public void RegisterYourModule()
   {
       _navigationItems.Add(new NavigationItem
       {
           Name = "Your Module",
           Icon = "\uE945", // Unicode icon
           PageType = typeof(YourModulePage)
       });
   }
   ```

3. **Add to Main Interface**
   ```csharp
   // In MainWindow or similar
   private void InitializeModules()
   {
       // Add your module to the main interface
       ModuleManager.RegisterModule(new YourModule());
   }
   ```

### Module Communication

Use the event-driven architecture for module communication:

```csharp
// Publishing events from your module
public class YourModuleService
{
    private readonly IMediator _mediator;
    
    public YourModuleService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task ProcessDataAsync(DataModel data)
    {
        // Process data
        await _mediator.Publish(new DataProcessedEvent(data), CancellationToken.None);
    }
}

// Handling events in your module
public class YourModuleHandler : INotificationHandler<DataProcessedEvent>
{
    public Task Handle(DataProcessedEvent notification, CancellationToken cancellationToken)
    {
        // Handle the event
        return Task.CompletedTask;
    }
}
```

## API Development

### REST API Guidelines

When developing APIs for PotomacAnalyst:

#### Endpoint Design
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserAsync(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return user is not null ? Ok(user) : NotFound();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<User>> CreateUserAsync([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user);
    }
}
```

#### Error Handling
```csharp
public class ApiExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    
    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }
    
    public override void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled API exception");
        
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred",
            Detail = "Please contact support if this persists"
        };
        
        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        
        context.ExceptionHandled = true;
    }
}
```

#### Authentication and Authorization
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SecureController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<ActionResult<IEnumerable<Data>>> GetDataAsync()
    {
        // Implementation
    }
    
    [HttpPost]
    [Authorize(Policy = "RequirePremiumSubscription")]
    public async Task<ActionResult> CreateDataAsync([FromBody] Data data)
    {
        // Implementation
    }
}
```

### GraphQL API (Optional)

For complex data requirements, consider implementing GraphQL:

```csharp
public class UserGraphType : ObjectGraphType<User>
{
    public UserGraphType()
    {
        Field(x => x.Id);
        Field(x => x.Email);
        Field(x => x.DisplayName);
        Field<ListGraphType<PostGraphType>>("posts");
    }
}

public class Query : ObjectGraphType
{
    public Query(IUserService userService)
    {
        Field<UserGraphType>(
            "user",
            arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" }),
            resolve: context =>
            {
                var id = context.GetArgument<Guid>("id");
                return userService.GetUserByIdAsync(id);
            });
    }
}
```

## UI Development

### XAML Best Practices

#### Layout and Structure
```xml
<!-- Use proper layout panels -->
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>
    
    <!-- Header -->
    <StackPanel Grid.Row="0" Orientation="Horizontal">
        <TextBlock Text="Title" Style="{StaticResource TitleTextBlockStyle}" />
        <Button Content="Action" Command="{x:Bind ViewModel.ActionCommand}" />
    </StackPanel>
    
    <!-- Content -->
    <ScrollViewer Grid.Row="1">
        <ItemsControl ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}">
            <ItemsControl.ItemTemplate>
                <DataTemplate x:DataType="local:ItemViewModel">
                    <local:ItemControl Item="{x:Bind}" />
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </ScrollViewer>
    
    <!-- Footer -->
    <StackPanel Grid.Row="2" Orientation="Horizontal">
        <TextBlock Text="{x:Bind ViewModel.Status, Mode=OneWay}" />
    </StackPanel>
</Grid>
```

#### Styling and Theming
```xml
<!-- In App.xaml or dedicated resource dictionary -->
<ResourceDictionary>
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#0078D4</Color>
    <Color x:Key="SecondaryColor">#107C10</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}" />
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}" />
    
    <!-- Styles -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}" />
        <Setter Property="Foreground" Value="White" />
        <Setter Property="CornerRadius" Value="4" />
        <Setter Property="Padding" Value="12,8" />
    </Style>
    
    <!-- Templates -->
    <ControlTemplate x:Key="CustomControlTemplate" TargetType="local:CustomControl">
        <!-- Template implementation -->
    </ControlTemplate>
</ResourceDictionary>
```

#### Data Binding
```xml
<!-- One-way binding for read-only data -->
<TextBlock Text="{x:Bind ViewModel.DisplayName, Mode=OneWay}" />

<!-- Two-way binding for editable data -->
<TextBox Text="{x:Bind ViewModel.UserName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- Binding with converters -->
<TextBlock Text="{x:Bind ViewModel.CreationDate, Mode=OneWay, Converter={StaticResource DateTimeConverter}}" />

<!-- Binding with validation -->
<TextBox
    Text="{x:Bind ViewModel.Email, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    PlaceholderText="Enter email address">
    <Interactivity:Interaction.Behaviors>
        <Core:DataValidationBehavior Binding="{x:Bind ViewModel.Email}" />
    </Interactivity:Interaction.Behaviors>
</TextBox>
```

### Custom Controls

#### Creating Custom Controls
```csharp
[TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
public class CustomControl : Control
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(CustomControl),
            new PropertyMetadata(default(string)));
    
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public CustomControl()
    {
        DefaultStyleKey = typeof(CustomControl);
    }
    
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        // Initialize template parts
        var contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
        // Configure control
    }
}
```

#### Control Template
```xml
<!-- In Generic.xaml -->
<Style TargetType="local:CustomControl">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="local:CustomControl">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <Grid>
                        <TextBlock Text="{TemplateBinding Header}"
                                   Style="{StaticResource HeaderTextStyle}"
                                   Margin="8,4" />
                        <ContentPresenter x:Name="PART_ContentPresenter"
                                          Margin="8"
                                          VerticalAlignment="Center" />
                    </Grid>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

## Testing

### Unit Testing

#### Test Structure
```csharp
public class UserServiceTests
{
    private Mock<IUserRepository> _mockRepository;
    private Mock<ILogger<UserService>> _mockLogger;
    private UserService _userService;
    
    [Fact]
    public async Task GetUserById_ValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Name = "Test User" };
        
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
                       .ReturnsAsync(expectedUser);
        
        // Act
        var result = await _userService.GetUserByIdAsync(userId);
        
        // Assert
        Assert.Equal(expectedUser, result);
    }
    
    [Fact]
    public async Task GetUserById_InvalidId_ThrowsException()
    {
        // Arrange
        var invalidId = Guid.Empty;
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.GetUserByIdAsync(invalidId));
    }
}
```

#### Test Data Builders
```csharp
public static class TestDataBuilder
{
    public static User BuildUser(Action<User> configure = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow
        };
        
        configure?.Invoke(user);
        return user;
    }
    
    public static List<User> BuildUsers(int count)
    {
        return Enumerable.Range(1, count)
                       .Select(i => BuildUser(u => u.DisplayName = $"User {i}"))
                       .ToList();
    }
}
```

### Integration Testing

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
    public async Task SaveAndRetrieveUser_WorksCorrectly()
    {
        // Arrange
        var user = TestDataBuilder.BuildUser();
        var repository = new UserRepository(_fixture.ConnectionString);
        
        // Act
        await repository.SaveAsync(user);
        var retrievedUser = await repository.GetByIdAsync(user.Id);
        
        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Email, retrievedUser.Email);
    }
}
```

### UI Testing

#### WinAppDriver Tests
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
}
```

## Debugging

### Debugging Tools

#### Visual Studio Debugger
- **Breakpoints**: Set conditional and function breakpoints
- **Watch Windows**: Monitor variable values
- **Call Stack**: Analyze execution flow
- **Immediate Window**: Execute code during debugging

#### Diagnostic Tools
```csharp
// Logging
private readonly ILogger<YourClass> _logger;

public YourClass(ILogger<YourClass> logger)
{
    _logger = logger;
}

public async Task YourMethodAsync()
{
    _logger.LogInformation("Starting method execution");
    
    try
    {
        // Your code here
        _logger.LogInformation("Method completed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in method execution");
        throw;
    }
}
```

#### Performance Profiling
- **CPU Usage**: Identify performance bottlenecks
- **Memory Usage**: Detect memory leaks
- **UI Responsiveness**: Monitor UI thread performance
- **Network Activity**: Analyze API calls and responses

### Common Debugging Scenarios

#### XAML Binding Issues
```xml
<!-- Enable binding diagnostics -->
<TextBlock Text="{x:Bind ViewModel.Property, Mode=OneWay, diag:PresentationTraceSources.TraceLevel=High}" />
```

#### Async/Await Deadlocks
```csharp
// Bad - can cause deadlocks
public async Task GetDataAsync()
{
    var result = await GetDataFromApiAsync();
    await ProcessDataAsync(result).ConfigureAwait(false); // Use ConfigureAwait(false)
}

// Good - prevents deadlocks
public async Task GetDataAsync()
{
    var result = await GetDataFromApiAsync().ConfigureAwait(false);
    await ProcessDataAsync(result).ConfigureAwait(false);
}
```

#### Memory Leaks
```csharp
// Implement IDisposable for classes with unmanaged resources
public class ResourceManager : IDisposable
{
    private bool _disposed = false;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            
            // Dispose unmanaged resources
            
            _disposed = true;
        }
    }
}
```

## Performance Optimization

### UI Performance

#### Virtualization
```xml
<!-- Enable virtualization for long lists -->
<ListView ItemsSource="{x:Bind ViewModel.Items}"
          VirtualizingStackPanel.VirtualizationMode="Recycling">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:ItemViewModel">
            <local:ItemControl />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

#### Image Optimization
```xml
<!-- Optimize image loading -->
<Image Source="{x:Bind ViewModel.ImageUrl, Mode=OneWay}"
       Stretch="UniformToFill"
       CacheMode="BitmapCache">
    <Image.LoadingBehavior>
        <MediaLoadingBehavior IsAnimated="False" />
    </Image.LoadingBehavior>
</Image>
```

#### Data Binding Optimization
```csharp
// Use ObservableCollection for dynamic collections
public ObservableCollection<ItemViewModel> Items { get; } = new();

// Implement INotifyPropertyChanged for property changes
public class ItemViewModel : ObservableObject
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

### Application Performance

#### Async Patterns
```csharp
// Use async streams for large datasets
public async IAsyncEnumerable<DataItem> GetDataAsync([EnumeratorCancellation] CancellationToken ct)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(ct);
    
    using var command = new SqlCommand("SELECT * FROM Data", connection);
    using var reader = await command.ExecuteReaderAsync(ct);
    
    while (await reader.ReadAsync(ct))
    {
        yield return new DataItem
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1)
        };
    }
}
```

#### Caching
```csharp
public class CachedDataService : IDataService
{
    private readonly IMemoryCache _cache;
    private readonly IDataService _innerService;
    
    public CachedDataService(IMemoryCache cache, IDataService innerService)
    {
        _cache = cache;
        _innerService = innerService;
    }
    
    public async Task<Data> GetDataAsync(string key)
    {
        var cacheKey = $"data_{key}";
        
        if (_cache.TryGetValue(cacheKey, out Data cachedData))
        {
            return cachedData;
        }
        
        var data = await _innerService.GetDataAsync(key);
        
        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));
        
        return data;
    }
}
```

#### Resource Management
```csharp
// Use object pooling for frequently created objects
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentBag<T> _objects = new();
    
    public T Get()
    {
        return _objects.TryTake(out T item) ? item : new T();
    }
    
    public void Return(T item)
    {
        _objects.Add(item);
    }
}
```

## Security Considerations

### Authentication and Authorization

#### Secure Authentication
```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    
    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", email);
            return AuthenticationResult.Failed("Invalid credentials");
        }
        
        var token = _tokenService.GenerateToken(user);
        return AuthenticationResult.Success(user, token);
    }
}
```

#### Authorization Policies
```csharp
public class PremiumSubscriptionRequirement : IAuthorizationRequirement
{
    // Implementation
}

public class PremiumSubscriptionHandler : AuthorizationHandler<PremiumSubscriptionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PremiumSubscriptionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "SubscriptionType" && c.Value == "Premium"))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

### Data Protection

#### Encryption
```csharp
public class DataProtector : IDataProtector
{
    private readonly IDataProtectionProvider _provider;
    
    public DataProtector(IDataProtectionProvider provider)
    {
        _provider = provider;
    }
    
    public string Protect(string data)
    {
        var protector = _provider.CreateProtector("PotomacAnalyst.Data");
        return protector.Protect(data);
    }
    
    public string Unprotect(string protectedData)
    {
        var protector = _provider.CreateProtector("PotomacAnalyst.Data");
        return protector.Unprotect(protectedData);
    }
}
```

#### Input Validation
```csharp
public class InputValidator : IInputValidator
{
    public ValidationResult ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationResult.Failed("Email cannot be empty");
        
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return ValidationResult.Failed("Invalid email format");
        
        return ValidationResult.Success();
    }
    
    public ValidationResult ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return ValidationResult.Failed("Password cannot be empty");
        
        if (password.Length < 8)
            return ValidationResult.Failed("Password must be at least 8 characters long");
        
        if (!password.Any(char.IsUpper))
            return ValidationResult.Failed("Password must contain at least one uppercase letter");
        
        return ValidationResult.Success();
    }
}
```

## Deployment

### Build Configuration

#### Release Build
```bash
# Build for release
dotnet build -c Release

# Publish for specific runtime
dotnet publish -c Release -r win-x64 --self-contained
```

#### MSIX Packaging
```xml
<!-- Package.appxmanifest -->
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" 
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         IgnorableNamespaces="uap mp">
  <Identity Name="PotomacAnalyst" Publisher="CN=YourCompany" Version="1.0.0.0" />
  <Properties>
    <DisplayName>PotomacAnalyst</DisplayName>
    <PublisherDisplayName>Your Company</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22621.0" />
  </Dependencies>
</Package>
```

### CI/CD Pipeline

#### GitHub Actions
```yaml
name: Build and Deploy
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
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
      run: dotnet build --no-restore --configuration Release
    
    - name: Run tests
      run: dotnet test --no-build --configuration Release
    
    - name: Publish
      run: dotnet publish --configuration Release --output ./publish
    
    - name: Create MSIX
      run: |
        # MSIX creation steps
        # Use MSBuild or other tools to create the package
```

### Distribution

#### Microsoft Store
1. **Create Developer Account**: Register as a Microsoft developer
2. **Prepare Assets**: Create store listing assets (screenshots, descriptions)
3. **Submit Package**: Upload MSIX package through Partner Center
4. **Certification**: Pass Microsoft Store certification requirements

#### Direct Distribution
1. **Code Signing**: Sign the application with a valid certificate
2. **Installer Creation**: Create installer packages (MSI, EXE)
3. **Distribution Channels**: Website, enterprise deployment tools

## Contributing Guidelines

### Code of Conduct
- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help others learn and grow

### Contribution Process
1. **Fork the Repository**: Create your own fork of the project
2. **Create Feature Branch**: Use descriptive branch names
3. **Make Changes**: Follow coding standards and best practices
4. **Write Tests**: Include appropriate tests for new functionality
5. **Update Documentation**: Keep documentation up to date
6. **Submit Pull Request**: Use the provided template

### Code Review Guidelines
- **Be Thorough**: Review all changes, not just the obvious ones
- **Be Constructive**: Provide helpful feedback and suggestions
- **Be Timely**: Review pull requests promptly
- **Be Consistent**: Apply the same standards to all contributions

### Issue Management
- **Use Templates**: Fill out issue templates completely
- **Provide Reproduction Steps**: Include steps to reproduce bugs
- **Use Labels**: Apply appropriate labels for categorization
- **Follow Up**: Respond to questions and updates

This development guide provides the foundation for contributing to PotomacAnalyst. Always refer to the latest documentation and follow the established patterns and practices for the best development experience.