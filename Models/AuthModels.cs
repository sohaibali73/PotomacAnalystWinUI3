namespace PotomacAnalyst.Models;

// ── Login ──────────────────────────────────────────────
public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    // Backend returns snake_case: access_token, token_type
    public string  AccessToken  { get; set; } = string.Empty;
    public string  TokenType    { get; set; } = "bearer";
    public int     ExpiresIn    { get; set; }
    public string? RefreshToken { get; set; }

    // Some backends embed user data in the login response
    public UserProfile? User { get; set; }
}

// ── Register ───────────────────────────────────────────
public class RegisterRequest
{
    public string FullName        { get; set; } = string.Empty;
    public string Email           { get; set; } = string.Empty;
    public string Password        { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RegisterResponse
{
    public bool    Success { get; set; }
    public string? Message { get; set; }
    public string? UserId  { get; set; }
}

// ── Forgot Password ────────────────────────────────────
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponse
{
    public bool    Success { get; set; }
    public string? Message { get; set; }
}

// ── User Profile ───────────────────────────────────────
// Handles both "name" (web API) and "full_name" via CaseInsensitive deserialization
public class UserProfile
{
    public string  Id       { get; set; } = string.Empty;
    public string  Email    { get; set; } = string.Empty;

    // Backend may return "name" or "full_name" — we capture both
    public string  Name     { get; set; } = string.Empty;
    public string  FullName { get; set; } = string.Empty;

    public string? Role      { get; set; }
    public string? AvatarUrl { get; set; }
    public bool    IsActive  { get; set; } = true;

    // Computed: return whichever name field is populated
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(FullName) ? FullName :
        !string.IsNullOrWhiteSpace(Name)     ? Name :
        Email.Contains('@') ? Email.Split('@')[0] : "User";
}

// ── API Key Settings ───────────────────────────────────
public class ApiKeySettings
{
    public string? ClaudeApiKey  { get; set; }
    public string? TavilyApiKey  { get; set; }
    public string? OpenAiApiKey  { get; set; }
}
