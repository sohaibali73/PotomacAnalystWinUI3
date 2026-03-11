using System;
using System.Collections.Generic;
using Windows.Storage;

namespace PotomacAnalyst.Services;

/// <summary>
/// Persists the auth session to <see cref="ApplicationData.Current.LocalSettings"/>
/// for packaged WinUI 3 apps.  Falls back to an in-memory dictionary when the app
/// runs unpackaged (no package identity), so nothing silently breaks.
/// </summary>
public class SessionService
{
    private const string KeyToken = "auth_token";
    private const string KeyEmail = "auth_email";
    private const string KeyUserId = "auth_user_id";

    // Non-null when the app is packaged and LocalSettings is available.
    private readonly ApplicationDataContainer? _settings;

    // In-memory fallback used when LocalSettings is unavailable (unpackaged scenario).
    // Session survives the process lifetime only, which is the correct behaviour for
    // an unpackaged app that has no secure persistent storage.
    private readonly Dictionary<string, string> _memory = new(3);

    public SessionService()
    {
        try
        {
            _settings = ApplicationData.Current.LocalSettings;
        }
        catch (Exception ex)
        {
            // ApplicationData.Current throws InvalidOperationException when there is
            // no package identity (e.g. running as an unpackaged exe during development).
            System.Diagnostics.Debug.WriteLine($"[Session] LocalSettings unavailable — using in-memory fallback: {ex.Message}");
        }
    }

    // ── Write ──────────────────────────────────────────────────────────
    public void SaveSession(string token, string email, string userId = "")
    {
        Write(KeyToken, token);
        Write(KeyEmail, email);
        Write(KeyUserId, userId);
    }

    public void ClearSession()
    {
        Delete(KeyToken);
        Delete(KeyEmail);
        Delete(KeyUserId);
    }

    // ── Read ───────────────────────────────────────────────────────────
    public string? GetToken() => Read(KeyToken);
    public string? GetEmail() => Read(KeyEmail);
    public string? GetUserId() => Read(KeyUserId);

    public bool IsAuthenticated() => !string.IsNullOrWhiteSpace(GetToken());

    // ── Private helpers ────────────────────────────────────────────────
    private void Write(string key, string value)
    {
        if (_settings is not null)
        {
            try { _settings.Values[key] = value; return; }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Session] Write failed for '{key}': {ex.Message}");
            }
        }
        _memory[key] = value;
    }

    private string? Read(string key)
    {
        if (_settings is not null)
        {
            try { return _settings.Values.TryGetValue(key, out var v) ? v as string : null; }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Session] Read failed for '{key}': {ex.Message}");
            }
        }
        return _memory.TryGetValue(key, out var m) ? m : null;
    }

    private void Delete(string key)
    {
        if (_settings is not null)
        {
            try { _settings.Values.Remove(key); return; }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Session] Delete failed for '{key}': {ex.Message}");
            }
        }
        _memory.Remove(key);
    }
}