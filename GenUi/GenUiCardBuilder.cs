// ============================================================================
//  GenUiCardBuilder.cs  —  PotomacAnalyst Generative UI Framework  v2
//
//  ARCHITECTURE
//  ─────────────────────────────────────────────────────────────────────────
//  Backend tags every structured response with a JSON envelope:
//
//      { "card": "stock",  "data": { ... } }
//      { "card": "backtest", "data": { ... } }
//
//  GenUiCardBuilder.TryBuild(rawText) detects the envelope, deserialises
//  the payload, delegates to Build* which returns a UIElement.
//
//  PERFORMANCE
//  ─────────────────────────────────────────────────────────────────────────
//  v1 called C.B(color) → new SolidColorBrush(color) on every primitive.
//  A single stock card called C.B() ~60+ times.  Brushes are now pre-cached
//  as static readonly fields in the Br class — zero allocations per card.
//
//  BUGS FIXED FROM v1
//  ─────────────────────────────────────────────────────────────────────────
//  • Stock card: Badge was added twice (SetColumn on discarded temp ref)
//  • MetricsGrid: null-value skip incremented col twice, leaving gaps
//  • Sectors: SetColumn on discarded Label ref (name never placed in grid)
//  • Comparison: used first metric's *values* as column headers (wrong)
//  • TableHeader/Row: ColumnDefinitions added in O(n²) loop pattern
//  • Progress bar: GridLength(0) Star crashes layout when pct == 0 or 1
//
//  CARD CATALOGUE
//  ─────────────────────────────────────────────────────────────────────────
//  ── FINANCE ──────────────────────────────────────────────────────────────
//   1  Stock            live quote, OHLCV, market cap
//   2  Backtest         equity curve stats
//   3  AFL              code block + "Open in Generator"
//   4  Portfolio        holdings table, total PnL
//   5  Screener         multi-row instrument table
//   6  News             headline list with sentiment bar
//   7  Watchlist        compact ticker strip
//   8  Economic Calendar event list with impact dots
//   9  Sectors          horizontal bar chart
//  10  Trade Signal     BUY/SELL/HOLD with confidence bar
//  11  Comparison       side-by-side metric table
//  12  File Analysis    key-points from uploaded doc
//  13  Knowledge Base   snippet with relevance score
//  14  Error/Warning    structured error card
//  15  Task Progress    step-list with timeline
//  ── TRAVEL ───────────────────────────────────────────────────────────────
//  16  Flight           outbound + return legs
//  17  Restaurant       rating, hours, dishes
//  18  Rental Car       vehicle, pickup/dropoff, price
//  19  Weather          conditions + 5-day forecast
//  20  Hotel            stars, price/night, amenities
//  21  Directions       turn-by-turn steps
//  22  Currency         live FX rate + conversion table
// ============================================================================

using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace PotomacAnalyst.GenUi;

// ============================================================================
//  COLOUR PALETTE
// ============================================================================
internal static class C
{
    public static readonly Color Background = Color.FromArgb(255, 0x16, 0x1E, 0x2E);
    public static readonly Color Surface = Color.FromArgb(255, 0x1C, 0x24, 0x36);
    public static readonly Color Border = Color.FromArgb(80, 255, 255, 255);
    public static readonly Color BorderStrong = Color.FromArgb(140, 255, 255, 255);
    public static readonly Color Yellow = Color.FromArgb(255, 0xFE, 0xC0, 0x0F);
    public static readonly Color YellowDim = Color.FromArgb(60, 0xFE, 0xC0, 0x0F);
    public static readonly Color Green = Color.FromArgb(255, 0x4A, 0xE0, 0x6A);
    public static readonly Color GreenDim = Color.FromArgb(40, 0x4A, 0xE0, 0x6A);
    public static readonly Color Red = Color.FromArgb(255, 0xE0, 0x4A, 0x4A);
    public static readonly Color RedDim = Color.FromArgb(40, 0xE0, 0x4A, 0x4A);
    public static readonly Color Blue = Color.FromArgb(255, 0x4A, 0x9E, 0xE0);
    public static readonly Color BlueDim = Color.FromArgb(40, 0x4A, 0x9E, 0xE0);
    public static readonly Color Orange = Color.FromArgb(255, 0xE0, 0x8C, 0x4A);
    public static readonly Color OrangeDim = Color.FromArgb(40, 0xE0, 0x8C, 0x4A);
    public static readonly Color TextPrimary = Color.FromArgb(255, 0xF0, 0xF0, 0xFF);
    public static readonly Color TextSecondary = Color.FromArgb(200, 0xB0, 0xB8, 0xCC);
    public static readonly Color TextMuted = Color.FromArgb(150, 0x80, 0x88, 0x99);
    public static readonly Color CodeBg = Color.FromArgb(255, 0x0D, 0x0D, 0x12);
    public static readonly Color CodeBorder = Color.FromArgb(255, 0x2A, 0x2A, 0x3A);
    public static readonly Color CodeHeader = Color.FromArgb(255, 0x14, 0x14, 0x1E);
    public static readonly Color Teal = Color.FromArgb(255, 0x2E, 0xCC, 0xB8);
    public static readonly Color TealDim = Color.FromArgb(40, 0x2E, 0xCC, 0xB8);
    public static readonly Color Purple = Color.FromArgb(255, 0xA0, 0x6E, 0xE8);
    public static readonly Color PurpleDim = Color.FromArgb(40, 0xA0, 0x6E, 0xE8);
    public static readonly Color Sky = Color.FromArgb(255, 0x5B, 0xC8, 0xF5);
    public static readonly Color SkyDim = Color.FromArgb(40, 0x5B, 0xC8, 0xF5);

    // Non-cached helper only needed where a dynamic Color value isn't in Br
    public static SolidColorBrush B(Color c) => new(c);
}

// ============================================================================
//  BRUSH CACHE  —  every named colour pre-allocated, zero allocations per card
// ============================================================================
internal static class Br
{
    public static readonly SolidColorBrush Background = new(C.Background);
    public static readonly SolidColorBrush Surface = new(C.Surface);
    public static readonly SolidColorBrush Border = new(C.Border);
    public static readonly SolidColorBrush BorderStrong = new(C.BorderStrong);
    public static readonly SolidColorBrush Yellow = new(C.Yellow);
    public static readonly SolidColorBrush YellowDim = new(C.YellowDim);
    public static readonly SolidColorBrush Green = new(C.Green);
    public static readonly SolidColorBrush GreenDim = new(C.GreenDim);
    public static readonly SolidColorBrush Red = new(C.Red);
    public static readonly SolidColorBrush RedDim = new(C.RedDim);
    public static readonly SolidColorBrush Blue = new(C.Blue);
    public static readonly SolidColorBrush BlueDim = new(C.BlueDim);
    public static readonly SolidColorBrush Orange = new(C.Orange);
    public static readonly SolidColorBrush OrangeDim = new(C.OrangeDim);
    public static readonly SolidColorBrush TextPrimary = new(C.TextPrimary);
    public static readonly SolidColorBrush TextSecondary = new(C.TextSecondary);
    public static readonly SolidColorBrush TextMuted = new(C.TextMuted);
    public static readonly SolidColorBrush CodeBg = new(C.CodeBg);
    public static readonly SolidColorBrush CodeBorder = new(C.CodeBorder);
    public static readonly SolidColorBrush CodeHeader = new(C.CodeHeader);
    public static readonly SolidColorBrush Teal = new(C.Teal);
    public static readonly SolidColorBrush TealDim = new(C.TealDim);
    public static readonly SolidColorBrush Purple = new(C.Purple);
    public static readonly SolidColorBrush PurpleDim = new(C.PurpleDim);
    public static readonly SolidColorBrush Sky = new(C.Sky);
    public static readonly SolidColorBrush SkyDim = new(C.SkyDim);

    // Semi-transparent shared brushes
    public static readonly SolidColorBrush SubtleBg = new(Color.FromArgb(30, 255, 255, 255));
    public static readonly SolidColorBrush DividerBrush = new(Color.FromArgb(40, 255, 255, 255));
    public static readonly SolidColorBrush StarEmpty = new(Color.FromArgb(40, 255, 255, 255));
    public static readonly SolidColorBrush CellBg = new(Color.FromArgb(40, 255, 255, 255));

    // Card-specific backgrounds (static, dark-theme matched)
    public static readonly SolidColorBrush FlightBg = new(Color.FromArgb(255, 0x10, 0x18, 0x2C));
    public static readonly SolidColorBrush RestaurantBg = new(Color.FromArgb(255, 0x18, 0x12, 0x10));
    public static readonly SolidColorBrush HotelBg = new(Color.FromArgb(255, 0x14, 0x10, 0x1C));
    public static readonly SolidColorBrush DirectionsBg = new(Color.FromArgb(255, 0x0E, 0x18, 0x14));
    public static readonly SolidColorBrush KbBg = new(Color.FromArgb(255, 0x0F, 0x1A, 0x2E));
    public static readonly SolidColorBrush Transparent = new(Colors.Transparent);
}

// ============================================================================
//  FONT CACHE
// ============================================================================
internal static class F
{
    private static FontFamily? _bold, _medium, _regular, _mono;
    public static FontFamily Bold => _bold ??= (FontFamily)Application.Current.Resources["RajdhaniBold"];
    public static FontFamily Medium => _medium ??= (FontFamily)Application.Current.Resources["QuicksandMedium"];
    public static FontFamily Regular => _regular ??= (FontFamily)Application.Current.Resources["QuicksandRegular"];
    public static FontFamily Mono => _mono ??= new FontFamily("Cascadia Code, Consolas, Courier New");
}

// ============================================================================
//  JSON MODELS
// ============================================================================

public sealed class CardEnvelope
{
    [JsonPropertyName("card")] public string Card { get; init; } = "";
    [JsonPropertyName("data")] public JsonElement Data { get; init; }
}

