using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PotomacAnalyst.Services;

public class ApiService
{
    private readonly HttpClient _http;

    // Primary backend URL
    public string BaseUrl { get; } =
        "https://potomac-analyst-workbench-production.up.railway.app";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public ApiService(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri(BaseUrl);
        _http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    // ── Auth headers ──────────────────────────────────────────────────────
    public void SetBearerToken(string token)
        => _http.DefaultRequestHeaders.Authorization =
           new AuthenticationHeaderValue("Bearer", token);

    public void ClearBearerToken()
        => _http.DefaultRequestHeaders.Authorization = null;

    // ── HTTP helpers ───────────────────────────────────────────────────────
    public async Task<TResponse?> GetAsync<TResponse>(
        string path, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(path, ct);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOpts, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string path, TRequest body, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync(path, content, ct);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOpts, ct);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string path, TRequest body, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PutAsync(path, content, ct);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOpts, ct);
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string path, TRequest body, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PatchAsync(path, content, ct);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOpts, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync(path, ct);
        await EnsureSuccessAsync(resp);
    }

    /// <summary>Downloads a file from a GET endpoint as raw bytes.</summary>
    public async Task<byte[]?> GetBytesAsync(string path, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync(path, ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadAsByteArrayAsync(ct);
    }

    // ── Extract server error detail ────────────────────────────────────────
    private static async Task EnsureSuccessAsync(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        // Try to read the error detail from the response body (FastAPI returns {"detail": "..."})
        string? detail = null;
        try
        {
            var raw = await resp.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(raw))
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;
                if (root.TryGetProperty("detail", out var d)) detail = d.GetString();
                else if (root.TryGetProperty("message", out var m)) detail = m.GetString();
                else if (root.TryGetProperty("msg", out var g)) detail = g.GetString();
            }
        }
        catch { /* ignore parse errors, fall through to status-based message */ }

        var statusCode = (int)resp.StatusCode;
        var reason = resp.ReasonPhrase ?? resp.StatusCode.ToString();
        var msg = detail ?? $"HTTP {statusCode} {reason}";

        throw new HttpRequestException(msg, null, resp.StatusCode);
    }

    // ── Multipart file upload ─────────────────────────────────────────────
    public async Task<TResponse?> PostFileAsync<TResponse>(
        string path, byte[] bytes, string fileName, string field = "file",
        Dictionary<string, string>? extra = null,
        CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var fc = new ByteArrayContent(bytes);
        fc.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        form.Add(fc, field, fileName);
        if (extra is not null)
            foreach (var (k, v) in extra)
                form.Add(new StringContent(v), k);

        var resp = await _http.PostAsync(path, form, ct);
        await EnsureSuccessAsync(resp);
        return await resp.Content.ReadFromJsonAsync<TResponse>(JsonOpts, ct);
    }

    /// <summary>
    /// Posts JSON and returns the raw response bytes (e.g. for file downloads).
    /// Throws <see cref="HttpRequestException"/> on non-success status codes,
    /// consistent with all other methods on this service.
    /// </summary>
    public async Task<byte[]> PostBytesAsync<TRequest>(
        string path, TRequest body, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync(path, content, ct);
        await EnsureSuccessAsync(resp);   // throws on non-2xx — consistent with other helpers
        return await resp.Content.ReadAsByteArrayAsync(ct);
    }

    // ── Streaming (Vercel AI SDK Data Stream Protocol) ───────────────────
    // Type codes: '0'=text, '3'=error, '8'=metadata/data, '9'=tool call, 'a'=tool result, 'd'=finish
    public async Task StreamAsync<TRequest>(
        string path, TRequest body,
        Action<string>? onText = null,
        Action<string>? onError = null,
        Action<string>? onConversationId = null,
        Action<string>? onToolCall = null,   // toolName
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(body, JsonOpts);
        var req = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var resp = await _http.SendAsync(
            req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (resp.Headers.TryGetValues("X-Conversation-Id", out var ids))
            onConversationId?.Invoke(string.Join("", ids));

        if (!resp.IsSuccessStatusCode)
        {
            onError?.Invoke($"HTTP {(int)resp.StatusCode}");
            return;
        }

        using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            // ConfigureAwait(false) is CRITICAL: it breaks the UI SynchronizationContext so the
            // read loop runs on a background thread, keeping the UI thread free for DispatcherTimer ticks.
            var line = await reader.ReadLineAsync(ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(line) || line.Length < 2) continue;

            var typeCode = line[0];
            var payload = line[2..];   // range syntax — cleaner than Substring(2)

            try
            {
                switch (typeCode)
                {
                    case '0': // Text chunk
                        var text = JsonSerializer.Deserialize<string>(payload);
                        if (!string.IsNullOrEmpty(text)) onText?.Invoke(text);
                        break;

                    case '3': // Error
                        var err = JsonSerializer.Deserialize<string>(payload) ?? "Stream error";
                        onError?.Invoke(err);
                        break;

                    case '8': // Metadata / Data — may carry conversationId
                        try
                        {
                            using var doc8 = JsonDocument.Parse(payload);
                            var root = doc8.RootElement;
                            // Format A: {"conversationId":"..."}
                            if (root.ValueKind == JsonValueKind.Object &&
                                root.TryGetProperty("conversationId", out var cid))
                                onConversationId?.Invoke(cid.GetString() ?? string.Empty);
                            // Format B: [{"conversationId":"..."}]
                            else if (root.ValueKind == JsonValueKind.Array)
                                foreach (var item in root.EnumerateArray())
                                    if (item.TryGetProperty("conversationId", out var cid2))
                                        onConversationId?.Invoke(cid2.GetString() ?? string.Empty);
                        }
                        catch { }
                        break;

                    case '9': // Tool call start
                        try
                        {
                            using var doc9 = JsonDocument.Parse(payload);
                            var toolName = doc9.RootElement.TryGetProperty("toolName", out var tn)
                                ? tn.GetString() ?? "tool"
                                : "tool";
                            onToolCall?.Invoke(toolName);
                        }
                        catch { }
                        break;

                        // 'a' = tool result, 'd' = finish — no action needed
                }
            }
            catch { }
        }
    }

    // ── Health ────────────────────────────────────────────────────────────
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try { return (await _http.GetAsync("/health", ct)).IsSuccessStatusCode; }
        catch { return false; }
    }
}