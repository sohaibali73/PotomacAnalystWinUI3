# GenUiCardBuilder — Integration Guide
# PotomacAnalyst  |  WinUI 3

────────────────────────────────────────────────────────────────
  1.  WIRING INTO ChatPage.xaml.cs
────────────────────────────────────────────────────────────────

Replace the existing TryBuildGenUiCard call in FinalizeAiBubble
and AppendRichBubble with the new builder:

    // FinalizeAiBubble — replace the old method body:
    private void FinalizeAiBubble(Border bubble, TextBlock streamLabel, string rawText)
    {
        var cleaned = Clean(rawText);

        if (string.IsNullOrEmpty(cleaned))
        {
            streamLabel.Text       = "No response received.";
            streamLabel.Foreground = (SolidColorBrush)Application.Current.Resources["TextMutedBrush"];
            return;
        }

        // Try Gen UI card first
        var card = GenUiCardBuilder.TryBuild(rawText,
            navigateTo: page => GetMainWindow()?.NavigateTo(page));

        if (card is not null)
        {
            bubble.Child = card;
            return;
        }

        // Fallback: code blocks, then plain text
        if (rawText.Contains("```"))
            bubble.Child = RenderRich(cleaned, rawText);
        else
        {
            streamLabel.Text       = cleaned;
            streamLabel.Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"];
        }
    }

    // AppendRichBubble — replace the old method body:
    private void AppendRichBubble(string cleaned, string raw)
    {
        var bubble = MakeBubble(isUser: false);
        var card   = GenUiCardBuilder.TryBuild(raw, navigateTo: page => GetMainWindow()?.NavigateTo(page));
        bubble.Child = card
            ?? (raw.Contains("```") ? RenderRich(cleaned, raw)
            : (UIElement)new TextBlock
              {
                  Text         = cleaned,
                  FontFamily   = (FontFamily)Application.Current.Resources["QuicksandRegular"],
                  FontSize     = 14,
                  TextWrapping = TextWrapping.Wrap,
                  LineHeight   = 22,
                  Foreground   = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
              });
        MessageList.Children.Add(bubble);
    }

────────────────────────────────────────────────────────────────
  2.  BACKEND CONTRACT
      The backend MUST prepend a JSON envelope to any structured
      response.  The rest of the response (prose summary) follows
      after the closing brace and is rendered as the caption below
      the card.
────────────────────────────────────────────────────────────────

  PLAIN TEXT RESPONSE (no card):
  ───────────────────────────────
  Just return the prose string.  No envelope needed.
  The frontend falls through to the text renderer.

  CARD RESPONSE (all card types):
  ─────────────────────────────────
  {"card":"<type>","data":{...}}

  Optional prose summary follows on a new line AFTER the JSON
  block.  The builder extracts the JSON, leaves the rest as
  the caption TextBlock below the card.

────────────────────────────────────────────────────────────────
  3.  FULL PAYLOAD EXAMPLES FOR EVERY CARD TYPE
────────────────────────────────────────────────────────────────

── CARD 1 — STOCK ──────────────────────────────────────────────
{"card":"stock","data":{
  "ticker":    "AAPL",
  "company":   "Apple Inc.",
  "price":     189.30,
  "change":    2.45,
  "changePct": 1.31,
  "open":      187.20,
  "prevClose": 186.85,
  "high":      190.10,
  "low":       186.50,
  "volume":    "52.3M",
  "marketCap": "$2.94T",
  "summary":   "Apple is trading near a 52-week high ahead of its earnings release."
}}

── CARD 2 — BACKTEST ───────────────────────────────────────────
{"card":"backtest","data":{
  "strategy":    "Dual MA Crossover",
  "symbol":      "BHP.AX",
  "from":        "2020-01-01",
  "to":          "2024-12-31",
  "netProfit":   18420,
  "netProfitPct":84.2,
  "winRate":     62.4,
  "maxDrawdown": 14.7,
  "sharpe":      1.43,
  "totalTrades": 38,
  "avgWin":      920.50,
  "avgLoss":     340.20,
  "summary":     "The strategy outperformed buy-and-hold by 24% over the period."
}}

