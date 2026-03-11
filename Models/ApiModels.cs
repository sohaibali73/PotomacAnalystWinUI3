// ── All API models matching the Potomac Analyst Workbench backend ────────
// NOTE: Classes are prefixed with "Api" to avoid conflicts with WinUI3/Windows Runtime types

namespace PotomacAnalyst.Models;

// ── Auth ──────────────────────────────────────────────────────────────

/// <summary>POST /auth/login or /auth/register response</summary>
public class TokenResponse
{
    public string  AccessToken  { get; set; } = string.Empty;
    public string  TokenType    { get; set; } = "bearer";
    public string  UserId       { get; set; } = string.Empty;
    public string  Email        { get; set; } = string.Empty;
    public int     ExpiresIn    { get; set; } = 3600;
}

/// <summary>GET /auth/me response</summary>
public class UserResponse
{
    public string   Id          { get; set; } = string.Empty;
    public string   Email       { get; set; } = string.Empty;
    public string?  Name        { get; set; }
    public string?  Nickname    { get; set; }
    public bool     IsAdmin     { get; set; }
    public bool     IsActive    { get; set; } = true;
    public bool     HasApiKeys  { get; set; }
    public string?  CreatedAt   { get; set; }

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Name)     ? Name :
        !string.IsNullOrWhiteSpace(Nickname) ? Nickname :
        Email.Contains('@') ? Email.Split('@')[0] : "User";
}

public class UpdateProfileRequest
{
    public string? Name           { get; set; }
    public string? Nickname       { get; set; }
    public string? ClaudeApiKey   { get; set; }
    public string? TavilyApiKey   { get; set; }
}

// ── Chat ──────────────────────────────────────────────────────────────

public class ApiConversation
{
    public string   Id               { get; set; } = string.Empty;
    public string   Title            { get; set; } = "New Conversation";
    public string?  ConversationType { get; set; } = "agent";
    public string?  CreatedAt        { get; set; }
    public string?  UpdatedAt        { get; set; }
    public int      MessageCount     { get; set; }
}

public class CreateConversationRequest
{
    public string Title            { get; set; } = "New Conversation";
    public string ConversationType { get; set; } = "agent";
}

public class ApiChatMessage
{
    public string  Id             { get; set; } = string.Empty;
    public string  ConversationId { get; set; } = string.Empty;
    public string  Role           { get; set; } = "user";
    public string  Content        { get; set; } = string.Empty;
    public string? CreatedAt      { get; set; }
}

