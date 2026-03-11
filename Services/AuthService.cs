using PotomacAnalyst.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PotomacAnalyst.Services;

public class AuthService
{
    private readonly ApiService _api;
    private readonly SessionService _session;

    public AuthService(ApiService api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    // ── Login ──────────────────────────────────────────────────────────
    // POST /auth/login  → { access_token, token_type, user_id, email, expires_in }
    public async Task<(bool Success, string? Error)> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        try
        {
            var req = new LoginRequest { Email = email, Password = password };
            var tok = await _api.PostAsync<LoginRequest, TokenResponse>("/auth/login", req, ct);

            if (tok is null || string.IsNullOrEmpty(tok.AccessToken))
                return (false, "Invalid credentials. Please try again.");

            _session.SaveSession(tok.AccessToken, tok.Email, tok.UserId);
            _api.SetBearerToken(tok.AccessToken);
            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Register ───────────────────────────────────────────────────────
    // POST /auth/register → same response shape as login
    public async Task<(bool Success, string? Error)> RegisterAsync(
        RegisterRequest req, CancellationToken ct = default)
    {
        try
        {
            if (req.Password != req.ConfirmPassword)
                return (false, "Passwords do not match.");

            // Backend only accepts name/email/password — use a typed internal record
            // so we never accidentally serialise ConfirmPassword to the wire.
            var body = new RegisterBody(req.Email, req.Password, req.FullName);
            var tok = await _api.PostAsync<RegisterBody, TokenResponse>("/auth/register", body, ct);

            if (tok is null)
                return (false, "Registration failed. Please try again.");

            // If email confirmation is required the backend returns an empty access_token
            if (string.IsNullOrEmpty(tok.AccessToken))
                return (true, null);

            _session.SaveSession(tok.AccessToken, tok.Email, tok.UserId);
            _api.SetBearerToken(tok.AccessToken);
            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Forgot Password ────────────────────────────────────────────────
    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(
        string email, CancellationToken ct = default)
    {
        try
        {
            var req = new ForgotPasswordRequest { Email = email };
            await _api.PostAsync<ForgotPasswordRequest, object>("/auth/forgot-password", req, ct);
            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Get Profile ────────────────────────────────────────────────────
    // GET /auth/me → { id, email, name, nickname, is_admin, is_active, has_api_keys, created_at }
    public async Task<UserProfile?> GetProfileAsync(CancellationToken ct = default)
    {
        var token = _session.GetToken();
        if (string.IsNullOrEmpty(token)) return null;

        _api.SetBearerToken(token);

        try
        {
            var user = await _api.GetAsync<UserResponse>("/auth/me", ct);

            if (user is null || string.IsNullOrEmpty(user.Email))
            {
                // Fallback: build a minimal profile from persisted session data
                return new UserProfile
                {
                    Email = _session.GetEmail() ?? string.Empty,
                    FullName = string.Empty,
                    Name = string.Empty,
                };
            }

            return new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name ?? string.Empty,
                FullName = user.Name ?? string.Empty,
                Role = user.IsAdmin ? "Admin" : "Analyst",
                IsActive = user.IsActive,
            };
        }
        catch (HttpRequestException)
        {
            // Network / auth failure — return minimal cached profile so the UI
            // can still show something rather than hard-crashing.
            var email = _session.GetEmail() ?? string.Empty;
            return string.IsNullOrEmpty(email)
                ? null
                : new UserProfile { Email = email };
        }
        // Let unexpected exceptions (e.g. OperationCanceledException) propagate naturally.
    }

    // ── Update Profile / API Keys ──────────────────────────────────────
    // PUT /auth/me
    public async Task<(bool Success, string? Error)> UpdateProfileAsync(
        string? name, string? claudeKey, string? tavilyKey,
        CancellationToken ct = default)
    {
        try
        {
            var req = new UpdateProfileRequest
            {
                Name = name,
                ClaudeApiKey = claudeKey,
                TavilyApiKey = tavilyKey,
            };
            await _api.PutAsync<UpdateProfileRequest, object>("/auth/me", req, ct);
            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Logout ─────────────────────────────────────────────────────────
    public async Task LogoutAsync()
    {
        try { await _api.PostAsync<object, object>("/auth/logout", new { }); }
        catch { /* best-effort — always clear the local session */ }
        finally
        {
            _session.ClearSession();
            _api.ClearBearerToken();
        }
    }

    // ── Internal DTOs ──────────────────────────────────────────────────
    // Private record used only for the register wire payload.
    // Keeps ConfirmPassword off the network and avoids anonymous-object serialisation.
    private sealed record RegisterBody(string Email, string Password, string Name);
}