── CARD 3 — AFL ────────────────────────────────────────────────
{"card":"afl","data":{
  "title":   "Dual MA Crossover — BHP.AX",
  "code":    "// Dual Moving Average Crossover\nperiodFast = Param(\"Fast MA\", 10, 5, 50, 1);\nperiodSlow = Param(\"Slow MA\", 30, 10, 200, 1);\nmaFast = MA(Close, periodFast);\nmaSlow = MA(Close, periodSlow);\nBuy  = Cross(maFast, maSlow);\nSell = Cross(maSlow, maFast);\nPlot(Close, \"Close\", colorBlack);\nPlot(maFast, \"Fast MA\", colorBlue);\nPlot(maSlow, \"Slow MA\", colorRed);",
  "summary": "10/30 EMA crossover with default parameters. Open in the AFL Generator to backtest."
}}

── CARD 4 — PORTFOLIO ──────────────────────────────────────────
{"card":"portfolio","data":{
  "totalValue":  52480.00,
  "totalPnl":    4210.50,
  "totalPnlPct": 8.72,
  "holdings": [
    {"ticker":"BHP",  "shares":100, "avgCost":44.20, "current":48.90, "pnl":470.00, "pnlPct":10.63},
    {"ticker":"CBA",  "shares":50,  "avgCost":98.50, "current":107.20,"pnl":435.00, "pnlPct":8.83},
    {"ticker":"WDS",  "shares":200, "avgCost":26.80, "current":24.10, "pnl":-540.00,"pnlPct":-10.07}
  ],
  "summary": "Portfolio is up 8.72% YTD. WDS is the only underperformer."
}}

── CARD 5 — SCREENER ───────────────────────────────────────────
{"card":"screener","data":{
  "title":   "ASX Top Movers",
  "columns": ["Ticker","Price","Change %","Volume","Market Cap"],
  "rows": [
    ["BHP",  "$48.90", "+3.2%", "18.2M", "$246B"],
    ["RIO",  "$126.40","+2.8%", "6.1M",  "$209B"],
    ["FMG",  "$21.60", "+4.1%", "22.4M", "$66B"]
  ],
  "summary": "Materials sector leading the ASX today."
}}

── CARD 6 — NEWS ───────────────────────────────────────────────
{"card":"news","data":{
  "title": "BHP — Latest News",
  "items": [
    {"headline":"BHP lifts iron ore production guidance for FY25","source":"AFR","time":"2h ago","sentiment":"positive"},
    {"headline":"China PMI data weighs on mining stocks","source":"Bloomberg","time":"4h ago","sentiment":"negative"},
    {"headline":"BHP board approves share buyback extension","source":"Reuters","time":"6h ago","sentiment":"positive"}
  ]
}}

── CARD 7 — WATCHLIST ──────────────────────────────────────────
{"card":"watchlist","data":{
  "tickers": [
    {"ticker":"BHP",  "price":48.90, "changePct":3.2},
    {"ticker":"CBA",  "price":107.20,"changePct":0.8},
    {"ticker":"WDS",  "price":24.10, "changePct":-1.4},
    {"ticker":"CSL",  "price":274.60,"changePct":0.3}
  ]
}}

── CARD 8 — ECONOMIC CALENDAR ──────────────────────────────────
{"card":"economic_calendar","data":{
  "items": [
    {"time":"09:30","event":"RBA Interest Rate Decision","country":"AU","impact":"high","forecast":"4.35%","previous":"4.35%"},
    {"time":"11:30","event":"Employment Change","country":"AU","impact":"high","forecast":"+18.5K","previous":"+35.6K"},
    {"time":"21:30","event":"US Initial Jobless Claims","country":"US","impact":"medium","forecast":"215K","previous":"220K"}
  ]
}}

── CARD 9 — SECTORS ────────────────────────────────────────────
{"card":"sectors","data":{
  "sectors": [
    {"name":"Materials",     "changePct": 2.4},
    {"name":"Energy",        "changePct": 1.8},
    {"name":"Financials",    "changePct": 0.6},
    {"name":"Healthcare",    "changePct":-0.3},
    {"name":"Consumer Disc.","changePct":-1.1},
    {"name":"Technology",    "changePct": 0.9}
  ]
}}

