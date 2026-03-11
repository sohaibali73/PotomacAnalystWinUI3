using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
// using PotomacAnalyst.Controls;  // Attachments disabled
using PotomacAnalyst.GenUi;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class ChatPage : Page
{
    // ── Services ────────────────────────────────────────────────────────────
    private readonly ApiService _api;

    // ── Conversation state ──────────────────────────────────────────────────
    private string? _currentConversationId;
    private CancellationTokenSource? _streamCts;
    private bool _isStreaming;

    // ── Streaming render state ──────────────────────────────────────────────
    // _rawBuffer is the only field touched by background threads — everything
    // else is UI-thread-only (written inside DispatcherQueue.TryEnqueue or
    // in the async continuation which runs on the UI thread).
    private readonly StringBuilder _rawBuffer = new();
    private TextBlock? _streamLabel;    // live text display
    private StackPanel? _dotsRow;        // 3-dot indicator
    // FIX: was DateTime — not thread-safe for unsynchronised cross-thread reads/writes.
    // CRITICAL FIX: C# forbids volatile long (only ≤32-bit types allowed).
    // Use a plain long + Interlocked.Read / Interlocked.Exchange for fully atomic,
    // cache-coherent access across the background streaming thread and the UI thread.
    private long _lastRenderTickMs = 0L;
    private int _receivedFirst = 0; // Interlocked: 0=false 1=true

    // ── Sidebar ─────────────────────────────────────────────────────────────
    private readonly List<Button> _allConvoButtons = new();
    private readonly HashSet<string> _knownConvoIds = new();
    private Button? _activeConvoButton;

    // ── File attachment (disabled — Attachments.cs removed) ──────────────────
    // private readonly List<AttachmentData> _pendingAttachments = new();
    private readonly Dictionary<string, StorageFile> _pendingFileMap = new();

    // ── Glyph constants (Segoe Fluent Icons — WinUI 3 standard) ─────────────
    private const string GlyphSend = "\uE725";
    private const string GlyphStop = "\uE71A";

    // ════════════════════════════════════════════════════════════════════════
    //  INIT
    // ════════════════════════════════════════════════════════════════════════

    public ChatPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();

        // Attachment strip disabled — Attachments.cs removed

        Loaded += async (_, _) => await LoadConversationsAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  SIDEBAR
    // ════════════════════════════════════════════════════════════════════════

    private async Task LoadConversationsAsync()
    {
        try
        {
            var list = await _api.GetAsync<List<ApiConversation>>("/chat/conversations");
            if (list is null || list.Count == 0) return;
            ConversationList.Children.Clear();
            _allConvoButtons.Clear();
            _knownConvoIds.Clear();
            foreach (var c in list)
            {
                var btn = BuildConvoButton(c);
                ConversationList.Children.Add(btn);
                _allConvoButtons.Add(btn);
                _knownConvoIds.Add(c.Id);
            }
        }
        catch { }
    }

    private Button BuildConvoButton(ApiConversation c)
    {
        var title = CleanTitle(c.Title);
        var btn = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(12, 9, 12, 9),
            Tag = c.Id,
        };
        btn.Resources["ButtonBackgroundPointerOver"] =
            new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x1A, 0x1A, 0x1A));
        btn.Resources["ButtonBackgroundPressed"] =
            new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x20, 0x20, 0x20));
        btn.Resources["ButtonBorderBrush"] = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        btn.Resources["ButtonBorderBrushPointerOver"] = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        btn.Resources["ButtonBorderBrushPressed"] = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

        var sp = new StackPanel { Spacing = 2 };
        sp.Children.Add(new TextBlock
        {
            Text = title,
            FontFamily = (FontFamily)Application.Current.Resources["QuicksandMedium"],
            FontSize = 13,
            Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });
        sp.Children.Add(new TextBlock
        {
            Text = $"{c.MessageCount} messages",
            FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
            FontSize = 11,
            Foreground = (SolidColorBrush)Application.Current.Resources["TextMutedBrush"],
        });
        btn.Content = sp;
        btn.Click += async (s, _) =>
        {
            if (s is Button b && b.Tag is string id)
            {
                SetActiveSidebarButton(b);
                await LoadConversationAsync(id, title);
            }
        };
        return btn;
    }

    private void SetActiveSidebarButton(Button? btn)
    {
        if (_activeConvoButton is not null)
            _activeConvoButton.Background =
                new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        _activeConvoButton = btn;
        if (btn is not null)
            btn.Background =
                new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x1E, 0x1E, 0x1E));
    }

    private void PrependConvoToSidebar(string id, string title)
    {
        if (_knownConvoIds.Contains(id)) return;
        _knownConvoIds.Add(id);
        var btn = BuildConvoButton(new ApiConversation { Id = id, Title = title, MessageCount = 1 });
        ConversationList.Children.Insert(0, btn);
        _allConvoButtons.Insert(0, btn);
        SetActiveSidebarButton(btn);
    }

    private void UpdateSidebarMsgCount(string id, int count)
    {
        var btn = _allConvoButtons.FirstOrDefault(b => b.Tag is string s && s == id);
        if (btn?.Content is StackPanel sp &&
            sp.Children.OfType<TextBlock>().Skip(1).FirstOrDefault() is TextBlock sub)
            sub.Text = $"{count} messages";
    }

    private async Task LoadConversationAsync(string id, string title)
    {
        _currentConversationId = id;
        ChatTitle.Text = CleanTitle(title).ToUpperInvariant();
        ChatSubtitle.Text = "Loading…";
        WelcomePanel.Visibility = Visibility.Collapsed;
        RenameBtn.Visibility = Visibility.Visible;
        MessageList.Children.Clear();
        try
        {
            var msgs = await _api.GetAsync<List<ApiChatMessage>>(
                $"/chat/conversations/{id}/messages");
            if (msgs is null) return;
            foreach (var m in msgs)
            {
                if (m.Role == "user")
                    await AppendBubble(m.Content, isUser: true, animate: false);
                else
                    AppendRichBubble(Clean(m.Content), m.Content, animate: false);
            }
            ChatSubtitle.Text = $"{msgs.Count} messages";
            ScrollToBottom();
        }
        catch (Exception ex) { ChatSubtitle.Text = $"Error: {ex.Message}"; }
    }

    private void SearchBox_TextChanged(object s, TextChangedEventArgs e)
    {
        var q = SearchBox.Text.Trim().ToLowerInvariant();
        foreach (var btn in _allConvoButtons)
        {
            if (btn.Content is not StackPanel sp) continue;
            var text = sp.Children.OfType<TextBlock>().FirstOrDefault()?.Text?.ToLowerInvariant() ?? "";
            btn.Visibility = q.Length == 0 || text.Contains(q)
                ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  NEW / CLEAR / RENAME
    // ════════════════════════════════════════════════════════════════════════

    private void NewChat_Click(object s, RoutedEventArgs e)
    {
        _currentConversationId = null;
        MessageList.Children.Clear();
        MessageList.Children.Add(WelcomePanel);
        WelcomePanel.Visibility = Visibility.Visible;
        RenameBtn.Visibility = Visibility.Collapsed;
        ChatTitle.Text = "NEW CONVERSATION";
        ChatSubtitle.Text = "Start a conversation with your AI analyst";
        SetActiveSidebarButton(null);
        ClearAttachment();
        MessageInput.Focus(FocusState.Programmatic);
    }

    private void ClearChat_Click(object s, RoutedEventArgs e) => NewChat_Click(s, e);

    private async void RenameChat_Click(object s, RoutedEventArgs e)
    {
        if (_currentConversationId is null) return;
        var input = new TextBox
        {
            Text = ChatTitle.Text,
            FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
            FontSize = 14,
            // FIX Bug 6: SelectionStart / SelectionLength set in an object initialiser
            // are applied before the TextBox enters the live visual tree and are silently
            // discarded by the framework when the control is first laid out.
            // Use the Loaded event instead, which fires after the element is in the tree.
        };
        input.Loaded += (_, _) =>
        {
            input.SelectionStart = 0;
            input.SelectionLength = input.Text.Length;
        };
        var dlg = new ContentDialog
        {
            Title = "Rename Conversation",
            Content = input,
            PrimaryButtonText = "Rename",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot,
            DefaultButton = ContentDialogButton.Primary,
        };
        if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;
        var newTitle = input.Text.Trim();
        if (string.IsNullOrEmpty(newTitle)) return;
        try
        {
            await _api.PatchAsync<object, object>(
                $"/chat/conversations/{_currentConversationId}", new { title = newTitle });
            ChatTitle.Text = newTitle.ToUpperInvariant();
            if (_activeConvoButton?.Content is StackPanel sp &&
                sp.Children.OfType<TextBlock>().FirstOrDefault() is TextBlock tb)
                tb.Text = CleanTitle(newTitle);
        }
        catch { }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PROMPT CHIPS
    // ════════════════════════════════════════════════════════════════════════

    private async void PromptChip_Click(object s, RoutedEventArgs e)
    {
        if (s is Button btn && btn.Tag is string prompt)
        {
            MessageInput.Text = prompt;
            await SendAsync();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  FILE ATTACHMENT
    // ════════════════════════════════════════════════════════════════════════

    private void AttachFile_Click(object s, RoutedEventArgs e) { /* Attachments disabled */ }
    private void RemoveAttachment_Click(object s, RoutedEventArgs e) { }

    private void ClearAttachment()
    {
        _pendingFileMap.Clear();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  SEND / STOP
    // ════════════════════════════════════════════════════════════════════════

    private async void SendMessage_Click(object s, RoutedEventArgs e)
    {
        if (_isStreaming) { _streamCts?.Cancel(); return; }
        await SendAsync();
    }

    private async void MessageInput_KeyDown(object s, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter) return;
        var shift = InputKeyboardSource
            .GetKeyStateForCurrentThread(VirtualKey.Shift)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        if (shift) return;
        e.Handled = true;
        await SendAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  STREAM PIPELINE
    // ════════════════════════════════════════════════════════════════════════

    private async Task SendAsync()
    {
        if (_isStreaming) return;
        var text = MessageInput.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        MessageInput.Text = string.Empty;
        // FIX Bug 4: Collapsing WelcomePanel in-place leaves it as a child of
        // MessageList, so Children.Count is always off-by-one for the subtitle
        // and for any code that enumerates the list.  Remove it from the tree
        // instead; NewChat_Click re-adds it when needed.
        if (MessageList.Children.Contains(WelcomePanel))
            MessageList.Children.Remove(WelcomePanel);
        _isStreaming = true;
        SendIcon.Glyph = GlyphStop;
        SendIcon.FontSize = 14;

        // ── File uploads (disabled — Attachments.cs removed) ─────────────

        // ── User bubble ──────────────────────────────────────────────────
        await AppendBubble(text, isUser: true, animate: true);

        // ── AI bubble — build all refs up-front with stable local vars ───
        // NEVER use Children[index] to recover these refs later.
        // Store them as local variables AND as instance fields (for callbacks).
        var streamLabel = new TextBlock
        {
            FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22,
            Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
            Visibility = Visibility.Collapsed,
        };
        var dotsRow = BuildDotsRow();

        var wrapper = new StackPanel { Spacing = 0 };
        wrapper.Children.Add(dotsRow);
        wrapper.Children.Add(streamLabel);

        var aiBubble = MakeBubble(isUser: false);
        aiBubble.Child = wrapper;

        // Instance fields for background-thread closures
        _streamLabel = streamLabel;
        _dotsRow = dotsRow;

        MessageList.Children.Add(aiBubble);
        SlideIn(aiBubble, fromRight: false);
        ScrollToBottom();

        // Start animation AFTER element is in the visual tree
        StartDotBounce(dotsRow);

        // ── Reset stream state ───────────────────────────────────────────
        // FIX Bug 7: _rawBuffer.Clear() must be under the same lock used by
        // background onText so no stale append from a cancelled stream slips
        // in after we clear but before streaming begins.
        lock (_rawBuffer) { _rawBuffer.Clear(); }
        System.Threading.Interlocked.Exchange(ref _receivedFirst, 0);
        System.Threading.Interlocked.Exchange(ref _lastRenderTickMs, 0L); // atomic reset visible to background thread
        // FIX Bug 2: Always dispose the previous CTS before allocating a new one,
        // otherwise every send leaks a WaitHandle and thread-pool registration.
        _streamCts?.Dispose();
        _streamCts = new CancellationTokenSource();

        var req = new SendMessageRequest
        {
            Content = text,
            ConversationId = _currentConversationId,
        };

        string finalRaw = string.Empty;

        try
        {
            await _api.StreamAsync("/chat/stream", req,

                onText: chunk =>
                {
                    // ── Runs on background thread ──────────────────────
                    lock (_rawBuffer) { _rawBuffer.Append(chunk); }

                    // Interlocked CAS: atomically flip 0→1, returns old value
                    bool isFirst = System.Threading.Interlocked.CompareExchange(
                        ref _receivedFirst, 1, 0) == 0;

                    // 60fps throttle (skip redundant dispatches).
                    // FIX: was DateTime which is not thread-safe for unsynchronised reads/writes.
                    // Environment.TickCount64 is a long; volatile long read/write is atomic on
                    // all .NET-supported architectures.
                    var nowMs = Environment.TickCount64;
                    if (!isFirst && nowMs - _lastRenderTickMs < 16L)
                        return;
                    _lastRenderTickMs = nowMs;

                    // Capture stable local refs — never access _streamLabel/_dotsRow
                    // directly from background thread without a local capture
                    var lbl = _streamLabel;
                    var dots = _dotsRow;
                    if (lbl is null) return;

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (isFirst)
                        {
                            if (dots is not null) dots.Visibility = Visibility.Collapsed;
                            lbl.Visibility = Visibility.Visible;
                        }
                        string snapshot;
                        lock (_rawBuffer) { snapshot = _rawBuffer.ToString(); }
                        lbl.Text = CleanForStream(snapshot) + "▍";
                        lbl.Foreground =
                            (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"];
                        ChatScrollViewer.ChangeView(null, double.MaxValue, null,
                            disableAnimation: true);
                    });
                },

                onToolCall: toolName =>
                {
                    var lbl = _streamLabel;
                    var dots = _dotsRow;
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (dots is not null) dots.Visibility = Visibility.Collapsed;
                        if (lbl is null) return;
                        lbl.Text = FormatToolBadge(toolName);
                        lbl.Foreground =
                            (SolidColorBrush)Application.Current.Resources["TextMutedBrush"];
                        lbl.Visibility = Visibility.Visible;
                    });
                },

                onConversationId: id =>
                {
                    // FIX Bug 3: _currentConversationId is a UI-thread field.
                    // This callback fires on the background streaming thread; writing
                    // the field there without synchronisation is a data race.
                    // Everything here (including the assignment) runs on the UI thread.
                    var label = text.Length > 40 ? text[..40] + "…" : text;
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        _currentConversationId = id;
                        PrependConvoToSidebar(id, label);
                    });
                },

                onError: err =>
                {
                    var lbl = _streamLabel;
                    var dots = _dotsRow;
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (dots is not null) dots.Visibility = Visibility.Collapsed;
                        if (lbl is null || _rawBuffer.Length > 0) return;
                        lbl.Text = $"Error: {err}";
                        lbl.Foreground =
                            (SolidColorBrush)Application.Current.Resources["PotomacPinkBrush"];
                        lbl.Visibility = Visibility.Visible;
                    });
                },

                ct: _streamCts.Token
            );

            lock (_rawBuffer) { finalRaw = _rawBuffer.ToString(); }

            // Non-streaming fallback
            if (string.IsNullOrEmpty(finalRaw) &&
                _receivedFirst == 0 &&
                !(_streamCts?.IsCancellationRequested ?? false))
            {
                try
                {
                    var r = await _api.PostAsync<SendMessageRequest, StreamResponse>(
                        "/chat/message", req);
                    if (r is not null)
                    {
                        finalRaw = r.Response ?? string.Empty;
                        if (!string.IsNullOrEmpty(r.ConversationId))
                        {
                            _currentConversationId = r.ConversationId;
                            var label = text.Length > 40 ? text[..40] + "…" : text;
                            PrependConvoToSidebar(r.ConversationId, label);
                        }
                    }
                }
                catch { }
            }
        }
        catch (OperationCanceledException)
        {
            lock (_rawBuffer) { finalRaw = _rawBuffer.ToString(); }
        }
        catch (Exception ex)
        {
            lock (_rawBuffer) { finalRaw = _rawBuffer.ToString(); }
            if (string.IsNullOrEmpty(finalRaw))
                finalRaw = $"Connection error: {ex.Message}";
        }
        finally
        {
            // ── Always on UI thread (SendAsync was awaited from UI) ──────
            _isStreaming = false;
            SendIcon.Glyph = GlyphSend;
            SendIcon.FontSize = 16;

            // Finalize with stable local refs (not the now-cleared instance fields)
            FinalizeAiBubble(aiBubble, streamLabel, dotsRow, finalRaw);

            if (_currentConversationId is not null)
            {
                var cnt = MessageList.Children.Count;
                ChatSubtitle.Text = $"{cnt} messages";
                UpdateSidebarMsgCount(_currentConversationId, cnt);
            }

            // Clear instance refs — future TryEnqueue callbacks see null and bail
            _streamLabel = null;
            _dotsRow = null;

            ScrollToBottom();
            MessageInput.Focus(FocusState.Programmatic);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  FINALIZE — replace streaming placeholder with real content
    // ════════════════════════════════════════════════════════════════════════

    private void FinalizeAiBubble(
        Border bubble, TextBlock streamLabel, StackPanel dotsRow, string rawText)
    {
        // Always kill the dots
        dotsRow.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(rawText))
        {
            streamLabel.Text = "No response received.";
            streamLabel.Foreground =
                (SolidColorBrush)Application.Current.Resources["TextMutedBrush"];
            streamLabel.Visibility = Visibility.Visible;
            return;
        }

        // ── 1. Try JSON card (with fence stripping) ──────────────────────
        var card = TryBuildCard(rawText);
        if (card is not null)
        {
            // Remove bubble styling — card has its own chrome
            bubble.Padding = new Thickness(0);
            bubble.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            bubble.BorderThickness = new Thickness(0);
            bubble.CornerRadius = new CornerRadius(0);
            bubble.Child = card;
            return;
        }

        // ── 2. Fenced code blocks ────────────────────────────────────────
        if (rawText.Contains("```"))
        {
            bubble.Child = RenderRich(rawText);
            return;
        }

        // ── 3. Plain / markdown text ─────────────────────────────────────
        var rendered = RenderMarkdown(Clean(rawText));
        if (rendered is TextBlock singleTb)
        {
            // Simple path: put the text block directly in the label slot
            streamLabel.Text = singleTb.Text;
            streamLabel.Visibility = Visibility.Visible;
        }
        else
        {
            bubble.Child = rendered;
        }
    }

    // Central card-build entry: strips JSON fences, tries heuristic, calls TryBuild
    private UIElement? TryBuildCard(string rawText)
    {
        Action<string> nav = page => GetMainWindow()?.NavigateTo(page);

        // a) strip ```json fence and try
        var stripped = StripJsonFence(rawText);
        var card = GenUiCardBuilder.TryBuild(stripped, nav);
        if (card is not null) return card;

        // b) plain-text stock heuristic
        var stockJson = TryConvertPlainTextToCardEnvelope(rawText);
        if (stockJson is not null)
        {
            card = GenUiCardBuilder.TryBuild(stockJson, nav);
            if (card is not null) return card;
        }

        return null;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  MARKDOWN RENDERER
    //  Handles: # headings, - / * / · bullets, 1. numbered, **bold**,
    //           *italic*, `inline code` — all real WinUI 3 Inlines
    // ════════════════════════════════════════════════════════════════════════

    private UIElement RenderMarkdown(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return MakePlainLabel(text);

        var lines = text.Split('\n');
        var panel = new StackPanel { Spacing = 4 };
        bool hasAny = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                if (hasAny) panel.Children.Add(new Border { Height = 5 });
                continue;
            }
            hasAny = true;

            // Heading (h1–h5)
            var hm = Regex.Match(line, @"^(#{1,5})\s+(.+)$");
            if (hm.Success)
            {
                int lvl = hm.Groups[1].Length;
                panel.Children.Add(new TextBlock
                {
                    Text = hm.Groups[2].Value,
                    FontFamily = (FontFamily)Application.Current.Resources["RajdhaniBold"],
                    FontSize = lvl switch { 1 => 17, 2 => 15, 3 => 13, 4 => 12, _ => 11 },
                    FontWeight = FontWeights.Bold,
                    CharacterSpacing = 40,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = lvl <= 2
                        ? (SolidColorBrush)Application.Current.Resources["PotomacYellowBrush"]
                        : (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
                    Margin = new Thickness(0, lvl == 1 ? 8 : 4, 0, 2),
                });
                continue;
            }

            // Bullet
            var bm = Regex.Match(line, @"^(\s*)[•·\-\*]\s+(.+)$");
            if (bm.Success)
            {
                var indent = bm.Groups[1].Length * 8;
                var row = new Grid { Margin = new Thickness(indent, 1, 0, 1) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var dot = new TextBlock
                {
                    Text = "·",
                    FontSize = 18,
                    Foreground = (SolidColorBrush)Application.Current.Resources["PotomacYellowBrush"],
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 1, 0, 0),
                };
                Grid.SetColumn(dot, 0);
                row.Children.Add(dot);
                var content = BuildInlineRuns(bm.Groups[2].Value);
                Grid.SetColumn(content, 1);
                row.Children.Add(content);
                panel.Children.Add(row);
                continue;
            }

            // Numbered list
            var nm = Regex.Match(line, @"^(\d+)\.\s+(.+)$");
            if (nm.Success)
            {
                var row = new Grid { Margin = new Thickness(0, 1, 0, 1) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var num = new TextBlock
                {
                    Text = nm.Groups[1].Value + ".",
                    FontFamily = (FontFamily)Application.Current.Resources["QuicksandMedium"],
                    FontSize = 14,
                    Foreground = (SolidColorBrush)Application.Current.Resources["PotomacYellowBrush"],
                    VerticalAlignment = VerticalAlignment.Top,
                };
                Grid.SetColumn(num, 0);
                row.Children.Add(num);
                var content = BuildInlineRuns(nm.Groups[2].Value);
                Grid.SetColumn(content, 1);
                row.Children.Add(content);
                panel.Children.Add(row);
                continue;
            }

            // Regular line with inline markdown
            panel.Children.Add(BuildInlineRuns(line));
        }

        if (panel.Children.Count == 0) return MakePlainLabel(text);
        if (panel.Children.Count == 1 && panel.Children[0] is TextBlock solo) return solo;
        return panel;
    }

    // Renders inline **bold**, *italic*, `code` using WinUI 3 Inlines
    private TextBlock BuildInlineRuns(string line)
    {
        var tb = new TextBlock
        {
            FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22,
            Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
        };

        var rx = new Regex(@"\*\*(.+?)\*\*|\*(.+?)\*|`([^`]+)`|~~(.+?)~~");
        int last = 0;

        foreach (Match m in rx.Matches(line))
        {
            if (m.Index > last)
                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = line[last..m.Index] });

            if (m.Groups[1].Success)
                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                {
                    Text = m.Groups[1].Value,
                    FontWeight = FontWeights.Bold,
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
                });
            else if (m.Groups[2].Success)
                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                { Text = m.Groups[2].Value, FontStyle = Windows.UI.Text.FontStyle.Italic });
            else if (m.Groups[3].Success)
                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                {
                    Text = m.Groups[3].Value,
                    FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xCE, 0xD4, 0xE0)),
                });
            else if (m.Groups[4].Success)
                tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run
                {
                    Text = m.Groups[4].Value,
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextSecondaryBrush"],
                    TextDecorations = Windows.UI.Text.TextDecorations.Strikethrough,
                });

            last = m.Index + m.Length;
        }

        if (last < line.Length)
            tb.Inlines.Add(new Microsoft.UI.Xaml.Documents.Run { Text = line[last..] });

        // No markdown found — use plain Text property (faster)
        if (tb.Inlines.Count == 0)
            tb.Text = line;

        return tb;
    }

    private TextBlock MakePlainLabel(string text) => new()
    {
        Text = text,
        FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
        FontSize = 14,
        TextWrapping = TextWrapping.Wrap,
        LineHeight = 22,
        Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
    };

    // ════════════════════════════════════════════════════════════════════════
    //  RICH RENDERER — text + fenced code blocks
    // ════════════════════════════════════════════════════════════════════════

    private UIElement RenderRich(string raw)
    {
        var parts = SplitCodeBlocks(raw);
        if (parts.Count == 1 && !parts[0].IsCode)
            return RenderMarkdown(Clean(raw));

        var panel = new StackPanel { Spacing = 10 };
        foreach (var part in parts)
        {
            if (part.IsCode)
                panel.Children.Add(CreateCodeBlock(part.Language, part.Content));
            else
            {
                var cleaned = Clean(part.Content).Trim();
                if (!string.IsNullOrEmpty(cleaned))
                    panel.Children.Add(RenderMarkdown(cleaned));
            }
        }
        return panel;
    }

    private sealed record CodePart(bool IsCode, string Language, string Content);

    private static List<CodePart> SplitCodeBlocks(string text)
    {
        var parts = new List<CodePart>();
        var pattern = new Regex(@"```(\w*)\r?\n?([\s\S]*?)```", RegexOptions.Compiled);
        int lastEnd = 0;
        foreach (Match m in pattern.Matches(text))
        {
            if (m.Index > lastEnd) parts.Add(new CodePart(false, "", text[lastEnd..m.Index]));
            parts.Add(new CodePart(true, m.Groups[1].Value, m.Groups[2].Value.TrimEnd()));
            lastEnd = m.Index + m.Length;
        }
        if (lastEnd < text.Length) parts.Add(new CodePart(false, "", text[lastEnd..]));
        return parts.Count > 0 ? parts : [new(false, "", text)];
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CODE BLOCK WIDGET
    // ════════════════════════════════════════════════════════════════════════

    private Border CreateCodeBlock(string language, string code)
    {
        code = code.Trim();
        var lang = (language ?? "").ToLower();
        var isAfl = lang is "afl" or "amibroker" or "amibrokerfml";
        var label = string.IsNullOrEmpty(language) ? "CODE" : language.ToUpper();

        var outer = new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x0D, 0x0D, 0x12)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x2A, 0x2A, 0x3A)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 4, 0, 4),
        };
        var stack = new StackPanel();
        var hdr = new Grid
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x14, 0x14, 0x1E)),
            Padding = new Thickness(14, 7, 10, 7),
        };
        hdr.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        hdr.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var langLbl = new TextBlock
        {
            Text = label,
            FontFamily = (FontFamily)Application.Current.Resources["RajdhaniBold"],
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            CharacterSpacing = 40,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = isAfl
                ? (SolidColorBrush)Application.Current.Resources["PotomacYellowBrush"]
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x88, 0x88, 0xBB)),
        };
        Grid.SetColumn(langLbl, 0);

        var btns = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        if (isAfl)
        {
            var aflBtn = MakeHeaderButton("AFL GENERATOR",
                (SolidColorBrush)Application.Current.Resources["PotomacYellowBrush"],
                Windows.UI.Color.FromArgb(255, 0x1A, 0x14, 0x00));
            aflBtn.Click += (_, _) => GetMainWindow()?.NavigateTo("AflGenerator");
            btns.Children.Add(aflBtn);
        }
        var copyBtn = MakeHeaderButton("COPY",
            new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x66, 0x66, 0x88)),
            Windows.UI.Color.FromArgb(255, 0x1E, 0x1E, 0x2E));
        var codeCopy = code;
        copyBtn.Click += (_, _) =>
        {
            var pkg = new DataPackage(); pkg.SetText(codeCopy); Clipboard.SetContent(pkg);
            copyBtn.Content = "COPIED";
            _ = Task.Delay(1200).ContinueWith(_ =>
                DispatcherQueue.TryEnqueue(() => copyBtn.Content = "COPY"));
        };
        btns.Children.Add(copyBtn);
        Grid.SetColumn(btns, 1);
        hdr.Children.Add(langLbl); hdr.Children.Add(btns);

        var cb = new TextBlock
        {
            Text = code,
            FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
            FontSize = 12,
            LineHeight = 20,
            TextWrapping = TextWrapping.NoWrap,
            Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0xCE, 0xD4, 0xE0)),
            Padding = new Thickness(14, 12, 14, 14),
        };
        stack.Children.Add(hdr);
        stack.Children.Add(new ScrollViewer
        {
            Content = cb,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = 420,
        });
        outer.Child = stack;
        return outer;
    }

    private static Button MakeHeaderButton(string text, SolidColorBrush fg, Windows.UI.Color bg)
    {
        var btn = new Button
        {
            Content = text,
            FontFamily = new FontFamily("Segoe UI Variable, Segoe UI"),
            FontSize = 10,
            Padding = new Thickness(10, 4, 10, 4),
            CornerRadius = new CornerRadius(5),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = fg,
        };
        btn.Resources["ButtonBackground"] = new SolidColorBrush(bg);
        btn.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(
            Windows.UI.Color.FromArgb(255,
                (byte)Math.Min(bg.R + 20, 255),
                (byte)Math.Min(bg.G + 20, 255),
                (byte)Math.Min(bg.B + 20, 255)));
        btn.Resources["ButtonBackgroundPressed"] = new SolidColorBrush(bg);
        btn.Resources["ButtonBorderBrush"] = new SolidColorBrush(Windows.UI.Color.FromArgb(60, 255, 255, 255));
        btn.Resources["ButtonBorderBrushPointerOver"] = btn.Resources["ButtonBorderBrush"];
        btn.Resources["ButtonBorderBrushPressed"] = btn.Resources["ButtonBorderBrush"];
        btn.Resources["ButtonForeground"] = fg;
        btn.Resources["ButtonForegroundPointerOver"] = fg;
        btn.Resources["ButtonForegroundPressed"] = fg;
        return btn;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  BUBBLE HELPERS
    // ════════════════════════════════════════════════════════════════════════

    private async Task AppendBubble(string text, bool isUser, bool animate)
    {
        var bubble = MakeBubble(isUser);
        bubble.Child = BuildInlineRuns(text);
        MessageList.Children.Add(bubble);
        if (animate) { SlideIn(bubble, fromRight: isUser); await Task.Delay(30); }
        ScrollToBottom();
    }

    private void AppendRichBubble(string cleaned, string raw, bool animate = true)
    {
        var bubble = MakeBubble(isUser: false);
        var card = TryBuildCard(raw);
        if (card is not null)
        {
            bubble.Padding = new Thickness(0);
            bubble.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            bubble.BorderThickness = new Thickness(0);
            bubble.CornerRadius = new CornerRadius(0);
            bubble.Child = card;
        }
        else if (raw.Contains("```"))
            bubble.Child = RenderRich(raw);
        else
            bubble.Child = RenderMarkdown(cleaned);
        MessageList.Children.Add(bubble);
        // FIX Bug 5: was unconditionally SlideIn — replayed history animated in one
        // by one on every conversation load which was jarring and slow.
        if (animate) SlideIn(bubble, fromRight: false);
    }

    private static Border MakeBubble(bool isUser) => isUser
        ? new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x1E, 0x17, 0x01)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(160, 0xFE, 0xC0, 0x0F)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18, 18, 4, 18),
            Padding = new Thickness(18, 11, 18, 11),
            Margin = new Thickness(80, 5, 0, 5),
            HorizontalAlignment = HorizontalAlignment.Right,
            MaxWidth = 680,
        }
        : new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x28, 0x28, 0x34)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(55, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18, 18, 18, 4),
            Padding = new Thickness(18, 11, 18, 11),
            Margin = new Thickness(0, 5, 80, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            MaxWidth = 680,
        };

    // ════════════════════════════════════════════════════════════════════════
    //  DOTS ANIMATION
    //  Split into Build (no tree needed) + Start (tree required)
    // ════════════════════════════════════════════════════════════════════════

    private static StackPanel BuildDotsRow()
    {
        var row = new StackPanel
        { Orientation = Orientation.Horizontal, Spacing = 5, Padding = new Thickness(4, 6, 4, 6) };
        for (int i = 0; i < 3; i++)
            row.Children.Add(new Ellipse
            {
                Width = 7,
                Height = 7,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0xFE, 0xC0, 0x0F)),
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform = new CompositeTransform(),
            });
        return row;
    }

    // Must be called AFTER the StackPanel is in the live visual tree
    private static void StartDotBounce(StackPanel dotsRow)
    {
        var ease = new SineEase { EasingMode = EasingMode.EaseInOut };
        int i = 0;
        foreach (var dot in dotsRow.Children.OfType<Ellipse>())
        {
            var delay = TimeSpan.FromMilliseconds(i++ * 160);
            var sb = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = new Duration(TimeSpan.FromMilliseconds(960))
            };

            void AddAnim(double v0, double v1, double v2, string prop)
            {
                var a = new DoubleAnimationUsingKeyFrames { BeginTime = delay };
                a.KeyFrames.Add(new EasingDoubleKeyFrame
                { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = v0, EasingFunction = ease });
                a.KeyFrames.Add(new EasingDoubleKeyFrame
                { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)), Value = v1, EasingFunction = ease });
                a.KeyFrames.Add(new EasingDoubleKeyFrame
                { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400)), Value = v2, EasingFunction = ease });
                Storyboard.SetTarget(a, dot);
                Storyboard.SetTargetProperty(a, prop);
                sb.Children.Add(a);
            }

            AddAnim(0, -7, 0, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            AddAnim(1, 1.15, 1, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            AddAnim(1, 1.15, 1, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
            sb.Begin();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ANIMATIONS
    // ════════════════════════════════════════════════════════════════════════

    private static void SlideIn(UIElement el, bool fromRight)
    {
        el.RenderTransform = new CompositeTransform
        { TranslateX = fromRight ? 24 : -24, ScaleX = 0.97, ScaleY = 0.97 };
        el.RenderTransformOrigin = new Windows.Foundation.Point(fromRight ? 1 : 0, 0.5);
        el.Opacity = 0;

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var dur = TimeSpan.FromMilliseconds(260);
        var sb = new Storyboard();

        void A(double from, double to, string prop)
        {
            var a = new DoubleAnimation
            { From = from, To = to, Duration = dur, EasingFunction = ease };
            Storyboard.SetTarget(a, el);
            Storyboard.SetTargetProperty(a, prop);
            sb.Children.Add(a);
        }
        A(0, 1.0, "Opacity");
        A(fromRight ? 24 : -24, 0, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
        A(0.97, 1.0, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
        A(0.97, 1.0, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");
        sb.Begin();
    }

    private void ScrollToBottom() =>
        ChatScrollViewer.ChangeView(null, double.MaxValue, null, disableAnimation: true);

    // ════════════════════════════════════════════════════════════════════════
    //  TEXT / JSON HELPERS
    // ════════════════════════════════════════════════════════════════════════

    // Strip ```json ... ``` so TryBuild can parse the inner JSON envelope
    private static string StripJsonFence(string text)
    {
        var m = Regex.Match(text.Trim(), @"^```(?:json)?\s*([\s\S]*?)\s*```$",
            RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value.Trim() : text;
    }

    // Light clean during live stream — preserve markdown, just strip FORMATTING tags
    private static string CleanForStream(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = Regex.Replace(s, @"\[FORMATTING[^\]]*\]", "", RegexOptions.IgnoreCase);
        return s.TrimEnd('▍');
    }

    // Full clean for finalized display
    private static string Clean(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var nb = new StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            // Drop surrogate pairs (emoji encoded as UTF-16 surrogates)
            if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            {
                i++; // skip both the high and low surrogate — don't append either
                continue;
            }
            // Drop BMP emoji / dingbats range
            if (c >= 0x2600 && c <= 0x27BF) continue;
            nb.Append(c);
        }
        s = nb.ToString();
        s = Regex.Replace(s, @"\[FORMATTING[^\]]*\]", "", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, @"\n{3,}", "\n\n");
        return s.TrimEnd('▍').Trim();
    }

    private static string CleanTitle(string t)
    {
        if (string.IsNullOrEmpty(t)) return "New Conversation";
        t = Regex.Replace(t, @"\[FORMATTING[^\]]*\]", "", RegexOptions.IgnoreCase).Trim();
        t = Regex.Replace(t, @"^RE:\s*", "", RegexOptions.IgnoreCase).Trim();
        return string.IsNullOrWhiteSpace(t) ? "New Conversation" : t;
    }

    // Heuristic: detect stock-data prose and synthesise a card JSON envelope
    private static string? TryConvertPlainTextToCardEnvelope(string text)
    {
        var pm = Regex.Match(text, @"current\s+price[:\s]+\$([\d,]+\.?\d*)", RegexOptions.IgnoreCase);
        if (!pm.Success) return null;

        static string? Ex(string s, string p)
        {
            var m = Regex.Match(s, p, RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }
        static string Esc(string? v) => v is null ? "null"
            : "\"" + v.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        static string Num(string? v) => v is null ? "null" : v.Replace(",", "");

        var ticker = Ex(text, @"\(([A-Z]{1,5})\)") ?? Ex(text, @"\b([A-Z]{1,5})\b(?=.*current price)") ?? "STOCK";
        var company = Ex(text, @"([A-Z][A-Za-z\s&.,]+?)\s*[\(\-\n]") ?? ticker;
        var price = pm.Groups[1].Value;
        var change = Ex(text, @"change[:\s]+[+\-]?\$?([\d,]+\.?\d*)");
        var changePct = Ex(text, @"\(([+\-]?\d+\.?\d*)%\)");
        var open = Ex(text, @"open[:\s]+\$([\d,]+\.?\d*)");
        var prevClose = Ex(text, @"prev(?:ious)?\s+close[:\s]+\$([\d,]+\.?\d*)");
        var high = Ex(text, @"(?:day\s+)?high[:\s]+\$([\d,]+\.?\d*)");
        var low = Ex(text, @"(?:day\s+)?low[:\s]+\$([\d,]+\.?\d*)");
        var volume = Ex(text, @"volume[:\s]+([\d,.]+\s*[MBK]?)");
        var mktCap = Ex(text, @"market\s+cap[:\s]+\$([\d,.]+\s*[TBM]?)");
        var sm = Regex.Match(text, @"(?:technical|analysis|overview|summary)[:\s]*\n(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        bool neg = text.Contains("-$") || text.Contains("(-");
        return $"{{\"card\":\"stock\",\"data\":{{" +
               $"\"ticker\":{Esc(ticker)},\"company\":{Esc(company?.Trim())}," +
               $"\"price\":{Num(price)}," +
               $"\"change\":{(change is not null ? (neg ? "-" : "") + Num(change) : "null")}," +
               $"\"changePct\":{Num(changePct)}," +
               $"\"open\":{Num(open)},\"prevClose\":{Num(prevClose)}," +
               $"\"high\":{Num(high)},\"low\":{Num(low)}," +
               $"\"volume\":{Esc(volume)},\"marketCap\":{Esc(mktCap is not null ? "$" + mktCap : null)}," +
               $"\"summary\":{Esc(sm.Success ? sm.Groups[1].Value.Trim() : null)}" +
               $"}}}}";
    }

    private static string FormatToolBadge(string toolName) => toolName switch
    {
        "web_search" or "search" => "Searching the web…",
        "get_company_data" or "company_data" => "Looking up company data…",
        "get_market_data" or "market_data" => "Fetching market data…",
        "run_backtest" or "backtest" => "Running backtest…",
        "generate_afl" or "afl_generate" => "Generating AFL code…",
        "read_knowledge_base" or "search_brain" => "Searching knowledge base…",
        "analyze_file" or "file_analysis" => "Analyzing file…",
        _ => $"Using {toolName}…",
    };

    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}

// ── File upload response ─────────────────────────────────────────────────────
file sealed class FileUploadResult
{
    [System.Text.Json.Serialization.JsonPropertyName("file_id")]
    public string? FileId { get; init; }
}