public sealed class StockPayload
{
    [JsonPropertyName("ticker")] public string Ticker { get; init; } = "";
    [JsonPropertyName("company")] public string Company { get; init; } = "";
    [JsonPropertyName("price")] public double? Price { get; init; }
    [JsonPropertyName("change")] public double? Change { get; init; }
    [JsonPropertyName("changePct")] public double? ChangePct { get; init; }
    [JsonPropertyName("open")] public double? Open { get; init; }
    [JsonPropertyName("prevClose")] public double? PrevClose { get; init; }
    [JsonPropertyName("high")] public double? High { get; init; }
    [JsonPropertyName("low")] public double? Low { get; init; }
    [JsonPropertyName("volume")] public string? Volume { get; init; }
    [JsonPropertyName("marketCap")] public string? MarketCap { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class BacktestPayload
{
    [JsonPropertyName("strategy")] public string Strategy { get; init; } = "";
    [JsonPropertyName("symbol")] public string Symbol { get; init; } = "";
    [JsonPropertyName("from")] public string From { get; init; } = "";
    [JsonPropertyName("to")] public string To { get; init; } = "";
    [JsonPropertyName("netProfit")] public double? NetProfit { get; init; }
    [JsonPropertyName("netProfitPct")] public double? NetProfitPct { get; init; }
    [JsonPropertyName("winRate")] public double? WinRate { get; init; }
    [JsonPropertyName("maxDrawdown")] public double? MaxDrawdown { get; init; }
    [JsonPropertyName("sharpe")] public double? Sharpe { get; init; }
    [JsonPropertyName("totalTrades")] public int? TotalTrades { get; init; }
    [JsonPropertyName("avgWin")] public double? AvgWin { get; init; }
    [JsonPropertyName("avgLoss")] public double? AvgLoss { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class AflPayload
{
    [JsonPropertyName("code")] public string Code { get; init; } = "";
    [JsonPropertyName("title")] public string Title { get; init; } = "AFL Strategy";
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class PortfolioPayload
{
    [JsonPropertyName("totalValue")] public double? TotalValue { get; init; }
    [JsonPropertyName("totalPnl")] public double? TotalPnl { get; init; }
    [JsonPropertyName("totalPnlPct")] public double? TotalPnlPct { get; init; }
    [JsonPropertyName("holdings")] public List<Holding> Holdings { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
public sealed class Holding
{
    [JsonPropertyName("ticker")] public string Ticker { get; init; } = "";
    [JsonPropertyName("shares")] public double? Shares { get; init; }
    [JsonPropertyName("avgCost")] public double? AvgCost { get; init; }
    [JsonPropertyName("current")] public double? Current { get; init; }
    [JsonPropertyName("pnl")] public double? Pnl { get; init; }
    [JsonPropertyName("pnlPct")] public double? PnlPct { get; init; }
}

public sealed class ScreenerPayload
{
    [JsonPropertyName("title")] public string Title { get; init; } = "Screener Results";
    [JsonPropertyName("columns")] public List<string> Columns { get; init; } = new();
    [JsonPropertyName("rows")] public List<List<string>> Rows { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class NewsPayload
{
    [JsonPropertyName("title")] public string Title { get; init; } = "Market News";
    [JsonPropertyName("items")] public List<NewsItem> Items { get; init; } = new();
}
public sealed class NewsItem
{
    [JsonPropertyName("headline")] public string Headline { get; init; } = "";
    [JsonPropertyName("source")] public string Source { get; init; } = "";
    [JsonPropertyName("time")] public string Time { get; init; } = "";
    [JsonPropertyName("sentiment")] public string? Sentiment { get; init; }
}

public sealed class WatchlistPayload
{
    [JsonPropertyName("tickers")] public List<WatchlistTicker> Tickers { get; init; } = new();
}
public sealed class WatchlistTicker
{
    [JsonPropertyName("ticker")] public string Ticker { get; init; } = "";
    [JsonPropertyName("price")] public double? Price { get; init; }
    [JsonPropertyName("changePct")] public double? ChangePct { get; init; }
}

public sealed class EconomicCalendarPayload
{
    [JsonPropertyName("items")] public List<EconEvent> Items { get; init; } = new();
}
public sealed class EconEvent
{
    [JsonPropertyName("time")] public string Time { get; init; } = "";
    [JsonPropertyName("event")] public string Event { get; init; } = "";
    [JsonPropertyName("country")] public string Country { get; init; } = "";
    [JsonPropertyName("impact")] public string Impact { get; init; } = "low";
    [JsonPropertyName("forecast")] public string? Forecast { get; init; }
    [JsonPropertyName("previous")] public string? Previous { get; init; }
}

public sealed class SectorPayload
{
    [JsonPropertyName("sectors")] public List<SectorBar> Sectors { get; init; } = new();
}
public sealed class SectorBar
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("changePct")] public double ChangePct { get; init; }
}

public sealed class TradeSignalPayload
{
    [JsonPropertyName("ticker")] public string Ticker { get; init; } = "";
    [JsonPropertyName("signal")] public string Signal { get; init; } = "HOLD";
    [JsonPropertyName("confidence")] public double? Confidence { get; init; }
    [JsonPropertyName("entry")] public double? Entry { get; init; }
    [JsonPropertyName("stopLoss")] public double? StopLoss { get; init; }
    [JsonPropertyName("target")] public double? Target { get; init; }
    [JsonPropertyName("timeframe")] public string? Timeframe { get; init; }
    [JsonPropertyName("reasons")] public List<string> Reasons { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class ComparisonPayload
{
    [JsonPropertyName("title")] public string Title { get; init; } = "Comparison";
    [JsonPropertyName("headers")] public List<string> Headers { get; init; } = new(); // NEW: explicit header row
    [JsonPropertyName("metrics")] public List<ComparisonMetric> Metrics { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
public sealed class ComparisonMetric
{
    [JsonPropertyName("label")] public string Label { get; init; } = "";
    [JsonPropertyName("values")] public List<string> Values { get; init; } = new();
}

public sealed class FileAnalysisPayload
{
    [JsonPropertyName("filename")] public string Filename { get; init; } = "";
    [JsonPropertyName("fileType")] public string FileType { get; init; } = "";
    [JsonPropertyName("keyPoints")] public List<string> KeyPoints { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class KnowledgeBasePayload
{
    [JsonPropertyName("source")] public string Source { get; init; } = "";
    [JsonPropertyName("snippet")] public string Snippet { get; init; } = "";
    [JsonPropertyName("relevance")] public double? Relevance { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class ErrorPayload
{
    [JsonPropertyName("level")] public string Level { get; init; } = "error";
    [JsonPropertyName("title")] public string Title { get; init; } = "Error";
    [JsonPropertyName("message")] public string Message { get; init; } = "";
    [JsonPropertyName("code")] public string? Code { get; init; }
}

public sealed class TaskProgressPayload
{
    [JsonPropertyName("title")] public string Title { get; init; } = "Task Progress";
    [JsonPropertyName("steps")] public List<TaskStep> Steps { get; init; } = new();
}
public sealed class TaskStep
{
    [JsonPropertyName("label")] public string Label { get; init; } = "";
    [JsonPropertyName("state")] public string State { get; init; } = "pending";
}

public sealed class FlightPayload
{
    [JsonPropertyName("outbound")] public FlightLeg Outbound { get; init; } = new();
    [JsonPropertyName("returnLeg")] public FlightLeg? ReturnLeg { get; init; }
    [JsonPropertyName("cabin")] public string Cabin { get; init; } = "Economy";
    [JsonPropertyName("price")] public double? Price { get; init; }
    [JsonPropertyName("currency")] public string Currency { get; init; } = "USD";
    [JsonPropertyName("bookingRef")] public string? BookingRef { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
public sealed class FlightLeg
{
    [JsonPropertyName("fromCode")] public string FromCode { get; init; } = "";
    [JsonPropertyName("fromCity")] public string FromCity { get; init; } = "";
    [JsonPropertyName("toCode")] public string ToCode { get; init; } = "";
    [JsonPropertyName("toCity")] public string ToCity { get; init; } = "";
    [JsonPropertyName("departTime")] public string DepartTime { get; init; } = "";
    [JsonPropertyName("arriveTime")] public string ArriveTime { get; init; } = "";
    [JsonPropertyName("duration")] public string Duration { get; init; } = "";
    [JsonPropertyName("stops")] public int Stops { get; init; }
    [JsonPropertyName("airline")] public string Airline { get; init; } = "";
    [JsonPropertyName("flightNumber")] public string FlightNumber { get; init; } = "";
    [JsonPropertyName("date")] public string Date { get; init; } = "";
}

public sealed class RestaurantPayload
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("cuisine")] public string Cuisine { get; init; } = "";
    [JsonPropertyName("rating")] public double? Rating { get; init; }
    [JsonPropertyName("reviewCount")] public int? ReviewCount { get; init; }
    [JsonPropertyName("priceRange")] public string PriceRange { get; init; } = "$$";
    [JsonPropertyName("address")] public string? Address { get; init; }
    [JsonPropertyName("phone")] public string? Phone { get; init; }
    [JsonPropertyName("hours")] public string? Hours { get; init; }
    [JsonPropertyName("isOpen")] public bool? IsOpen { get; init; }
    [JsonPropertyName("topDishes")] public List<string> TopDishes { get; init; } = new();
    [JsonPropertyName("features")] public List<string> Features { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class RentalCarPayload
{
    [JsonPropertyName("company")] public string Company { get; init; } = "";
    [JsonPropertyName("vehicle")] public string Vehicle { get; init; } = "";
    [JsonPropertyName("category")] public string Category { get; init; } = "";
    [JsonPropertyName("pickupDate")] public string? PickupDate { get; init; }
    [JsonPropertyName("dropoffDate")] public string? DropoffDate { get; init; }
    [JsonPropertyName("pickupLoc")] public string? PickupLoc { get; init; }
    [JsonPropertyName("dropoffLoc")] public string? DropoffLoc { get; init; }
    [JsonPropertyName("days")] public int? Days { get; init; }
    [JsonPropertyName("pricePerDay")] public double? PricePerDay { get; init; }
    [JsonPropertyName("totalPrice")] public double? TotalPrice { get; init; }
    [JsonPropertyName("currency")] public string Currency { get; init; } = "USD";
    [JsonPropertyName("inclusions")] public List<string> Inclusions { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class WeatherPayload
{
    [JsonPropertyName("location")] public string Location { get; init; } = "";
    [JsonPropertyName("condition")] public string Condition { get; init; } = "";
    [JsonPropertyName("tempC")] public double? TempC { get; init; }
    [JsonPropertyName("tempF")] public double? TempF { get; init; }
    [JsonPropertyName("feelsLikeC")] public double? FeelsLikeC { get; init; }
    [JsonPropertyName("feelsLikeF")] public double? FeelsLikeF { get; init; }
    [JsonPropertyName("humidity")] public int? Humidity { get; init; }
    [JsonPropertyName("windKph")] public double? WindKph { get; init; }
    [JsonPropertyName("windDir")] public string? WindDir { get; init; }
    [JsonPropertyName("uvIndex")] public int? UvIndex { get; init; }
    [JsonPropertyName("visibility")] public string? Visibility { get; init; }
    [JsonPropertyName("useFahrenheit")] public bool UseFahrenheit { get; init; }
    [JsonPropertyName("forecast")] public List<WeatherDay> Forecast { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
public sealed class WeatherDay
{
    [JsonPropertyName("day")] public string Day { get; init; } = "";
    [JsonPropertyName("condition")] public string Condition { get; init; } = "";
    [JsonPropertyName("highC")] public double? HighC { get; init; }
    [JsonPropertyName("lowC")] public double? LowC { get; init; }
    [JsonPropertyName("highF")] public double? HighF { get; init; }
    [JsonPropertyName("lowF")] public double? LowF { get; init; }
    [JsonPropertyName("precipPct")] public int? PrecipPct { get; init; }
}

public sealed class HotelPayload
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("stars")] public int? Stars { get; init; }
    [JsonPropertyName("rating")] public double? Rating { get; init; }
    [JsonPropertyName("reviewCount")] public int? ReviewCount { get; init; }
    [JsonPropertyName("address")] public string? Address { get; init; }
    [JsonPropertyName("checkIn")] public string? CheckIn { get; init; }
    [JsonPropertyName("checkOut")] public string? CheckOut { get; init; }
    [JsonPropertyName("nights")] public int? Nights { get; init; }
    [JsonPropertyName("roomType")] public string? RoomType { get; init; }
    [JsonPropertyName("pricePerNight")] public double? PricePerNight { get; init; }
    [JsonPropertyName("totalPrice")] public double? TotalPrice { get; init; }
    [JsonPropertyName("currency")] public string Currency { get; init; } = "USD";
    [JsonPropertyName("amenities")] public List<string> Amenities { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}

public sealed class DirectionsPayload
{
    [JsonPropertyName("origin")] public string Origin { get; init; } = "";
    [JsonPropertyName("destination")] public string Destination { get; init; } = "";
    [JsonPropertyName("mode")] public string Mode { get; init; } = "driving";
    [JsonPropertyName("totalDistance")] public string? TotalDistance { get; init; }
    [JsonPropertyName("totalDuration")] public string? TotalDuration { get; init; }
    [JsonPropertyName("steps")] public List<DirectionStep> Steps { get; init; } = new();
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
public sealed class DirectionStep
{
    [JsonPropertyName("instruction")] public string Instruction { get; init; } = "";
    [JsonPropertyName("distance")] public string? Distance { get; init; }
    [JsonPropertyName("duration")] public string? Duration { get; init; }
}

public sealed class CurrencyPayload
{
    [JsonPropertyName("fromCode")] public string FromCode { get; init; } = "";
    [JsonPropertyName("fromName")] public string FromName { get; init; } = "";
    [JsonPropertyName("toCode")] public string ToCode { get; init; } = "";
    [JsonPropertyName("toName")] public string ToName { get; init; } = "";
    [JsonPropertyName("rate")] public double Rate { get; init; }
    [JsonPropertyName("change")] public double? Change { get; init; }
    [JsonPropertyName("changePct")] public double? ChangePct { get; init; }
    [JsonPropertyName("conversions")] public List<CurrencyRow> Conversions { get; init; } = new();
    [JsonPropertyName("asOf")] public string? AsOf { get; init; }
    [JsonPropertyName("summary")] public string? Summary { get; init; }
}
public sealed class CurrencyRow
{
    [JsonPropertyName("amount")] public double Amount { get; init; }
    [JsonPropertyName("converted")] public double Converted { get; init; }
}

// ============================================================================
//  BUILDER — single public entry point
// ============================================================================

public static class GenUiCardBuilder
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Attempts to parse rawText as a card envelope.
    /// Returns null if rawText is plain prose (no card signal found).
    /// </summary>
    public static UIElement? TryBuild(string rawText, Action<string>? navigateTo = null)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return null;

        var start = rawText.IndexOf("{\"card\"", StringComparison.OrdinalIgnoreCase);
        if (start < 0) start = rawText.IndexOf("{ \"card\"", StringComparison.OrdinalIgnoreCase);
        if (start < 0) return null;

        var json = ExtractJson(rawText, start);
        if (json is null) return null;

        try
        {
            var env = JsonSerializer.Deserialize<CardEnvelope>(json, _json);
            if (env is null) return null;

            return env.Card.ToLowerInvariant() switch
            {
                "stock" => BuildStock(env.Data.Deserialize<StockPayload>(_json)),
                "backtest" => BuildBacktest(env.Data.Deserialize<BacktestPayload>(_json)),
                "afl" => BuildAfl(env.Data.Deserialize<AflPayload>(_json), navigateTo),
                "portfolio" => BuildPortfolio(env.Data.Deserialize<PortfolioPayload>(_json)),
                "screener" => BuildScreener(env.Data.Deserialize<ScreenerPayload>(_json)),
                "news" => BuildNews(env.Data.Deserialize<NewsPayload>(_json)),
                "watchlist" => BuildWatchlist(env.Data.Deserialize<WatchlistPayload>(_json)),
                "economic_calendar" => BuildEconomicCalendar(env.Data.Deserialize<EconomicCalendarPayload>(_json)),
                "sectors" => BuildSectors(env.Data.Deserialize<SectorPayload>(_json)),
                "trade_signal" => BuildTradeSignal(env.Data.Deserialize<TradeSignalPayload>(_json)),
                "comparison" => BuildComparison(env.Data.Deserialize<ComparisonPayload>(_json)),
                "file_analysis" => BuildFileAnalysis(env.Data.Deserialize<FileAnalysisPayload>(_json)),
                "knowledge_base" => BuildKnowledgeBase(env.Data.Deserialize<KnowledgeBasePayload>(_json)),
                "error" => BuildError(env.Data.Deserialize<ErrorPayload>(_json)),
                "task_progress" => BuildTaskProgress(env.Data.Deserialize<TaskProgressPayload>(_json)),
                "flight" => BuildFlight(env.Data.Deserialize<FlightPayload>(_json)),
                "restaurant" => BuildRestaurant(env.Data.Deserialize<RestaurantPayload>(_json)),
                "rental_car" => BuildRentalCar(env.Data.Deserialize<RentalCarPayload>(_json)),
                "weather" => BuildWeather(env.Data.Deserialize<WeatherPayload>(_json)),
                "hotel" => BuildHotel(env.Data.Deserialize<HotelPayload>(_json)),
                "directions" => BuildDirections(env.Data.Deserialize<DirectionsPayload>(_json)),
                "currency" => BuildCurrency(env.Data.Deserialize<CurrencyPayload>(_json)),
                _ => null,
            };
        }
        catch { return null; }
    }

    private static string? ExtractJson(string src, int start)
    {
        int depth = 0;
        for (int i = start; i < src.Length; i++)
        {
            if (src[i] == '{') depth++;
            else if (src[i] == '}') { if (--depth == 0) return src[start..(i + 1)]; }
        }
        return null;
    }

    // =========================================================================
    //  CARD 1 — STOCK
    // =========================================================================
    private static UIElement? BuildStock(StockPayload? p)
    {
        if (p is null) return null;
        bool up = (p.Change ?? 0) >= 0;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        // Header row
        var header = TwoColGrid();
        var nameStack = new StackPanel { Spacing = 1 };
        nameStack.Children.Add(Label(p.Company, 12, Br.TextSecondary));
        nameStack.Children.Add(BoldLabel(p.Ticker.ToUpper(), 18, Br.Yellow, spacing: 40));
        Grid.SetColumn(nameStack, 0);
        header.Children.Add(nameStack);

        // FIX v1: badge was added twice; SetColumn was called on the return value
        // of Badge(), discarded, then Badge() was called again and added without SetColumn.
        if (p.Change.HasValue && p.ChangePct.HasValue)
        {
            var sign = up ? "+" : "";
            var badge = Badge($"{sign}{p.Change:F2}  ({sign}{p.ChangePct:F2}%)",
                up ? Br.Green : Br.Red, up ? Br.GreenDim : Br.RedDim);
            Grid.SetColumn(badge, 1);
            header.Children.Add(badge);
        }
        stack.Children.Add(header);

        if (p.Price.HasValue)
            stack.Children.Add(BoldLabel($"${p.Price:N2}", 36, Br.TextPrimary,
                margin: new Thickness(0, 6, 0, 2)));
        stack.Children.Add(Divider(14));

        var metrics = new List<(string, string?)>
        {
            ("OPEN",       p.Open.HasValue      ? $"${p.Open:N2}"      : null),
            ("PREV CLOSE", p.PrevClose.HasValue ? $"${p.PrevClose:N2}" : null),
            ("DAY HIGH",   p.High.HasValue      ? $"${p.High:N2}"      : null),
            ("DAY LOW",    p.Low.HasValue       ? $"${p.Low:N2}"       : null),
            ("VOLUME",     p.Volume),
            ("MARKET CAP", p.MarketCap),
        };
        var colours = new Dictionary<string, SolidColorBrush>
        {
            ["DAY HIGH"] = Br.Green,
            ["DAY LOW"] = Br.Red,
        };
        stack.Children.Add(MetricsGrid(metrics, colours));
        stack.Children.Add(LiveFooter());

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 2 — BACKTEST
    // =========================================================================
    private static UIElement? BuildBacktest(BacktestPayload? p)
    {
        if (p is null) return null;
        bool profit = (p.NetProfit ?? 0) >= 0;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        var titleRow = TwoColGrid();
        var titleStack = new StackPanel { Spacing = 1 };
        titleStack.Children.Add(Label("BACKTEST RESULTS", 10, Br.TextMuted, spacing: 60));
        titleStack.Children.Add(BoldLabel(p.Strategy, 16, Br.Yellow));
        Grid.SetColumn(titleStack, 0);
        titleRow.Children.Add(titleStack);

        var periodLabel = Label($"{p.From}  —  {p.To}", 11, Br.TextMuted);
        periodLabel.VerticalAlignment = VerticalAlignment.Top;
        Grid.SetColumn(periodLabel, 1);
        titleRow.Children.Add(periodLabel);
        stack.Children.Add(titleRow);

        if (p.NetProfit.HasValue)
        {
            var heroRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Margin = new Thickness(0, 8, 0, 2),
                VerticalAlignment = VerticalAlignment.Center,
            };
            heroRow.Children.Add(BoldLabel(
                $"{(profit ? "+" : "")}{p.NetProfit:N0}",
                32, profit ? Br.Green : Br.Red));
            if (p.NetProfitPct.HasValue)
                heroRow.Children.Add(Badge(
                    $"{(profit ? "+" : "")}{p.NetProfitPct:F1}%",
                    profit ? Br.Green : Br.Red,
                    profit ? Br.GreenDim : Br.RedDim));
            stack.Children.Add(heroRow);
        }
        stack.Children.Add(Divider(12));

        var metrics = new List<(string, string?)>
        {
            ("WIN RATE",     p.WinRate.HasValue    ? $"{p.WinRate:F1}%"    : null),
            ("MAX DRAWDOWN", p.MaxDrawdown.HasValue ? $"{p.MaxDrawdown:F1}%" : null),
            ("SHARPE",       p.Sharpe.HasValue      ? $"{p.Sharpe:F2}"     : null),
            ("TOTAL TRADES", p.TotalTrades.HasValue ? $"{p.TotalTrades}"   : null),
            ("AVG WIN",      p.AvgWin.HasValue      ? $"${p.AvgWin:N2}"    : null),
            ("AVG LOSS",     p.AvgLoss.HasValue     ? $"${p.AvgLoss:N2}"   : null),
        };
        var colours = new Dictionary<string, SolidColorBrush>
        {
            ["WIN RATE"] = Br.Green,
            ["MAX DRAWDOWN"] = Br.Red,
        };
        stack.Children.Add(MetricsGrid(metrics, colours));

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 3 — AFL CODE
    // =========================================================================
    private static UIElement? BuildAfl(AflPayload? p, Action<string>? navigateTo)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };

        var outer = new Border
        {
            Background = Br.CodeBg,
            BorderBrush = Br.CodeBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel();

        var headerGrid = new Grid
        {
            Background = Br.CodeHeader,
            Padding = new Thickness(14, 8, 10, 8),
        };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleStack = new StackPanel { Spacing = 1 };
        titleStack.Children.Add(Label("AFL STRATEGY", 10, Br.Yellow, spacing: 60));
        titleStack.Children.Add(Label(p.Title, 13, Br.TextPrimary));
        Grid.SetColumn(titleStack, 0);
        headerGrid.Children.Add(titleStack);

        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        if (navigateTo is not null)
        {
            var openBtn = SmallButton("OPEN IN GENERATOR", Br.Yellow,
                Color.FromArgb(255, 0x1A, 0x14, 0x00));
            openBtn.Click += (_, _) => navigateTo("AflGenerator");
            btnRow.Children.Add(openBtn);
        }
        var copyBtn = SmallButton("COPY", Br.TextMuted, C.CodeBg);
        // FIX Bug 8: previously `var code = p.Code` (untrimmed), while the TextBlock
        // below used `p.Code.Trim()`.  Clipboard content and display must match.
        var code = p.Code.Trim();
        copyBtn.Click += (_, _) =>
        {
            var pkg = new DataPackage(); pkg.SetText(code); Clipboard.SetContent(pkg);
            copyBtn.Content = "COPIED";
            _ = Task.Delay(1200).ContinueWith(_ =>
                copyBtn.DispatcherQueue.TryEnqueue(() => copyBtn.Content = "COPY"));
        };
        btnRow.Children.Add(copyBtn);
        Grid.SetColumn(btnRow, 1);
        headerGrid.Children.Add(btnRow);
        stack.Children.Add(headerGrid);

        stack.Children.Add(new ScrollViewer
        {
            Content = new TextBlock
            {
                Text = code,  // already trimmed above
                FontFamily = F.Mono,
                FontSize = 12,
                LineHeight = 20,
                TextWrapping = TextWrapping.NoWrap,
                Foreground = C.B(Color.FromArgb(255, 0xCE, 0xD4, 0xE0)),
                Padding = new Thickness(14, 12, 14, 14),
            },
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = 420,
        });

        outer.Child = stack;
        panel.Children.Add(outer);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 4 — PORTFOLIO
    // =========================================================================
    private static UIElement? BuildPortfolio(PortfolioPayload? p)
    {
        if (p is null) return null;
        bool profit = (p.TotalPnl ?? 0) >= 0;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        var h = TwoColGrid();
        h.Children.Add(BoldLabel("PORTFOLIO", 13, Br.Yellow, spacing: 60));
        if (p.TotalValue.HasValue)
        {
            var rv = BoldLabel($"${p.TotalValue:N2}", 22, Br.TextPrimary);
            Grid.SetColumn(rv, 1);
            h.Children.Add(rv);
        }
        stack.Children.Add(h);

        if (p.TotalPnl.HasValue)
        {
            var pnlRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 4, 0, 0),
            };
            pnlRow.Children.Add(Label("TOTAL P&L", 10, Br.TextMuted, spacing: 40));
            pnlRow.Children.Add(Badge(
                $"{(profit ? "+" : "")}{p.TotalPnl:N2}  ({(profit ? "+" : "")}{p.TotalPnlPct:F2}%)",
                profit ? Br.Green : Br.Red,
                profit ? Br.GreenDim : Br.RedDim));
            stack.Children.Add(pnlRow);
        }
        stack.Children.Add(Divider(14));

        if (p.Holdings.Count > 0)
        {
            stack.Children.Add(TableHeader(["TICKER", "SHARES", "AVG COST", "CURRENT", "P&L", "%"]));
            foreach (var h2 in p.Holdings)
            {
                bool hp = (h2.Pnl ?? 0) >= 0;
                var colOverrides = new Dictionary<int, SolidColorBrush>
                {
                    [4] = hp ? Br.Green : Br.Red,
                    [5] = hp ? Br.Green : Br.Red,
                };
                stack.Children.Add(TableRow([
                    h2.Ticker,
                    h2.Shares.HasValue  ? $"{h2.Shares:N0}"   : "—",
                    h2.AvgCost.HasValue ? $"${h2.AvgCost:N2}" : "—",
                    h2.Current.HasValue ? $"${h2.Current:N2}" : "—",
                    h2.Pnl.HasValue     ? $"{(hp?"+":"")}{h2.Pnl:N2}" : "—",
                    h2.PnlPct.HasValue  ? $"{(hp?"+":"")}{h2.PnlPct:F1}%" : "—",
                ], colOverrides));
            }
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 5 — SCREENER
    // =========================================================================
    private static UIElement? BuildScreener(ScreenerPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        stack.Children.Add(BoldLabel(p.Title.ToUpper(), 13, Br.Yellow,
            spacing: 60, margin: new Thickness(0, 0, 0, 12)));
        stack.Children.Add(TableHeader(p.Columns.Select(c => c.ToUpper()).ToArray()));

        foreach (var row in p.Rows)
            stack.Children.Add(TableRow(row.ToArray(), new Dictionary<int, SolidColorBrush>()));

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 6 — NEWS
    // =========================================================================
    private static UIElement? BuildNews(NewsPayload? p)
    {
        if (p is null) return null;
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        stack.Children.Add(BoldLabel(p.Title.ToUpper(), 13, Br.Yellow,
            spacing: 60, margin: new Thickness(0, 0, 0, 12)));

        for (int i = 0; i < p.Items.Count; i++)
        {
            var item = p.Items[i];
            if (i > 0) stack.Children.Add(Divider(8));

            var sentBrush = item.Sentiment switch
            {
                "positive" => Br.Green,
                "negative" => Br.Red,
                _ => Br.Blue,
            };
            var row = new Grid { Margin = new Thickness(0) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });

            var bar = new Border
            {
                Width = 3,
                CornerRadius = new CornerRadius(2),
                Background = sentBrush,
                Margin = new Thickness(0, 2, 10, 2),
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            Grid.SetColumn(bar, 0);

            var textStack = new StackPanel { Spacing = 3 };
            textStack.Children.Add(new TextBlock
            {
                Text = item.Headline,
                FontFamily = F.Medium,
                FontSize = 13,
                Foreground = Br.TextPrimary,
                TextWrapping = TextWrapping.Wrap,
            });
            var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            meta.Children.Add(Label(item.Source, 11, Br.TextMuted));
            meta.Children.Add(Label(item.Time, 11, Br.TextMuted));
            textStack.Children.Add(meta);
            Grid.SetColumn(textStack, 1);

            row.Children.Add(bar);
            row.Children.Add(textStack);
            stack.Children.Add(row);
        }

        card.Child = stack;
        return card;
    }

    // =========================================================================
    //  CARD 7 — WATCHLIST
    // =========================================================================
    private static UIElement? BuildWatchlist(WatchlistPayload? p)
    {
        if (p is null) return null;
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        stack.Children.Add(BoldLabel("WATCHLIST", 13, Br.Yellow,
            spacing: 60, margin: new Thickness(0, 0, 0, 10)));

        var strip = new Grid { ColumnSpacing = 10 };
        for (int i = 0; i < p.Tickers.Count; i++)
            strip.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });

        for (int i = 0; i < p.Tickers.Count; i++)
        {
            var t = p.Tickers[i];
            bool up = (t.ChangePct ?? 0) >= 0;
            var cell = new Border
            {
                Background = Br.CellBg,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 8, 10, 8),
            };
            var cellStack = new StackPanel { Spacing = 2 };
            cellStack.Children.Add(BoldLabel(t.Ticker, 14, Br.Yellow));
            if (t.Price.HasValue)
                cellStack.Children.Add(Label($"${t.Price:N2}", 13, Br.TextPrimary));
            if (t.ChangePct.HasValue)
                cellStack.Children.Add(Label(
                    $"{(up ? "+" : "")}{t.ChangePct:F2}%", 11,
                    up ? Br.Green : Br.Red));
            cell.Child = cellStack;
            Grid.SetColumn(cell, i);
            strip.Children.Add(cell);
        }
        stack.Children.Add(strip);
        card.Child = stack;
        return card;
    }

    // =========================================================================
    //  CARD 8 — ECONOMIC CALENDAR
    // =========================================================================
    private static UIElement? BuildEconomicCalendar(EconomicCalendarPayload? p)
    {
        if (p is null) return null;
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        stack.Children.Add(BoldLabel("ECONOMIC CALENDAR", 13, Br.Yellow,
            spacing: 60, margin: new Thickness(0, 0, 0, 12)));

        foreach (var ev in p.Items)
        {
            var impactBrush = ev.Impact switch
            {
                "high" => Br.Red,
                "medium" => Br.Orange,
                _ => Br.Blue,
            };
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8), ColumnSpacing = 10 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Pixel) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var timeLbl = Label(ev.Time, 11, Br.TextMuted);
            Grid.SetColumn(timeLbl, 0);
            row.Children.Add(timeLbl);

            var dot = new Border
            {
                Width = 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = impactBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(dot, 1);
            row.Children.Add(dot);

            var eventStack = new StackPanel { Spacing = 1 };
            eventStack.Children.Add(Label(ev.Event, 12, Br.TextPrimary));
            eventStack.Children.Add(Label(ev.Country, 10, Br.TextMuted));
            Grid.SetColumn(eventStack, 2);
            row.Children.Add(eventStack);

            if (ev.Forecast is not null || ev.Previous is not null)
            {
                var fStack = new StackPanel
                {
                    Spacing = 1,
                    HorizontalAlignment = HorizontalAlignment.Right,
                };
                if (ev.Forecast is not null) fStack.Children.Add(Label($"F: {ev.Forecast}", 10, Br.TextSecondary));
                if (ev.Previous is not null) fStack.Children.Add(Label($"P: {ev.Previous}", 10, Br.TextMuted));
                Grid.SetColumn(fStack, 3);
                row.Children.Add(fStack);
            }
            stack.Children.Add(row);
        }
        card.Child = stack;
        return card;
    }

    // =========================================================================
    //  CARD 9 — SECTOR PERFORMANCE
    //  FIX v1: Label(sec.Name) was added with SetColumn on discarded return value
    //          → name never appeared in the grid at column 0
    // =========================================================================
    private static UIElement? BuildSectors(SectorPayload? p)
    {
        if (p is null || p.Sectors.Count == 0) return null;
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(BoldLabel("SECTOR PERFORMANCE", 13, Br.Yellow, spacing: 60));

        double maxAbs = p.Sectors.Max(s => Math.Abs(s.ChangePct));
        if (maxAbs < 0.001) maxAbs = 1;

        foreach (var sec in p.Sectors.OrderByDescending(s => s.ChangePct))
        {
            bool up = sec.ChangePct >= 0;
            double pct = Math.Clamp(Math.Abs(sec.ChangePct) / maxAbs, 0.01, 1.0);
            double rest = Math.Max(0.01, 1.0 - pct);

            var row = new Grid { ColumnSpacing = 8 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130, GridUnitType.Pixel) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });

            // FIX: store the TextBlock, set its column, then add it
            var nameLbl = Label(sec.Name, 12, Br.TextSecondary);
            Grid.SetColumn(nameLbl, 0);
            row.Children.Add(nameLbl);

            var barBg = new Border
            {
                Background = Br.SubtleBg,
                CornerRadius = new CornerRadius(3),
                Height = 8,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var barGrid = new Grid();
            barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pct, GridUnitType.Star) });
            barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(rest, GridUnitType.Star) });
            // FIX Bug 9: v1 placed negative-sector fills in column 1 (width = rest = 1-pct)
            // so negative bars were drawn at (1-pct) width instead of pct width — the
            // magnitude was inverted.  Both positive and negative bars simply fill column 0
            // (the pct-wide column); colour alone distinguishes direction.
            var barFill = new Border
            {
                Background = up ? Br.Green : Br.Red,
                CornerRadius = new CornerRadius(3),
                Height = 8,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            Grid.SetColumn(barFill, 0);
            barGrid.Children.Add(barFill);
            barBg.Child = barGrid;
            Grid.SetColumn(barBg, 1);
            row.Children.Add(barBg);

            var pctLbl = Label($"{(up ? "+" : "")}{sec.ChangePct:F2}%", 11,
                up ? Br.Green : Br.Red);
            pctLbl.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(pctLbl, 2);
            row.Children.Add(pctLbl);

            stack.Children.Add(row);
        }
        card.Child = stack;
        return card;
    }

    // =========================================================================
    //  CARD 10 — TRADE SIGNAL
    // =========================================================================
    private static UIElement? BuildTradeSignal(TradeSignalPayload? p)
    {
        if (p is null) return null;

        var (sigColor, sigDim) = p.Signal.ToUpperInvariant() switch
        {
            "BUY" => (Br.Green, Br.GreenDim),
            "SELL" => (Br.Red, Br.RedDim),
            _ => (Br.Yellow, Br.YellowDim),
        };

        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        // Signal hero row
        var sigRow = TwoColGrid();
        var sigStack = new StackPanel { Spacing = 2 };
        sigStack.Children.Add(Label("TRADE SIGNAL", 10, Br.TextMuted, spacing: 60));
        sigStack.Children.Add(BoldLabel(p.Ticker.ToUpper(), 20, Br.Yellow, spacing: 30));
        Grid.SetColumn(sigStack, 0);
        sigRow.Children.Add(sigStack);

        var sigBadge = new Border
        {
            Background = sigDim,
            BorderBrush = sigColor,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16, 8, 16, 8),
            VerticalAlignment = VerticalAlignment.Center,
            Child = BoldLabel(p.Signal.ToUpperInvariant(), 16, sigColor, spacing: 80),
        };
        Grid.SetColumn(sigBadge, 1);
        sigRow.Children.Add(sigBadge);
        stack.Children.Add(sigRow);

        // Confidence bar
        if (p.Confidence.HasValue)
        {
            stack.Children.Add(Divider(12));
            var confRow = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 4) };
            var confHeader = TwoColGrid();
            confHeader.Children.Add(Label("CONFIDENCE", 10, Br.TextMuted, spacing: 40));
            var confPct = Label($"{p.Confidence:F0}%", 10, sigColor);
            confPct.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(confPct, 1);
            confHeader.Children.Add(confPct);
            confRow.Children.Add(confHeader);

            double filled = Math.Clamp(p.Confidence.Value / 100.0, 0.01, 1.0);
            double unfilled = Math.Max(0.01, 1.0 - filled);
            var barBg = new Border { Background = Br.SubtleBg, CornerRadius = new CornerRadius(4), Height = 8 };
            var barGrid = new Grid();
            barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(filled, GridUnitType.Star) });
            barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(unfilled, GridUnitType.Star) });
            barGrid.Children.Add(new Border { Background = sigColor, CornerRadius = new CornerRadius(4), Height = 8 });
            barBg.Child = barGrid;
            confRow.Children.Add(barBg);
            stack.Children.Add(confRow);
        }

        stack.Children.Add(Divider(12));

        var levels = new List<(string, string?)>
        {
            ("ENTRY",     p.Entry.HasValue    ? $"${p.Entry:N2}"    : null),
            ("STOP LOSS", p.StopLoss.HasValue ? $"${p.StopLoss:N2}" : null),
            ("TARGET",    p.Target.HasValue   ? $"${p.Target:N2}"   : null),
            ("TIMEFRAME", p.Timeframe),
        };
        var levelColors = new Dictionary<string, SolidColorBrush>
        {
            ["STOP LOSS"] = Br.Red,
            ["TARGET"] = Br.Green,
        };
        stack.Children.Add(MetricsGrid(levels, levelColors));

        if (p.Reasons.Count > 0)
        {
            stack.Children.Add(Divider(12));
            stack.Children.Add(Label("REASONING", 10, Br.TextMuted,
                spacing: 40, margin: new Thickness(0, 0, 0, 6)));
            foreach (var reason in p.Reasons)
            {
                var rRow = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Margin = new Thickness(0, 0, 0, 4),
                };
                rRow.Children.Add(new Border
                {
                    Width = 4,
                    Height = 4,
                    CornerRadius = new CornerRadius(2),
                    Background = sigColor,
                    VerticalAlignment = VerticalAlignment.Center,
                });
                rRow.Children.Add(Label(reason, 12, Br.TextSecondary));
                stack.Children.Add(rRow);
            }
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 11 — COMPARISON
    //  FIX v1: headers were derived from first metric's *values* (wrong).
    //          Model now has an explicit `headers` array from the backend.
    //          Fallback: auto-generate "Item 1", "Item 2" etc. if absent.
    // =========================================================================
    private static UIElement? BuildComparison(ComparisonPayload? p)
    {
        if (p is null || p.Metrics.Count == 0) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        stack.Children.Add(BoldLabel(p.Title.ToUpper(), 13, Br.Yellow,
            spacing: 60, margin: new Thickness(0, 0, 0, 12)));

        int valCount = p.Metrics.Max(m => m.Values.Count);

        // Build column headers: ["METRIC", header1, header2, …]
        var colHeaders = new string[valCount + 1];
        colHeaders[0] = "METRIC";
        for (int i = 0; i < valCount; i++)
            colHeaders[i + 1] = i < p.Headers.Count ? p.Headers[i].ToUpper() : $"ITEM {i + 1}";

        stack.Children.Add(TableHeader(colHeaders));

        foreach (var metric in p.Metrics)
        {
            var row = new string[valCount + 1];
            row[0] = metric.Label.ToUpper();
            for (int i = 0; i < valCount; i++)
                row[i + 1] = i < metric.Values.Count ? metric.Values[i] : "—";
            stack.Children.Add(TableRow(row, new Dictionary<int, SolidColorBrush>()));
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 12 — FILE ANALYSIS
    // =========================================================================
    private static UIElement? BuildFileAnalysis(FileAnalysisPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        var fileRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 12),
        };
        var iconBorder = new Border
        {
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(8),
            Background = Br.YellowDim,
            BorderBrush = Br.Yellow,
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = p.FileType.ToUpper() switch { "PDF" => "PDF", "CSV" => "CSV", "XLSX" => "XLS", _ => "DOC" },
                FontFamily = F.Bold,
                FontSize = 9,
                Foreground = Br.Yellow,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        fileRow.Children.Add(iconBorder);
        var fileInfo = new StackPanel { Spacing = 1 };
        fileInfo.Children.Add(Label("FILE ANALYSIS", 10, Br.TextMuted, spacing: 60));
        fileInfo.Children.Add(Label(p.Filename, 13, Br.TextPrimary));
        fileRow.Children.Add(fileInfo);
        stack.Children.Add(fileRow);
        stack.Children.Add(Divider(0, new Thickness(0, 0, 0, 10)));

        if (p.KeyPoints.Count > 0)
        {
            stack.Children.Add(Label("KEY POINTS", 10, Br.TextMuted,
                spacing: 40, margin: new Thickness(0, 0, 0, 6)));
            foreach (var pt in p.KeyPoints)
            {
                var ptRow = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Margin = new Thickness(0, 0, 0, 5),
                };
                ptRow.Children.Add(new Border
                {
                    Width = 4,
                    Height = 4,
                    CornerRadius = new CornerRadius(2),
                    Background = Br.Yellow,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 1, 0, 0),
                });
                ptRow.Children.Add(new TextBlock
                {
                    Text = pt,
                    FontFamily = F.Regular,
                    FontSize = 13,
                    Foreground = Br.TextSecondary,
                    TextWrapping = TextWrapping.Wrap,
                });
                stack.Children.Add(ptRow);
            }
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 13 — KNOWLEDGE BASE
    // =========================================================================
    private static UIElement? BuildKnowledgeBase(KnowledgeBasePayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = new Border
        {
            Background = Br.KbBg,
            BorderBrush = Br.Blue,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(18, 14, 18, 14),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel { Spacing = 8 };

        var metaRow = TwoColGrid();
        var sourceStack = new StackPanel { Spacing = 1 };
        sourceStack.Children.Add(Label("KNOWLEDGE BASE", 10, Br.Blue, spacing: 60));
        sourceStack.Children.Add(Label(p.Source, 12, Br.TextSecondary));
        Grid.SetColumn(sourceStack, 0);
        metaRow.Children.Add(sourceStack);

        if (p.Relevance.HasValue)
        {
            var relLabel = Label($"{p.Relevance:P0} match", 11, Br.Blue);
            relLabel.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(relLabel, 1);
            metaRow.Children.Add(relLabel);
        }
        stack.Children.Add(metaRow);
        stack.Children.Add(Divider(0));

        var quoteBar = new Border
        {
            BorderBrush = Br.Blue,
            BorderThickness = new Thickness(3, 0, 0, 0),
            Padding = new Thickness(10, 2, 0, 2),
            Child = new TextBlock
            {
                Text = p.Snippet,
                FontFamily = F.Regular,
                FontSize = 13,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                Foreground = Br.TextSecondary,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
            },
        };
        stack.Children.Add(quoteBar);

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 14 — ERROR / WARNING / INFO
    // =========================================================================
    private static UIElement? BuildError(ErrorPayload? p)
    {
        if (p is null) return null;
        var (accent, dim) = p.Level switch
        {
            "warning" => (Br.Orange, Br.OrangeDim),
            "info" => (Br.Blue, Br.BlueDim),
            _ => (Br.Red, Br.RedDim),
        };
        var card = new Border
        {
            Background = dim,
            BorderBrush = accent,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16, 12, 16, 12),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel { Spacing = 6 };
        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        header.Children.Add(new Border
        {
            Width = 3,
            CornerRadius = new CornerRadius(2),
            Background = accent,
            VerticalAlignment = VerticalAlignment.Stretch,
        });
        var titleStack = new StackPanel { Spacing = 1 };
        titleStack.Children.Add(Label(p.Level.ToUpper(), 9, accent, spacing: 80));
        titleStack.Children.Add(BoldLabel(p.Title, 14, accent));
        header.Children.Add(titleStack);
        stack.Children.Add(header);
        stack.Children.Add(new TextBlock
        {
            Text = p.Message,
            FontFamily = F.Regular,
            FontSize = 13,
            Foreground = Br.TextSecondary,
            TextWrapping = TextWrapping.Wrap,
        });
        if (!string.IsNullOrWhiteSpace(p.Code))
            stack.Children.Add(new Border
            {
                Background = Br.CodeBg,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6),
                Child = new TextBlock
                {
                    Text = p.Code,
                    FontFamily = F.Mono,
                    FontSize = 11,
                    Foreground = Br.TextSecondary,
                },
            });
        card.Child = stack;
        return card;
    }

    // =========================================================================
    //  CARD 15 — TASK PROGRESS
    //  FIX v1: progress bar used GridLength(0) Star which crashes WinUI layout
    //          → all Star values now clamped to [0.01, ∞)
    // =========================================================================
    private static UIElement? BuildTaskProgress(TaskProgressPayload? p)
    {
        if (p is null) return null;
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        int done = p.Steps.Count(s => s.State == "done");
        int total = p.Steps.Count;

        var headerRow = TwoColGrid();
        headerRow.Children.Add(BoldLabel(p.Title.ToUpper(), 13, Br.Yellow, spacing: 50));
        var countLbl = Label($"{done}/{total} Complete", 11, Br.TextMuted);
        countLbl.HorizontalAlignment = HorizontalAlignment.Right;
        Grid.SetColumn(countLbl, 1);
        headerRow.Children.Add(countLbl);
        stack.Children.Add(headerRow);

        // Progress bar — clamp both star values away from 0
        double filled = total > 0 ? Math.Clamp((double)done / total, 0.01, 1.0) : 0.01;
        double unfilled = Math.Max(0.01, 1.0 - filled);
        var progressBg = new Border
        {
            Background = Br.SubtleBg,
            CornerRadius = new CornerRadius(4),
            Height = 6,
            Margin = new Thickness(0, 8, 0, 14),
        };
        var progressGrid = new Grid();
        progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(filled, GridUnitType.Star) });
        progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(unfilled, GridUnitType.Star) });
        progressGrid.Children.Add(new Border
        {
            Background = Br.Yellow,
            CornerRadius = new CornerRadius(4),
            Height = 6,
        });
        progressBg.Child = progressGrid;
        stack.Children.Add(progressBg);

        for (int i = 0; i < p.Steps.Count; i++)
        {
            var step = p.Steps[i];
            var (dotBrush, lblBrush) = step.State switch
            {
                "done" => (Br.Green, Br.TextPrimary),
                "active" => (Br.Yellow, Br.TextPrimary),
                "error" => (Br.Red, Br.Red),
                _ => (Br.DividerBrush, Br.TextMuted),
            };
            bool isLast = i == p.Steps.Count - 1;
            var stepRow = new Grid { Margin = new Thickness(0, 0, 0, isLast ? 0 : 2) };
            stepRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28, GridUnitType.Pixel) });
            stepRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });

            var dotCol = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            dotCol.Children.Add(new Border
            {
                Width = 10,
                Height = 10,
                CornerRadius = new CornerRadius(5),
                Background = dotBrush,
                Margin = new Thickness(0, 3, 0, 0),
            });
            if (!isLast)
                dotCol.Children.Add(new Border
                {
                    Width = 2,
                    Height = 18,
                    Background = Br.DividerBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0),
                });
            Grid.SetColumn(dotCol, 0);
            stepRow.Children.Add(dotCol);