── CARD 10 — TRADE SIGNAL ──────────────────────────────────────
{"card":"trade_signal","data":{
  "ticker":    "BHP",
  "signal":    "BUY",
  "confidence":78.5,
  "entry":     48.90,
  "stopLoss":  46.20,
  "target":    54.50,
  "timeframe": "Swing (5-10 days)",
  "reasons": [
    "Price broke above 50-day MA with volume confirmation",
    "RSI divergence on daily chart",
    "Iron ore futures up 3% overnight"
  ],
  "summary": "Risk/reward ratio of 2.1:1 at current levels."
}}

── CARD 11 — COMPARISON ────────────────────────────────────────
{"card":"comparison","data":{
  "title": "BHP vs RIO vs FMG",
  "metrics": [
    {"label":"Price",       "values":["$48.90", "$126.40", "$21.60"]},
    {"label":"P/E Ratio",   "values":["12.4x",  "9.8x",    "7.2x"]},
    {"label":"Dividend Yld","values":["4.8%",   "5.2%",    "8.1%"]},
    {"label":"Market Cap",  "values":["$246B",  "$209B",   "$66B"]},
    {"label":"YTD Return",  "values":["+18.2%", "+12.4%",  "+24.6%"]}
  ],
  "summary": "FMG offers the highest yield and YTD return but carries more China exposure risk."
}}

── CARD 12 — FILE ANALYSIS ─────────────────────────────────────
{"card":"file_analysis","data":{
  "filename": "BHP_Annual_Report_2024.pdf",
  "fileType": "PDF",
  "keyPoints": [
    "Revenue up 8.4% YoY to $55.7B driven by iron ore and copper",
    "Net profit after tax increased to $13.7B from $12.9B",
    "Final dividend declared at USD $0.74 per share, fully franked",
    "Capex guidance of $10-10.5B for FY25 maintained"
  ],
  "summary": "Strong results underpinned by commodity price recovery and operational efficiency."
}}

── CARD 13 — KNOWLEDGE BASE ────────────────────────────────────
{"card":"knowledge_base","data":{
  "source":    "Potomac Strategy Playbook — Chapter 4",
  "snippet":   "The dual moving average crossover system enters long when the fast MA crosses above the slow MA with volume confirmation exceeding the 20-day average.",
  "relevance": 0.92,
  "summary":   "This matches your query about MA crossover entry conditions."
}}

── CARD 14 — ERROR / WARNING / INFO ────────────────────────────
Error:
{"card":"error","data":{
  "level":   "error",
  "title":   "API Connection Failed",
  "message": "Could not reach the market data provider. Please check your connection and try again.",
  "code":    "ERR_NETWORK_TIMEOUT"
}}

Warning:
{"card":"error","data":{
  "level":   "warning",
  "title":   "Backtest Data Incomplete",
  "message": "Data for CBA.AX between 2019-03-01 and 2019-06-30 is missing. Results may be inaccurate.",
  "code":    null
}}

Info:
{"card":"error","data":{
  "level":   "info",
  "title":   "Market Closed",
  "message": "The ASX is currently closed. Prices shown are end-of-day from the previous session.",
  "code":    null
}}

── CARD 15 — TASK PROGRESS ─────────────────────────────────────
{"card":"task_progress","data":{
  "title": "Running Full Strategy Analysis",
  "steps": [
    {"label":"Fetching market data",          "state":"done"},
    {"label":"Running AFL backtest",          "state":"done"},
    {"label":"Analysing drawdown periods",   "state":"active"},
    {"label":"Generating trade log",         "state":"pending"},
    {"label":"Producing summary report",     "state":"pending"}
  ]
}}