public class SendMessageRequest
{
    public string  Content        { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

/// <summary>Non-streaming fallback response from POST /chat/message</summary>
public class StreamResponse
{
    public string  ConversationId { get; set; } = string.Empty;
    public string  Response       { get; set; } = string.Empty;
    public string? ToolsUsed      { get; set; }
}

// ── AFL ───────────────────────────────────────────────────────────────

public class AflGenerateRequest
{
    public string  Prompt        { get; set; } = string.Empty;
    public string? StrategyType  { get; set; }
    public AflSettings? Settings { get; set; }
}

public class AflSettings
{
    public string? Timeframe     { get; set; } = "Daily";
    public string? Indicators    { get; set; }
    public int     PositionSize  { get; set; } = 10;
    public int     StopLoss      { get; set; } = 5;
}

public class AflCode
{
    public string   Id          { get; set; } = string.Empty;
    public string   Code        { get; set; } = string.Empty;
    public string?  Explanation { get; set; }
    public string?  Title       { get; set; }
    public string?  CreatedAt   { get; set; }
}

// ── Knowledge Base (Brain) ────────────────────────────────────────────

public class ApiDocument
{
    public string   Id          { get; set; } = string.Empty;
    public string   Title       { get; set; } = string.Empty;
    public string?  Category    { get; set; }
    public string?  ContentType { get; set; }
    public int      ChunkCount  { get; set; }
    public string?  CreatedAt   { get; set; }
}

public class ApiSearchResult
{
    public string  Title    { get; set; } = string.Empty;
    public string  Content  { get; set; } = string.Empty;
    public float   Score    { get; set; }
    public string? Source   { get; set; }
}

// ── Backtest ──────────────────────────────────────────────────────────

public class BacktestResult
{
    public string   Id              { get; set; } = string.Empty;
    public string?  StrategyName    { get; set; }
    public double   TotalReturn     { get; set; }
    public double   SharpeRatio     { get; set; }
    public double   MaxDrawdown     { get; set; }
    public double   WinRate         { get; set; }
    public int      TotalTrades     { get; set; }
    public string?  AiInsights      { get; set; }
    public string?  CreatedAt       { get; set; }
}

// ── Researcher ────────────────────────────────────────────────────────

public class ResearcherCompanyResult
{
    public string   Symbol     { get; set; } = string.Empty;
    public string?  Name       { get; set; }
    public string?  Sector     { get; set; }
    public string?  Industry   { get; set; }
    public double   MarketCap  { get; set; }
    public double   Price      { get; set; }
    // raw data payload from backend (ignored in display, used for future expansion)
}

public class PeerComparisonRequest
{
    public string    Symbol { get; set; } = string.Empty;
    public string[]? Peers  { get; set; }
    public string?   Sector { get; set; }
}

public class PeerComparisonResult
{
    public string    Symbol   { get; set; } = string.Empty;
    public string[]? Peers    { get; set; }
    public string?   Analysis { get; set; }
}

public class StrategyAnalysisRequest
{
    public string  Symbol            { get; set; } = string.Empty;
    public string  StrategyType      { get; set; } = "custom";
    public string  Timeframe         { get; set; } = "Daily";
    public string? AdditionalContext { get; set; }
}

public class StrategyAnalysisResult
{
    public string  Symbol   { get; set; } = string.Empty;
    public string? Analysis { get; set; }
}

// ── Reverse Engineer ──────────────────────────────────────────────────

public class ReverseEngineerStartRequest
{
    public string Query { get; set; } = string.Empty;
}

public class ReverseEngineerStrategy
{
    public string   Id        { get; set; } = string.Empty;
    public string?  Query     { get; set; }
    public string?  Phase     { get; set; }
    public string?  Code      { get; set; }
    public string?  Schematic { get; set; }
    public string?  Analysis  { get; set; }
    public string?  Status    { get; set; }
}

// ── Presentations (Deck Generator) ───────────────────────────────────

public class PresentationRequest
{
    public string  Prompt        { get; set; } = string.Empty;
    public int?    SlideCount    { get; set; } = 10;
    public string? Style         { get; set; }
    public bool    IncludeCharts { get; set; } = true;
}

public class ApiPresentation
{
    public string   Id          { get; set; } = string.Empty;
    public string   Title       { get; set; } = string.Empty;
    public string?  Status      { get; set; }
    public string?  DownloadUrl { get; set; }
    public string?  CreatedAt   { get; set; }
}

// ── Skills ────────────────────────────────────────────────────────────

public class ApiSkill
{
    public string  SkillId           { get; set; } = string.Empty;
    public string  Name              { get; set; } = string.Empty;
    public string  Slug              { get; set; } = string.Empty;
    public string  Description       { get; set; } = string.Empty;
    public string? Category          { get; set; }
    public int     MaxTokens         { get; set; }
    public List<string> Tags         { get; set; } = new();
    public bool    Enabled           { get; set; } = true;
    public bool    SupportsStreaming  { get; set; }
}

public class ApiSkillsResponse
{
    public List<ApiSkill> Skills { get; set; } = new();
    public int            Total  { get; set; }
    public string?        CategoryFilter { get; set; }
}

public class ApiSkillCategory
{
    public string  Category { get; set; } = string.Empty;
    public string  Label    { get; set; } = string.Empty;
    public int     Count    { get; set; }
}

public class ApiSkillCategoriesResponse
{
    public List<ApiSkillCategory> Categories { get; set; } = new();
}

public class SkillExecuteRequest
{
    public string  Message       { get; set; } = string.Empty;
    public string? ExtraContext   { get; set; }
    public bool    Stream        { get; set; }
}

public class SkillExecuteResponse
{
    public string   Text           { get; set; } = string.Empty;
    public string?  Skill          { get; set; }
    public string?  SkillName      { get; set; }
    public double   ExecutionTime  { get; set; }
    public string?  DownloadUrl    { get; set; }
    public string?  Filename       { get; set; }
}