            var stepLbl = Label(step.Label, 13, lblBrush, margin: new Thickness(0, 3, 0, 0));
            Grid.SetColumn(stepLbl, 1);
            stepRow.Children.Add(stepLbl);
            stack.Children.Add(stepRow);
        }
        card.Child = stack;
        return card;
    }

    // =========================================================================
    //  CARD 16 — FLIGHT
    // =========================================================================
    private static UIElement? BuildFlight(FlightPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = new Border
        {
            Background = Br.FlightBg,
            BorderBrush = Br.Teal,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel { Spacing = 0 };

        var hdr = TwoColGrid();
        var hdrLeft = new StackPanel { Spacing = 1 };
        hdrLeft.Children.Add(Label("FLIGHT", 10, Br.Teal, spacing: 60));
        hdrLeft.Children.Add(BoldLabel(
            $"{p.Outbound.FromCode}  —  {p.Outbound.ToCode}", 20, Br.TextPrimary, spacing: 20));
        Grid.SetColumn(hdrLeft, 0);
        hdr.Children.Add(hdrLeft);

        if (p.Price.HasValue)
        {
            var priceStack = new StackPanel { Spacing = 1, HorizontalAlignment = HorizontalAlignment.Right };
            priceStack.Children.Add(Label("FROM", 9, Br.TextMuted, spacing: 60));
            priceStack.Children.Add(BoldLabel($"{p.Currency} {p.Price:N0}", 18, Br.Teal));
            Grid.SetColumn(priceStack, 1);
            hdr.Children.Add(priceStack);
        }
        stack.Children.Add(hdr);
        stack.Children.Add(Divider(14));

        static StackPanel RenderLeg(FlightLeg leg, string label)
        {
            var legStack = new StackPanel { Spacing = 10 };
            legStack.Children.Add(Label(label, 10, Br.TextMuted, spacing: 60));

            var timeline = new Grid { ColumnSpacing = 0 };
            timeline.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            timeline.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
            timeline.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var depart = new StackPanel { Spacing = 2 };
            depart.Children.Add(BoldLabel(leg.DepartTime, 22, Br.TextPrimary));
            depart.Children.Add(BoldLabel(leg.FromCode, 14, Br.Teal));
            depart.Children.Add(Label(leg.FromCity, 11, Br.TextSecondary));
            Grid.SetColumn(depart, 0);
            timeline.Children.Add(depart);

            var mid = new StackPanel
            {
                Spacing = 4,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 12, 0),
            };
            mid.Children.Add(Label(leg.Duration, 10, Br.TextMuted));
            mid.Children.Add(new Border
            {
                Height = 1,
                Background = Br.Border,
                Margin = new Thickness(0, 2, 0, 2),
            });
            mid.Children.Add(Label(
                leg.Stops == 0 ? "Non-stop" : $"{leg.Stops} stop{(leg.Stops > 1 ? "s" : "")}",
                10, leg.Stops == 0 ? Br.Green : Br.Orange));
            Grid.SetColumn(mid, 1);
            timeline.Children.Add(mid);

            var arrive = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Right };
            arrive.Children.Add(BoldLabel(leg.ArriveTime, 22, Br.TextPrimary));
            arrive.Children.Add(BoldLabel(leg.ToCode, 14, Br.Teal));
            arrive.Children.Add(Label(leg.ToCity, 11, Br.TextSecondary));
            Grid.SetColumn(arrive, 2);
            timeline.Children.Add(arrive);
            legStack.Children.Add(timeline);

            var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            meta.Children.Add(Label(leg.Airline, 11, Br.TextSecondary));
            meta.Children.Add(Label(leg.FlightNumber, 11, Br.TextMuted));
            meta.Children.Add(Label(leg.Date, 11, Br.TextMuted));
            legStack.Children.Add(meta);
            return legStack;
        }

        stack.Children.Add(RenderLeg(p.Outbound, "OUTBOUND"));
        if (p.ReturnLeg is not null)
        {
            stack.Children.Add(Divider(14));
            stack.Children.Add(RenderLeg(p.ReturnLeg, "RETURN"));
        }
        stack.Children.Add(Divider(14));

        var footer = TwoColGrid();
        footer.Children.Add(Badge(p.Cabin.ToUpper(), Br.Teal, Br.TealDim));
        if (!string.IsNullOrWhiteSpace(p.BookingRef))
        {
            var refLbl = Label($"REF: {p.BookingRef}", 11, Br.TextMuted);
            refLbl.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(refLbl, 1);
            footer.Children.Add(refLbl);
        }
        stack.Children.Add(footer);

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 17 — RESTAURANT
    // =========================================================================
    private static UIElement? BuildRestaurant(RestaurantPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = new Border
        {
            Background = Br.RestaurantBg,
            BorderBrush = Br.Orange,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel { Spacing = 0 };

        var hdr = TwoColGrid();
        var nameStack = new StackPanel { Spacing = 2 };
        nameStack.Children.Add(Label(p.Cuisine.ToUpper(), 10, Br.Orange, spacing: 60));
        nameStack.Children.Add(BoldLabel(p.Name, 18, Br.TextPrimary));
        Grid.SetColumn(nameStack, 0);
        hdr.Children.Add(nameStack);

        var priceLabel = BoldLabel(p.PriceRange, 18, Br.Orange);
        priceLabel.HorizontalAlignment = HorizontalAlignment.Right;
        priceLabel.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(priceLabel, 1);
        hdr.Children.Add(priceLabel);
        stack.Children.Add(hdr);

        if (p.Rating.HasValue)
        {
            var ratingRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 8, 0, 0),
            };
            var dotsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
                VerticalAlignment = VerticalAlignment.Center,
            };
            for (int i = 0; i < 5; i++)
            {
                double threshold = i + 1;
                var dotBrush = p.Rating.Value >= threshold ? Br.Orange
                             : p.Rating.Value >= threshold - 0.5 ? C.B(Color.FromArgb(200, 0xE0, 0x8C, 0x4A))
                             : Br.StarEmpty;
                dotsPanel.Children.Add(new Border
                {
                    Width = 8,
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Background = dotBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
            ratingRow.Children.Add(dotsPanel);
            ratingRow.Children.Add(BoldLabel($"{p.Rating:F1}", 14, Br.Orange));
            if (p.ReviewCount.HasValue)
                ratingRow.Children.Add(Label($"({p.ReviewCount:N0} reviews)", 11, Br.TextMuted));
            stack.Children.Add(ratingRow);
        }
        stack.Children.Add(Divider(12));

        foreach (var (lbl, val) in new[] { ("ADDRESS", p.Address), ("PHONE", p.Phone), ("HOURS", p.Hours) }
            .Where(x => x.Item2 is not null))
        {
            var row = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 0, 6) };
            row.Children.Add(Label(lbl, 10, Br.TextMuted, spacing: 40));
            row.Children.Add(new TextBlock
            {
                Text = val,
                FontFamily = F.Regular,
                FontSize = 12,
                Foreground = Br.TextSecondary,
                TextWrapping = TextWrapping.Wrap,
            });
            stack.Children.Add(row);
        }

        if (p.IsOpen.HasValue)
        {
            var openRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 4, 0, 10),
            };
            openRow.Children.Add(new Border
            {
                Width = 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = p.IsOpen.Value ? Br.Green : Br.Red,
                VerticalAlignment = VerticalAlignment.Center,
            });
            openRow.Children.Add(Label(
                p.IsOpen.Value ? "Open now" : "Closed", 12,
                p.IsOpen.Value ? Br.Green : Br.Red));
            stack.Children.Add(openRow);
        }

        if (p.TopDishes.Count > 0)
        {
            stack.Children.Add(Label("TOP DISHES", 10, Br.TextMuted,
                spacing: 40, margin: new Thickness(0, 0, 0, 6)));
            var wrap = new VariableSizedWrapGrid
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 28,
                MaximumRowsOrColumns = 20,
            };
            foreach (var dish in p.TopDishes)
                wrap.Children.Add(new Border
                {
                    Background = Br.OrangeDim,
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(0, 0, 6, 6),
                    Child = Label(dish, 11, Br.Orange),
                });
            stack.Children.Add(wrap);
        }

        if (p.Features.Count > 0)
        {
            stack.Children.Add(Divider(8));
            var wrap = new VariableSizedWrapGrid
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 26,
                MaximumRowsOrColumns = 20,
            };
            foreach (var feat in p.Features)
                wrap.Children.Add(new Border
                {
                    Background = Br.SubtleBg,
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 0, 6, 0),
                    Child = Label(feat, 11, Br.TextSecondary),
                });
            stack.Children.Add(wrap);
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 18 — RENTAL CAR
    // =========================================================================
    private static UIElement? BuildRentalCar(RentalCarPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        var hdr = TwoColGrid();
        var infoStack = new StackPanel { Spacing = 2 };
        infoStack.Children.Add(Label(p.Company.ToUpper(), 10, Br.TextMuted, spacing: 60));
        infoStack.Children.Add(BoldLabel(p.Vehicle, 16, Br.TextPrimary));
        infoStack.Children.Add(Badge(p.Category, Br.Yellow, Br.YellowDim));
        Grid.SetColumn(infoStack, 0);
        hdr.Children.Add(infoStack);

        if (p.TotalPrice.HasValue)
        {
            var priceStack = new StackPanel { Spacing = 1, HorizontalAlignment = HorizontalAlignment.Right };
            priceStack.Children.Add(BoldLabel($"{p.Currency} {p.TotalPrice:N0}", 20, Br.Yellow));
            if (p.PricePerDay.HasValue && p.Days.HasValue)
                priceStack.Children.Add(Label($"{p.Currency} {p.PricePerDay:N0}/day  x  {p.Days}", 10, Br.TextMuted));
            Grid.SetColumn(priceStack, 1);
            hdr.Children.Add(priceStack);
        }
        stack.Children.Add(hdr);
        stack.Children.Add(Divider(14));

        var details = new List<(string, string?)>
        {
            ("PICKUP DATE",  p.PickupDate),
            ("DROP-OFF",     p.DropoffDate),
            ("PICKUP LOC",   p.PickupLoc),
            ("DROP-OFF LOC", p.DropoffLoc),
        };
        stack.Children.Add(MetricsGrid(details, new Dictionary<string, SolidColorBrush>()));

        if (p.Inclusions.Count > 0)
        {
            stack.Children.Add(Divider(12));
            stack.Children.Add(Label("INCLUSIONS", 10, Br.TextMuted,
                spacing: 40, margin: new Thickness(0, 0, 0, 6)));
            var wrap = new VariableSizedWrapGrid
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 26,
                MaximumRowsOrColumns = 20,
            };
            foreach (var inc in p.Inclusions)
                wrap.Children.Add(new Border
                {
                    Background = Br.GreenDim,
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 0, 6, 0),
                    Child = Label(inc, 11, Br.Green),
                });
            stack.Children.Add(wrap);
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 19 — WEATHER
    // =========================================================================
    private static UIElement? BuildWeather(WeatherPayload? p)
    {
        if (p is null) return null;
        var accent = ConditionColor(p.Condition);
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        // Location + condition
        var locRow = TwoColGrid();
        var locStack = new StackPanel { Spacing = 2 };
        locStack.Children.Add(Label("WEATHER", 10, C.B(accent), spacing: 60));
        locStack.Children.Add(BoldLabel(p.Location, 16, Br.TextPrimary));
        locStack.Children.Add(Label(p.Condition, 12, Br.TextSecondary));
        Grid.SetColumn(locStack, 0);
        locRow.Children.Add(locStack);

        // Current temperature
        var tempStr = TempStr(p.TempC, p.TempF, p.UseFahrenheit);
        var tempLbl = BoldLabel(tempStr, 36, C.B(accent));
        tempLbl.HorizontalAlignment = HorizontalAlignment.Right;
        tempLbl.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(tempLbl, 1);
        locRow.Children.Add(tempLbl);
        stack.Children.Add(locRow);

        stack.Children.Add(Divider(12));

        // Conditions grid
        var condMetrics = new List<(string, string?)>
        {
            ("FEELS LIKE",  TempStr(p.FeelsLikeC, p.FeelsLikeF, p.UseFahrenheit)),
            ("HUMIDITY",    p.Humidity.HasValue ? $"{p.Humidity}%" : null),
            ("WIND",        p.WindKph.HasValue  ? $"{p.WindKph:F0} km/h {p.WindDir}" : null),
            ("UV INDEX",    p.UvIndex.HasValue  ? $"{p.UvIndex}" : null),
            ("VISIBILITY",  p.Visibility),
        };
        stack.Children.Add(MetricsGrid(condMetrics, new Dictionary<string, SolidColorBrush>()));

        // 5-day forecast strip
        if (p.Forecast.Count > 0)
        {
            stack.Children.Add(Divider(14));
            var forecastGrid = new Grid { ColumnSpacing = 4 };
            for (int i = 0; i < p.Forecast.Count; i++)
                forecastGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });

            for (int i = 0; i < p.Forecast.Count; i++)
            {
                var day = p.Forecast[i];
                var dayAccent = ConditionColor(day.Condition);

                var dayStack = new StackPanel
                {
                    Spacing = 4,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                dayStack.Children.Add(BoldLabel(day.Day, 10,
                    i == 0 ? C.B(accent) : Br.TextMuted, spacing: 20));
                dayStack.Children.Add(new Border
                {
                    Width = 8,
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Background = C.B(dayAccent),
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
                dayStack.Children.Add(Label(TempStr(day.HighC, day.HighF, p.UseFahrenheit), 12, Br.TextPrimary));
                dayStack.Children.Add(Label(TempStr(day.LowC, day.LowF, p.UseFahrenheit), 11, Br.TextMuted));
                if (day.PrecipPct.HasValue)
                    dayStack.Children.Add(Label($"{day.PrecipPct}%", 10, Br.Sky));

                var dayCell = new Border
                {
                    Child = dayStack,
                    Padding = new Thickness(4, 8, 4, 8),
                    CornerRadius = new CornerRadius(8),
                    Background = i == 0 ? Br.SubtleBg : Br.Transparent,
                };
                Grid.SetColumn(dayCell, i);
                forecastGrid.Children.Add(dayCell);
            }
            stack.Children.Add(forecastGrid);
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 20 — HOTEL
    // =========================================================================
    private static UIElement? BuildHotel(HotelPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = new Border
        {
            Background = Br.HotelBg,
            BorderBrush = Br.Purple,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel { Spacing = 0 };

        var hdr = TwoColGrid();
        var nameStack = new StackPanel { Spacing = 4 };
        nameStack.Children.Add(Label("HOTEL", 10, Br.Purple, spacing: 60));
        nameStack.Children.Add(BoldLabel(p.Name, 17, Br.TextPrimary));
        if (p.Stars.HasValue)
        {
            var starsRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
            for (int i = 0; i < 5; i++)
                starsRow.Children.Add(new Border
                {
                    Width = 7,
                    Height = 7,
                    CornerRadius = new CornerRadius(3.5f),
                    Background = i < p.Stars.Value ? Br.Yellow : Br.StarEmpty,
                });
            nameStack.Children.Add(starsRow);
        }
        Grid.SetColumn(nameStack, 0);
        hdr.Children.Add(nameStack);

        if (p.TotalPrice.HasValue)
        {
            var priceStack = new StackPanel { Spacing = 1, HorizontalAlignment = HorizontalAlignment.Right };
            priceStack.Children.Add(BoldLabel($"{p.Currency} {p.TotalPrice:N0}", 18, Br.Purple));
            if (p.PricePerNight.HasValue && p.Nights.HasValue)
                priceStack.Children.Add(Label(
                    $"{p.Currency} {p.PricePerNight:N0}/night  x  {p.Nights}", 10, Br.TextMuted));
            Grid.SetColumn(priceStack, 1);
            hdr.Children.Add(priceStack);
        }
        stack.Children.Add(hdr);

        if (p.Rating.HasValue)
        {
            var ratingRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 8, 0, 0),
            };
            ratingRow.Children.Add(new Border
            {
                Background = Br.PurpleDim,
                BorderBrush = Br.Purple,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 3, 8, 3),
                Child = BoldLabel($"{p.Rating:F1}", 14, Br.Purple),
            });
            ratingRow.Children.Add(Label("/ 10", 12, Br.TextMuted));
            if (p.ReviewCount.HasValue)
                ratingRow.Children.Add(Label($"— {p.ReviewCount:N0} reviews", 11, Br.TextMuted));
            stack.Children.Add(ratingRow);
        }
        stack.Children.Add(Divider(12));

        foreach (var (lbl, val) in new[]
            {
                ("CHECK-IN",  p.CheckIn),
                ("CHECK-OUT", p.CheckOut),
                ("ROOM",      p.RoomType),
                ("ADDRESS",   p.Address),
            }.Where(x => x.Item2 is not null))
        {
            var row = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 0, 8) };
            row.Children.Add(Label(lbl, 10, Br.TextMuted, spacing: 40));
            row.Children.Add(new TextBlock
            {
                Text = val,
                FontFamily = F.Regular,
                FontSize = 12,
                Foreground = Br.TextSecondary,
                TextWrapping = TextWrapping.Wrap,
            });
            stack.Children.Add(row);
        }

        if (p.Amenities.Count > 0)
        {
            stack.Children.Add(Divider(4));
            stack.Children.Add(Label("AMENITIES", 10, Br.TextMuted,
                spacing: 40, margin: new Thickness(0, 8, 0, 6)));
            var wrap = new VariableSizedWrapGrid
            {
                Orientation = Orientation.Horizontal,
                ItemHeight = 26,
                MaximumRowsOrColumns = 20,
            };
            foreach (var a in p.Amenities)
                wrap.Children.Add(new Border
                {
                    Background = Br.PurpleDim,
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 0, 6, 0),
                    Child = Label(a, 10, Br.Purple),
                });
            stack.Children.Add(wrap);
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 21 — DIRECTIONS
    // =========================================================================
    private static UIElement? BuildDirections(DirectionsPayload? p)
    {
        if (p is null) return null;
        var panel = new StackPanel { Spacing = 0 };
        var card = new Border
        {
            Background = Br.DirectionsBg,
            BorderBrush = Br.Green,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20, 16, 20, 16),
            Margin = new Thickness(0, 0, 0, 10),
        };
        var stack = new StackPanel { Spacing = 0 };

        string modeLabel = p.Mode.ToLowerInvariant() switch
        {
            "walking" => "WALKING",
            "transit" => "TRANSIT",
            "cycling" => "CYCLING",
            _ => "DRIVING",
        };
        var hdr = TwoColGrid();
        var routeStack = new StackPanel { Spacing = 4 };
        routeStack.Children.Add(Badge(modeLabel, Br.Green, Br.GreenDim));
        routeStack.Children.Add(BoldLabel(p.Origin, 13, Br.TextSecondary));
        routeStack.Children.Add(Label("↓", 12, Br.TextMuted));
        routeStack.Children.Add(BoldLabel(p.Destination, 13, Br.TextPrimary));
        Grid.SetColumn(routeStack, 0);
        hdr.Children.Add(routeStack);

        if (p.TotalDuration is not null || p.TotalDistance is not null)
        {
            var etaStack = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Right };
            if (p.TotalDuration is not null)
                etaStack.Children.Add(BoldLabel(p.TotalDuration, 16, Br.Green));
            if (p.TotalDistance is not null)
                etaStack.Children.Add(Label(p.TotalDistance, 11, Br.TextMuted));
            Grid.SetColumn(etaStack, 1);
            hdr.Children.Add(etaStack);
        }
        stack.Children.Add(hdr);
        stack.Children.Add(Divider(14));

        for (int i = 0; i < p.Steps.Count; i++)
        {
            var step = p.Steps[i];
            bool isLast = i == p.Steps.Count - 1;
            var stepRow = new Grid { Margin = new Thickness(0, 0, 0, isLast ? 0 : 8) };
            stepRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24, GridUnitType.Pixel) });
            stepRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });

            var dotCol = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            dotCol.Children.Add(new Border
            {
                Width = 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = i == 0 ? Br.Green : Br.SubtleBg,
                Margin = new Thickness(0, 3, 0, 0),
            });
            if (!isLast)
                dotCol.Children.Add(new Border
                {
                    Width = 2,
                    Height = 20,
                    Background = Br.DividerBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0),
                });
            Grid.SetColumn(dotCol, 0);
            stepRow.Children.Add(dotCol);

            var textStack = new StackPanel { Spacing = 2, Margin = new Thickness(0, 2, 0, 0) };
            textStack.Children.Add(new TextBlock
            {
                Text = step.Instruction,
                FontFamily = F.Regular,
                FontSize = 13,
                Foreground = Br.TextSecondary,
                TextWrapping = TextWrapping.Wrap,
            });
            if (step.Distance is not null || step.Duration is not null)
            {
                var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                if (step.Distance is not null) meta.Children.Add(Label(step.Distance, 10, Br.TextMuted));
                if (step.Duration is not null) meta.Children.Add(Label(step.Duration, 10, Br.TextMuted));
                textStack.Children.Add(meta);
            }
            Grid.SetColumn(textStack, 1);
            stepRow.Children.Add(textStack);
            stack.Children.Add(stepRow);
        }

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  CARD 22 — CURRENCY
    // =========================================================================
    private static UIElement? BuildCurrency(CurrencyPayload? p)
    {
        if (p is null) return null;
        bool up = (p.ChangePct ?? 0) >= 0;
        var panel = new StackPanel { Spacing = 0 };
        var card = MakeCard();
        var stack = new StackPanel { Spacing = 0 };

        // Header
        var hdr = TwoColGrid();
        var pairStack = new StackPanel { Spacing = 2 };
        pairStack.Children.Add(Label("EXCHANGE RATE", 10, Br.TextMuted, spacing: 60));
        pairStack.Children.Add(BoldLabel($"{p.FromCode} / {p.ToCode}", 20, Br.Yellow, spacing: 20));
        pairStack.Children.Add(Label($"{p.FromName}  →  {p.ToName}", 11, Br.TextMuted));
        Grid.SetColumn(pairStack, 0);
        hdr.Children.Add(pairStack);

        var rateStack = new StackPanel { Spacing = 2, HorizontalAlignment = HorizontalAlignment.Right };
        rateStack.Children.Add(BoldLabel($"{p.Rate:F4}", 22, Br.TextPrimary));
        if (p.ChangePct.HasValue)
        {
            var badge = Badge($"{(up ? "+" : "")}{p.ChangePct:F2}%",
                up ? Br.Green : Br.Red,
                up ? Br.GreenDim : Br.RedDim);
            badge.HorizontalAlignment = HorizontalAlignment.Right;
            rateStack.Children.Add(badge);
        }
        Grid.SetColumn(rateStack, 1);
        hdr.Children.Add(rateStack);
        stack.Children.Add(hdr);
        stack.Children.Add(Divider(14));

        // Conversion table
        if (p.Conversions.Count > 0)
        {
            stack.Children.Add(TableHeader([p.FromCode.ToUpper(), p.ToCode.ToUpper()]));
            foreach (var row in p.Conversions)
                stack.Children.Add(TableRow(
                    [$"{row.Amount:N2}", $"{row.Converted:N2}"],
                    new Dictionary<int, SolidColorBrush> { [1] = Br.Yellow }));
        }

        if (p.AsOf is not null)
            stack.Children.Add(Label($"As of {p.AsOf}", 10, Br.TextMuted,
                margin: new Thickness(0, 10, 0, 0)));

        card.Child = stack;
        panel.Children.Add(card);
        if (!string.IsNullOrWhiteSpace(p.Summary))
            panel.Children.Add(SummaryLabel(p.Summary!));
        return panel;
    }

    // =========================================================================
    //  SHARED PRIMITIVES  (all use Br.* — zero brush allocations)
    // =========================================================================

    // Reusable GridLength constants
    private static readonly GridLength GridLengthStar = new(1, GridUnitType.Star);

    private static Border MakeCard() => new()
    {
        Background = Br.Background,
        BorderBrush = Br.Border,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(14),
        Padding = new Thickness(20, 16, 20, 16),
        Margin = new Thickness(0, 0, 0, 10),
    };

    private static Grid TwoColGrid()
    {
        var g = new Grid();
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        return g;
    }

    private static Border Divider(int marginV, Thickness? margin = null) => new()
    {
        Height = 1,
        Background = Br.DividerBrush,
        Margin = margin ?? new Thickness(0, marginV, 0, marginV),
    };

    private static TextBlock Label(
        string text, double size, SolidColorBrush fg,
        int spacing = 0, Thickness? margin = null) => new()
        {
            Text = text,
            FontFamily = F.Regular,
            FontSize = size,
            CharacterSpacing = spacing,
            Foreground = fg,
            Margin = margin ?? new Thickness(0),
        };

    private static TextBlock BoldLabel(
        string text, double size, SolidColorBrush fg,
        int spacing = 0, Thickness? margin = null) => new()
        {
            Text = text,
            FontFamily = F.Bold,
            FontSize = size,
            FontWeight = FontWeights.Bold,
            CharacterSpacing = spacing,
            Foreground = fg,
            Margin = margin ?? new Thickness(0),
        };

    private static Border Badge(string text, SolidColorBrush fg, SolidColorBrush bg) => new()
    {
        Background = bg,
        CornerRadius = new CornerRadius(20),
        Padding = new Thickness(10, 4, 10, 4),
        VerticalAlignment = VerticalAlignment.Top,
        Child = new TextBlock
        {
            Text = text,
            FontFamily = F.Medium,
            FontSize = 11,
            Foreground = fg,
        },
    };

    /// <summary>
    /// 2-column metrics grid. Nulls are skipped entirely (not counted toward layout).
    /// FIX v1: original incremented col on null skip which created phantom empty cells.
    /// </summary>
    private static UIElement MetricsGrid(
        List<(string label, string? val)> metrics,
        Dictionary<string, SolidColorBrush> colourMap)
    {
        var grid = new Grid { ColumnSpacing = 20, RowSpacing = 14 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });

        // Build list of only non-null entries, then lay them out
        var valid = metrics.Where(m => m.val is not null).ToList();
        int totalRows = (valid.Count + 1) / 2;
        for (int r = 0; r < totalRows; r++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < valid.Count; i++)
        {
            var (lbl, val) = valid[i];
            int row = i / 2;
            int col = i % 2;
            var valBrush = colourMap.TryGetValue(lbl, out var c) ? c : Br.TextPrimary;

            var cell = new StackPanel { Spacing = 2 };
            cell.Children.Add(Label(lbl, 10, Br.TextMuted, spacing: 40));
            cell.Children.Add(Label(val!, 14, valBrush));
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            grid.Children.Add(cell);
        }
        return grid;
    }

    private static UIElement TableHeader(string[] cols)
    {
        var grid = new Grid
        {
            Background = Br.SubtleBg,
            Padding = new Thickness(0, 6, 0, 6),
            Margin = new Thickness(0, 0, 0, 2),
        };
        for (int i = 0; i < cols.Length; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
        for (int i = 0; i < cols.Length; i++)
        {
            var tb = Label(cols[i], 10, Br.TextMuted, spacing: 40);
            tb.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(tb, i);
            grid.Children.Add(tb);
        }
        return grid;
    }

    private static UIElement TableRow(
        string[] vals, Dictionary<int, SolidColorBrush> colourOverrides)
    {
        var grid = new Grid { Padding = new Thickness(0, 6, 0, 6) };
        for (int i = 0; i < vals.Length; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLengthStar });
        for (int i = 0; i < vals.Length; i++)
        {
            var fg = colourOverrides.TryGetValue(i, out var c) ? c
                   : i == 0 ? Br.TextSecondary : Br.TextPrimary;
            var tb = Label(vals[i], 13, fg);
            tb.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(tb, i);
            grid.Children.Add(tb);
        }
        return grid;
    }

    private static UIElement LiveFooter()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Thickness(0, 14, 0, 0),
        };
        row.Children.Add(new Border
        {
            Width = 6,
            Height = 6,
            CornerRadius = new CornerRadius(3),
            Background = Br.Green,
            VerticalAlignment = VerticalAlignment.Center,
        });
        row.Children.Add(Label("Live", 11, C.B(Color.FromArgb(180, 180, 200, 180))));
        return row;
    }

    private static Button SmallButton(string text, SolidColorBrush fg, Color bg)
    {
        var btn = new Button
        {
            Content = text,
            FontFamily = F.Bold,
            FontSize = 10,
            Padding = new Thickness(10, 4, 10, 4),
            CornerRadius = new CornerRadius(5),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = fg,
        };
        btn.Resources["ButtonBackground"] = C.B(bg);
        btn.Resources["ButtonBackgroundPointerOver"] = C.B(Color.FromArgb(255,
            (byte)Math.Min(bg.R + 20, 255),
            (byte)Math.Min(bg.G + 20, 255),
            (byte)Math.Min(bg.B + 20, 255)));
        btn.Resources["ButtonBackgroundPressed"] = C.B(bg);
        btn.Resources["ButtonBorderBrush"] = C.B(Color.FromArgb(60, 255, 255, 255));
        btn.Resources["ButtonBorderBrushPointerOver"] = btn.Resources["ButtonBorderBrush"];
        btn.Resources["ButtonBorderBrushPressed"] = btn.Resources["ButtonBorderBrush"];
        btn.Resources["ButtonForeground"] = fg;
        btn.Resources["ButtonForegroundPointerOver"] = fg;
        btn.Resources["ButtonForegroundPressed"] = fg;
        return btn;
    }

    private static TextBlock SummaryLabel(string text) => new()
    {
        Text = text,
        FontFamily = F.Regular,
        FontSize = 14,
        TextWrapping = TextWrapping.Wrap,
        LineHeight = 22,
        Foreground = Br.TextPrimary,
        Margin = new Thickness(4, 4, 4, 0),
    };

    // ── Weather helpers ───────────────────────────────────────────────────────
    private static string TempStr(double? c, double? f, bool useFahrenheit = false)
    {
        if (useFahrenheit && f.HasValue) return $"{f:F0}°F";
        if (c.HasValue) return $"{c:F0}°C";
        if (f.HasValue) return $"{f:F0}°F";
        return "—";
    }

    private static Color ConditionColor(string condition) =>
        condition.ToLowerInvariant() switch
        {
            var s when s.Contains("sun") || s.Contains("clear") => C.Yellow,
            var s when s.Contains("cloud") || s.Contains("overcast") => C.TextSecondary,
            var s when s.Contains("rain") || s.Contains("drizzle") => C.Blue,
            var s when s.Contains("snow") || s.Contains("sleet") => C.Sky,
            var s when s.Contains("storm") || s.Contains("thunder") => C.Orange,
            _ => C.TextMuted,
        };
}