── CARD 16 — FLIGHT ────────────────────────────────────────────
{"card":"flight","data":{
  "cabin":    "Business",
  "price":    1840,
  "currency": "USD",
  "bookingRef": "QF-7X29KL",
  "outbound": {
    "airline":      "Qantas",
    "flightNumber": "QF1",
    "fromCode":     "SYD",
    "fromCity":     "Sydney",
    "toCode":       "LHR",
    "toCity":       "London Heathrow",
    "departTime":   "16:00",
    "arriveTime":   "05:05",
    "date":         "12 Aug 2025",
    "duration":     "22h 05m",
    "stops":        1
  },
  "returnLeg": {
    "airline":      "Qantas",
    "flightNumber": "QF2",
    "fromCode":     "LHR",
    "fromCity":     "London Heathrow",
    "toCode":       "SYD",
    "toCity":       "Sydney",
    "departTime":   "11:00",
    "arriveTime":   "06:00",
    "date":         "26 Aug 2025",
    "duration":     "21h 00m",
    "stops":        1
  },
  "summary": "One-stop via Singapore on both legs. Lie-flat seats included in Business."
}}

── CARD 17 — RESTAURANT ────────────────────────────────────────
{"card":"restaurant","data":{
  "name":        "Rockpool Bar & Grill",
  "cuisine":     "Modern Australian",
  "priceRange":  "$$$",
  "rating":      4.6,
  "reviewCount": 2840,
  "address":     "66 Hunter St, Sydney NSW 2000",
  "phone":       "(02) 8078 1900",
  "hours":       "Mon-Fri 12pm-11pm, Sat 5pm-11pm",
  "isOpen":      true,
  "topDishes":   ["Wagyu sirloin", "Lobster bisque", "Kingfish crudo", "Sticky date pudding"],
  "features":    ["Outdoor seating", "Private dining", "Full bar", "Valet parking"],
  "summary":     "One of Sydney's premier steakhouses. Bookings strongly recommended on weekends."
}}

── CARD 18 — RENTAL CAR ────────────────────────────────────────
{"card":"rental_car","data":{
  "company":          "Hertz",
  "carName":          "Toyota RAV4 or similar",
  "carClass":         "SUV",
  "transmission":     "Automatic",
  "seats":            5,
  "pickupLocation":   "Sydney Airport T1",
  "pickupDate":       "12 Aug 2025, 08:00",
  "dropoffLocation":  "Sydney Airport T1",
  "dropoffDate":      "19 Aug 2025, 08:00",
  "days":             7,
  "pricePerDay":      89,
  "totalPrice":       623,
  "currency":         "AUD",
  "inclusions":       ["Unlimited kilometres", "CDW insurance", "Airport surcharge included", "GPS included"],
  "summary":          "Free cancellation up to 48 hours before pickup."
}}

── CARD 19 — WEATHER ───────────────────────────────────────────
{"card":"weather","data":{
  "location":     "Sydney, NSW",
  "condition":    "Partly Cloudy",
  "tempC":        22,
  "feelsLikeC":   20,
  "humidity":     68,
  "windKph":      18,
  "windDir":      "NE",
  "uvIndex":      5,
  "visibility":   "10 km",
  "useFahrenheit":false,
  "forecast": [
    {"day":"Today", "condition":"Partly Cloudy","highC":23,"lowC":16,"precipPct":10},
    {"day":"Tue",   "condition":"Sunny",         "highC":26,"lowC":17,"precipPct":0},
    {"day":"Wed",   "condition":"Rain",           "highC":19,"lowC":14,"precipPct":80},
    {"day":"Thu",   "condition":"Cloudy",         "highC":20,"lowC":15,"precipPct":30},
    {"day":"Fri",   "condition":"Sunny",          "highC":24,"lowC":16,"precipPct":5}
  ],
  "summary": "Comfortable conditions today with a cold front bringing rain Wednesday."
}}

── CARD 20 — HOTEL ─────────────────────────────────────────────
{"card":"hotel","data":{
  "name":          "Park Hyatt Sydney",
  "stars":         5,
  "rating":        9.2,
  "reviewCount":   3410,
  "address":       "7 Hickson Rd, The Rocks NSW 2000",
  "checkIn":       "12 Aug 2025 — from 3:00 PM",
  "checkOut":      "19 Aug 2025 — by 11:00 AM",
  "nights":        7,
  "roomType":      "Harbour View Deluxe King",
  "pricePerNight": 680,
  "totalPrice":    4760,
  "currency":      "AUD",
  "amenities":     ["Rooftop pool", "Spa", "Valet parking", "Room service 24hr", "Opera House views", "Gym"],
  "summary":       "Direct Opera House views from this room category. Breakfast not included."
}}

── CARD 21 — DIRECTIONS ────────────────────────────────────────
{"card":"directions","data":{
  "origin":        "Sydney Airport T1",
  "destination":   "Park Hyatt Sydney",
  "mode":          "driving",
  "totalDistance": "14.3 km",
  "totalDuration": "28 min",
  "steps": [
    {"instruction":"Head north on Airport Dr toward General Holmes Dr",    "distance":"600 m",  "duration":"2 min"},
    {"instruction":"Merge onto M1 Eastern Distributor toward City",        "distance":"9.2 km", "duration":"12 min"},
    {"instruction":"Take exit 1 toward The Rocks / Circular Quay",        "distance":"1.1 km", "duration":"3 min"},
    {"instruction":"Turn left onto Hickson Rd",                            "distance":"400 m",  "duration":"1 min"},
    {"instruction":"Arrive at Park Hyatt Sydney on the right",             "distance":null,     "duration":null}
  ],
  "summary": "Toll road — approx AUD $8.00 via Eastern Distributor. No tolls via Anzac Pde (adds 12 min)."
}}

── CARD 22 — CURRENCY ──────────────────────────────────────────
{"card":"currency","data":{
  "fromCode":   "USD",
  "fromName":   "US Dollar",
  "toCode":     "AUD",
  "toName":     "Australian Dollar",
  "rate":       1.5342,
  "change":     0.0041,
  "changePct":  0.27,
  "asOf":       "4 Mar 2026, 14:30 AEDT",
  "conversions": [
    {"amount":1,    "converted":1.53},
    {"amount":10,   "converted":15.34},
    {"amount":50,   "converted":76.71},
    {"amount":100,  "converted":153.42},
    {"amount":500,  "converted":767.10},
    {"amount":1000, "converted":1534.20}
  ],
  "summary": "AUD has strengthened 0.27% over the past 24 hours against the USD."
}}

────────────────────────────────────────────────────────────────
  4.  SYSTEM PROMPT FRAGMENT FOR THE BACKEND LLM
      Add this to your agent's system prompt so it knows when
      and how to emit card envelopes.
────────────────────────────────────────────────────────────────

When your response contains structured financial data, ALWAYS
begin your reply with a JSON card envelope on a single line,
followed by any prose explanation on the next line.

Available card types and when to use them:

  stock            — Any live or delayed stock quote request
  backtest         — After running a backtest via run_backtest tool
  afl              — Any generated AFL code block
  portfolio        — Portfolio summary or holdings query
  screener         — Multi-row instrument list or scan results
  news             — News headlines for a ticker or topic
  watchlist        — Quick price check on multiple tickers at once
  economic_calendar— Economic events or calendar queries
  sectors          — Sector performance or rotation queries
  trade_signal     — BUY/SELL/HOLD signal with levels
  comparison       — Side-by-side metric table for 2+ instruments
  file_analysis    — After processing an uploaded document
  knowledge_base   — When citing the internal knowledge base
  error            — Any error, warning, or informational notice
  task_progress    — During multi-step async operations
  flight           — Flight search results or itinerary lookup
  restaurant       — Restaurant recommendation or detail query
  rental_car       — Car hire search or booking confirmation
  weather          — Current conditions or forecast for any location
  hotel            — Hotel search results or accommodation booking
  directions       — Route or navigation request between two points
  currency         — Exchange rate or currency conversion query

Rules:
  - Only emit one card per response
  - The JSON must be valid and start with {"card":"
  - Null out any fields you do not have data for
  - Do NOT wrap the JSON in markdown code fences
  - Write the prose summary as plain text after the JSON line

────────────────────────────────────────────────────────────────
  5.  ADDING A NEW CARD TYPE (checklist)
────────────────────────────────────────────────────────────────

  [ ]  Add a payload record class (sealed record MyPayload)
  [ ]  Add the case to the switch in TryBuild()
  [ ]  Implement BuildMyCard(MyPayload? p) following the
       MakeCard / TwoColumnGrid / MetricsGrid pattern
  [ ]  Add the card type name and trigger conditions to the
       backend system prompt fragment above
  [ ]  Add a payload example to section 3 of this guide
