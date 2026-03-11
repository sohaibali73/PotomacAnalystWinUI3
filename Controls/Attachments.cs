// =============================================================================
//  Attachments.cs  —  WinUI 3 native port of Vercel AI Elements / Attachments
//  Potomac Analyst Workbench
//
//  THREE VARIANTS:
//
//  Grid   — 84×84 square tiles in a wrap flow
//           Images: full-bleed photo + frosted filename strip at bottom
//           Other types: centered category icon + file extension tag
//           Hover: ✕ button fades in top-right; tile border brightens
//           Use for: rendered message history
//
//  Inline — horizontal scrolling pill strip, height 36
//           Each pill: [28px round thumb] [filename ~20 chars] [tiny ✕]
//           Image pills: PointerPressed opens flyout with 300×220 preview
//           Use for: pre-send input attachment queue
//
//  List   — rounded card container, separator lines between rows
//           Each row: [38×38 icon/thumb] | [name + MIME] | [size] | [✕]
//           Remove button fades from 40% → 100% on row hover
//           Use for: upload progress / file manager views
//
//  USAGE:
//    // Grid (message history)
//    var grid = new AttachmentsContainer(AttachmentVariant.Grid);
//    grid.SetAttachments(files, id => RemoveFile(id));
//    MessageList.Children.Add(grid);
//
//    // Inline (input bar)
//    var strip = new AttachmentsContainer(AttachmentVariant.Inline);
//    strip.SetAttachments(pending, id => pending.Remove(id));
//    InputBar.Children.Add(strip);
//
//    // Build a single tile manually
//    var tile = AttachmentTile.CreateGrid(data, onRemove: id => …);
//
//  DATA MODEL  (mirrors Vercel AI SDK FileUIPart + SourceDocumentUIPart):
//    AttachmentData.MediaType — MIME string ("image/jpeg", "application/pdf")
//    AttachmentData.Url       — remote image URL (for BitmapImage)
//    AttachmentData.Bytes     — raw bytes (for local files)
//    AttachmentData.IsSourceDocument + SourceTitle — for RAG source cards
//
//  UTILITY FUNCTIONS  (mirrors JS getMediaCategory / getAttachmentLabel):
//    AttachmentHelpers.GetMediaCategory(data) → AttachmentMediaCategory
//    AttachmentHelpers.GetAttachmentLabel(data) → string
// =============================================================================

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace PotomacAnalyst.Controls;

// ─────────────────────────────────────────────────────────────────────────────
//  Enums & data model
// ─────────────────────────────────────────────────────────────────────────────

public enum AttachmentVariant { Grid, Inline, List }
public enum AttachmentMediaCategory { Image, Video, Audio, Document, Source, Unknown }

/// <summary>
/// Mirrors Vercel AI SDK FileUIPart + SourceDocumentUIPart with { id }.
/// Provide either Url (remote) or Bytes (local) for image preview.
/// </summary>
public sealed class AttachmentData
{
    public required string Id { get; init; }
    public required string FileName { get; init; }
    public string? MediaType { get; init; }  // MIME string
    public string? Url { get; init; }  // remote image
    public byte[]? Bytes { get; init; }  // local image bytes
    public long? SizeBytes { get; init; }
    public bool IsSourceDocument { get; init; }
    public string? SourceTitle { get; init; }

    public string DisplayName =>
        IsSourceDocument ? (SourceTitle ?? FileName) : FileName;

    public AttachmentMediaCategory Category =>
        AttachmentHelpers.GetMediaCategory(this);
}

// ─────────────────────────────────────────────────────────────────────────────
//  Utility functions  (getMediaCategory / getAttachmentLabel)
// ─────────────────────────────────────────────────────────────────────────────

public static class AttachmentHelpers
{
    public static AttachmentMediaCategory GetMediaCategory(AttachmentData d)
    {
        if (d.IsSourceDocument) return AttachmentMediaCategory.Source;

        var mime = (d.MediaType ?? "").ToLowerInvariant();
        if (mime.StartsWith("image/")) return AttachmentMediaCategory.Image;
        if (mime.StartsWith("video/")) return AttachmentMediaCategory.Video;
        if (mime.StartsWith("audio/")) return AttachmentMediaCategory.Audio;
        if (mime is "application/pdf"
                 or "application/msword"
                 or "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                 or "application/vnd.ms-excel"
                 or "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                 or "application/vnd.ms-powerpoint"
                 or "application/vnd.openxmlformats-officedocument.presentationml.presentation"
                 or "text/plain" or "text/html" or "text/csv" or "text/markdown")
            return AttachmentMediaCategory.Document;

        // Fallback: extension sniffing
        return Path.GetExtension(d.FileName).TrimStart('.').ToLowerInvariant() switch
        {
            "jpg" or "jpeg" or "png" or "gif" or "webp" or "svg" or "bmp" or "avif"
                => AttachmentMediaCategory.Image,
            "mp4" or "mov" or "avi" or "mkv" or "webm"
                => AttachmentMediaCategory.Video,
            "mp3" or "wav" or "ogg" or "flac" or "m4a" or "aac"
                => AttachmentMediaCategory.Audio,
            "pdf" or "doc" or "docx" or "xls" or "xlsx"
                or "ppt" or "pptx" or "txt" or "csv" or "md" or "html"
                => AttachmentMediaCategory.Document,
            _ => AttachmentMediaCategory.Unknown,
        };
    }

    public static string GetAttachmentLabel(AttachmentData d)
    {
        if (!string.IsNullOrWhiteSpace(d.DisplayName)) return d.DisplayName;
        return d.Category switch
        {
            AttachmentMediaCategory.Image => "Image",
            AttachmentMediaCategory.Video => "Video",
            AttachmentMediaCategory.Audio => "Audio recording",
            AttachmentMediaCategory.Document => "Document",
            AttachmentMediaCategory.Source => "Source",
            _ => "Attachment",
        };
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  Design tokens  (Potomac dark theme — matches GenUiCardBuilder palette)
// ─────────────────────────────────────────────────────────────────────────────

internal static class AT
{
    // Surfaces
    public static readonly Color CardBg = Color.FromArgb(255, 0x1C, 0x1C, 0x24);
    public static readonly Color CardBgHov = Color.FromArgb(255, 0x26, 0x26, 0x32);
    public static readonly Color PillBg = Color.FromArgb(255, 0x22, 0x22, 0x2E);
    public static readonly Color PillBgHov = Color.FromArgb(255, 0x2E, 0x2E, 0x3C);
    public static readonly Color ListBg = Color.FromArgb(255, 0x18, 0x18, 0x22);
    public static readonly Color RowBgHov = Color.FromArgb(20, 255, 255, 255);
    public static readonly Color NameOverlay = Color.FromArgb(185, 0x0C, 0x0C, 0x12);

    // Borders
    public static readonly Color Border = Color.FromArgb(55, 255, 255, 255);
    public static readonly Color BorderHov = Color.FromArgb(115, 255, 255, 255);
    public static readonly Color Divider = Color.FromArgb(35, 255, 255, 255);

    // Text
    public static readonly Color TxtPrimary = Color.FromArgb(255, 0xF0, 0xF0, 0xFF);
    public static readonly Color TxtSecondary = Color.FromArgb(200, 0xB0, 0xB8, 0xCC);
    public static readonly Color TxtMuted = Color.FromArgb(130, 0x80, 0x88, 0x99);

    // Remove
    public static readonly Color RemRed = Color.FromArgb(255, 0xE0, 0x4A, 0x4A);
    public static readonly Color RemDim = Color.FromArgb(50, 0xE0, 0x4A, 0x4A);

    // Category accents
    public static readonly Color AImage = Color.FromArgb(255, 0x4A, 0x9E, 0xE0);
    public static readonly Color AVideo = Color.FromArgb(255, 0xA0, 0x6E, 0xE8);
    public static readonly Color AAudio = Color.FromArgb(255, 0x2E, 0xCC, 0xB8);
    public static readonly Color ADoc = Color.FromArgb(255, 0xE0, 0x8C, 0x4A);
    public static readonly Color ASource = Color.FromArgb(255, 0x4A, 0xE0, 0x6A);
    public static readonly Color AUnknown = Color.FromArgb(130, 0x80, 0x88, 0x99);

    // Helpers
    public static SolidColorBrush B(Color c) => new(c);

    public static Color Accent(AttachmentMediaCategory cat) => cat switch
    {
        AttachmentMediaCategory.Image => AImage,
        AttachmentMediaCategory.Video => AVideo,
        AttachmentMediaCategory.Audio => AAudio,
        AttachmentMediaCategory.Document => ADoc,
        AttachmentMediaCategory.Source => ASource,
        _ => AUnknown,
    };

    public static string Glyph(AttachmentMediaCategory cat) => cat switch
    {
        AttachmentMediaCategory.Image => "\uEB9F",  // FileImage
        AttachmentMediaCategory.Video => "\uE8B2",  // Video
        AttachmentMediaCategory.Audio => "\uE8D6",  // MusicNote
        AttachmentMediaCategory.Document => "\uE8A5",  // Document
        AttachmentMediaCategory.Source => "\uE946",  // Globe / Link
        _ => "\uE8A5",   // Document fallback
    };

    // Lazy font accessors matching PotomacTheme.xaml keys
    private static FontFamily? _reg, _med, _bld;
    public static FontFamily Reg => _reg ??= (FontFamily)Application.Current.Resources["QuicksandRegular"];
    public static FontFamily Med => _med ??= (FontFamily)Application.Current.Resources["QuicksandMedium"];
    public static FontFamily Bld => _bld ??= (FontFamily)Application.Current.Resources["RajdhaniBold"];

    public static readonly FontFamily FluentIcons = new("Segoe Fluent Icons");
    public static readonly GridLength Star = new(1, GridUnitType.Star);
}

// ─────────────────────────────────────────────────────────────────────────────
//  AttachmentsContainer  — <Attachments variant="…">…</Attachments>
// ─────────────────────────────────────────────────────────────────────────────

public sealed class AttachmentsContainer : ContentControl
{
    private readonly AttachmentVariant _variant;
    private List<AttachmentData> _items = new();
    private Action<string>? _onRemove;
    private readonly StackPanel _root = new();

    public AttachmentsContainer(AttachmentVariant variant)
    {
        _variant = variant;
        Content = _root;
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void SetAttachments(IEnumerable<AttachmentData> items, Action<string>? onRemove = null)
    {
        _items = items.ToList();
        _onRemove = onRemove;
        Rebuild();
    }

    public void AddAttachment(AttachmentData item) { _items.Add(item); Rebuild(); }
    public void RemoveAttachment(string id) { _items.RemoveAll(x => x.Id == id); Rebuild(); }
    public void Clear() { _items.Clear(); Rebuild(); }
    public int Count => _items.Count;

    // ── Rebuild ───────────────────────────────────────────────────────────

    private void Rebuild()
    {
        _root.Children.Clear();
        if (_items.Count == 0) { BuildEmpty(); return; }
        switch (_variant)
        {
            case AttachmentVariant.Grid: BuildGrid(); break;
            case AttachmentVariant.Inline: BuildInline(); break;
            case AttachmentVariant.List: BuildList(); break;
        }
    }

    // ─── Grid ────────────────────────────────────────────────────────────
    // VariableSizedWrapGrid: each cell = 96×96, tile inside = 84×84 + 12px margin
    private void BuildGrid()
    {
        var wrap = new VariableSizedWrapGrid
        {
            Orientation = Orientation.Horizontal,
            ItemWidth = 96,
            ItemHeight = 96,
            MaximumRowsOrColumns = 10,
        };
        foreach (var item in _items)
            wrap.Children.Add(AttachmentTile.CreateGrid(item, _onRemove));
        _root.Children.Add(wrap);
    }

    // ─── Inline ──────────────────────────────────────────────────────────
    private void BuildInline()
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        foreach (var item in _items)
            strip.Children.Add(AttachmentTile.CreateInline(item, _onRemove));
        _root.Children.Add(new ScrollViewer
        {
            Content = strip,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
        });
    }

    // ─── List ────────────────────────────────────────────────────────────
    private void BuildList()
    {
        var inner = new StackPanel();
        for (int i = 0; i < _items.Count; i++)
        {
            if (i > 0)
                inner.Children.Add(new Border
                {
                    Height = 1,
                    Background = AT.B(AT.Divider),
                    Margin = new Thickness(14, 0, 14, 0),
                });
            inner.Children.Add(
                AttachmentTile.CreateListRow(
                    _items[i], _onRemove,
                    isFirst: i == 0,
                    isLast: i == _items.Count - 1));
        }
        _root.Children.Add(new Border
        {
            Background = AT.B(AT.ListBg),
            BorderBrush = AT.B(AT.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Child = inner,
        });
    }

    // ─── Empty state  (<AttachmentEmpty />) ──────────────────────────────
    private void BuildEmpty()
    {
        var stack = new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(0, 20, 0, 20),
        };
        stack.Children.Add(new FontIcon
        {
            Glyph = "\uE7C3",
            FontFamily = AT.FluentIcons,
            FontSize = 24,
            Foreground = AT.B(AT.TxtMuted),
            HorizontalAlignment = HorizontalAlignment.Center,
        });
        stack.Children.Add(new TextBlock
        {
            Text = "No attachments",
            FontFamily = AT.Reg,
            FontSize = 13,
            Foreground = AT.B(AT.TxtMuted),
            HorizontalAlignment = HorizontalAlignment.Center,
        });
        _root.Children.Add(stack);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  AttachmentTile  — individual tile/pill/row factory
//  Mirrors the composable <Attachment> + <AttachmentPreview> / <AttachmentRemove>
// ─────────────────────────────────────────────────────────────────────────────

public static class AttachmentTile
{
    // =========================================================================
    //  Grid tile  —  <Attachment> with <AttachmentPreview> + <AttachmentRemove>
    //  in grid variant context
    // =========================================================================
    public static UIElement CreateGrid(AttachmentData item, Action<string>? onRemove)
    {
        var accent = AT.Accent(item.Category);

        var tile = new Border
        {
            Width = 84,
            Height = 84,
            CornerRadius = new CornerRadius(10),
            BorderBrush = AT.B(AT.Border),
            BorderThickness = new Thickness(1),
            Background = AT.B(AT.CardBg),
            Margin = new Thickness(0, 0, 12, 12),
        };

        var grid = new Grid();

        // ── AttachmentPreview layer ───────────────────────────────────────
        if (item.Category == AttachmentMediaCategory.Image)
        {
            var img = MakeImageElement(item, 84, 84, Stretch.UniformToFill);
            grid.Children.Add(img);
        }
        else
        {
            // Category icon + extension tag
            var iconStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 4,
            };
            var icon = new FontIcon
            {
                Glyph = AT.Glyph(item.Category),
                FontFamily = AT.FluentIcons,
                FontSize = 24,
                Foreground = AT.B(accent),
            };
            iconStack.Children.Add(icon);

            var extTag = Path.GetExtension(item.FileName).TrimStart('.').ToUpper();
            if (extTag.Length > 0)
                iconStack.Children.Add(new TextBlock
                {
                    Text = extTag,
                    FontFamily = AT.Reg,
                    FontSize = 9,
                    CharacterSpacing = 60,
                    Foreground = AT.B(AT.TxtMuted),
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            grid.Children.Add(iconStack);
        }

        // ── AttachmentInfo — frosted name strip at bottom ─────────────────
        grid.Children.Add(new Border
        {
            Background = AT.B(AT.NameOverlay),
            Padding = new Thickness(6, 3, 6, 5),
            VerticalAlignment = VerticalAlignment.Bottom,
            Child = new TextBlock
            {
                Text = item.DisplayName,
                FontFamily = AT.Reg,
                FontSize = 10,
                Foreground = AT.B(AT.TxtPrimary),
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap,
            },
        });

        // ── AttachmentRemove — fades in on hover ──────────────────────────
        Button? removeBtn = null;
        if (onRemove is not null)
        {
            removeBtn = MakeRemoveCircle(item.Id, onRemove, 22);
            removeBtn.Opacity = 0;
            removeBtn.HorizontalAlignment = HorizontalAlignment.Right;
            removeBtn.VerticalAlignment = VerticalAlignment.Top;
            removeBtn.Margin = new Thickness(0, 6, 6, 0);
            grid.Children.Add(removeBtn);
        }

        tile.Child = grid;

        // Hover: border brightens, remove fades in
        var capturedBtn = removeBtn;
        tile.PointerEntered += (_, _) =>
        {
            tile.BorderBrush = AT.B(AT.BorderHov);
            tile.Background = AT.B(AT.CardBgHov);
            if (capturedBtn is not null) Fade(capturedBtn, 1, 140);
        };
        tile.PointerExited += (_, _) =>
        {
            tile.BorderBrush = AT.B(AT.Border);
            tile.Background = AT.B(AT.CardBg);
            if (capturedBtn is not null) Fade(capturedBtn, 0, 140);
        };

        // Wrap in a 96×96 cell so VSWG ItemWidth/ItemHeight work correctly
        var cell = new Grid { Width = 96, Height = 96 };
        cell.Children.Add(tile);
        return cell;
    }

    // =========================================================================
    //  Inline pill  —  compact badge variant
    // =========================================================================
    public static UIElement CreateInline(AttachmentData item, Action<string>? onRemove)
    {
        var accent = AT.Accent(item.Category);

        var pill = new Border
        {
            Height = 36,
            CornerRadius = new CornerRadius(18),
            BorderBrush = AT.B(AT.Border),
            BorderThickness = new Thickness(1),
            Background = AT.B(AT.PillBg),
            Padding = new Thickness(4, 0, onRemove is not null ? 6 : 10, 0),
        };

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // ── Round thumbnail or icon ───────────────────────────────────────
        var thumbBorder = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(14),
            Background = AT.B(Color.FromArgb(60, accent.R, accent.G, accent.B)),
        };
        if (item.Category == AttachmentMediaCategory.Image)
        {
            thumbBorder.Child = MakeImageElement(item, 28, 28, Stretch.UniformToFill);
        }
        else
        {
            var icon = new FontIcon
            {
                Glyph = AT.Glyph(item.Category),
                FontFamily = AT.FluentIcons,
                FontSize = 14,
                Foreground = AT.B(accent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            thumbBorder.Child = icon;
        }
        row.Children.Add(thumbBorder);

        // ── Filename ──────────────────────────────────────────────────────
        row.Children.Add(new TextBlock
        {
            Text = TruncateName(item.DisplayName, 22),
            FontFamily = AT.Reg,
            FontSize = 12,
            Foreground = AT.B(AT.TxtPrimary),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
        });

        // ── Tiny ✕ remove ─────────────────────────────────────────────────
        if (onRemove is not null)
            row.Children.Add(MakeRemoveTiny(item.Id, onRemove));

        pill.Child = row;

        // Hover
        pill.PointerEntered += (_, _) => { pill.Background = AT.B(AT.PillBgHov); pill.BorderBrush = AT.B(AT.BorderHov); };
        pill.PointerExited += (_, _) => { pill.Background = AT.B(AT.PillBg); pill.BorderBrush = AT.B(AT.Border); };

        // Image tap → flyout preview  (<AttachmentHoverCard />)
        if (item.Category == AttachmentMediaCategory.Image)
        {
            pill.PointerPressed += (_, _) => ShowFlyout(pill, item);
            ToolTipService.SetToolTip(pill, "Click to preview");
        }

        return pill;
    }

    // =========================================================================
    //  List row  —  <AttachmentInfo showMediaType> + size + remove
    // =========================================================================
    public static UIElement CreateListRow(
        AttachmentData item, Action<string>? onRemove,
        bool isFirst = false, bool isLast = false)
    {
        var accent = AT.Accent(item.Category);

        double rTop = isFirst ? 12 : 0;
        double rBottom = isLast ? 12 : 0;

        var rowBorder = new Border
        {
            CornerRadius = new CornerRadius(rTop, rTop, rBottom, rBottom),
            Padding = new Thickness(14, 10, 14, 10),
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // icon
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = AT.Star }); // name+mime
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // size
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // remove

        // ── Col 0: AttachmentPreview (icon / thumbnail) ───────────────────
        var iconHost = new Border
        {
            Width = 38,
            Height = 38,
            CornerRadius = new CornerRadius(8),
            Background = AT.B(Color.FromArgb(50, accent.R, accent.G, accent.B)),
            Margin = new Thickness(0, 0, 12, 0),
        };
        if (item.Category == AttachmentMediaCategory.Image &&
            (item.Url is not null || item.Bytes is not null))
        {
            iconHost.Child = MakeImageElement(item, 38, 38, Stretch.UniformToFill);
        }
        else
        {
            iconHost.Child = new FontIcon
            {
                Glyph = AT.Glyph(item.Category),
                FontFamily = AT.FluentIcons,
                FontSize = 18,
                Foreground = AT.B(accent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        Grid.SetColumn(iconHost, 0);
        grid.Children.Add(iconHost);

        // ── Col 1: AttachmentInfo (name + MIME) ───────────────────────────
        var infoStack = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };
        infoStack.Children.Add(new TextBlock
        {
            Text = item.DisplayName,
            FontFamily = AT.Reg,
            FontSize = 13,
            Foreground = AT.B(AT.TxtPrimary),
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap,
        });
        if (!string.IsNullOrEmpty(item.MediaType))
            infoStack.Children.Add(new TextBlock
            {
                Text = item.MediaType,
                FontFamily = AT.Reg,
                FontSize = 11,
                Foreground = AT.B(AT.TxtMuted),
            });
        Grid.SetColumn(infoStack, 1);
        grid.Children.Add(infoStack);

        // ── Col 2: file size ──────────────────────────────────────────────
        if (item.SizeBytes.HasValue)
        {
            var sizeLbl = new TextBlock
            {
                Text = FormatSize(item.SizeBytes.Value),
                FontFamily = AT.Reg,
                FontSize = 11,
                Foreground = AT.B(AT.TxtMuted),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0),
            };
            Grid.SetColumn(sizeLbl, 2);
            grid.Children.Add(sizeLbl);
        }

        // ── Col 3: AttachmentRemove ───────────────────────────────────────
        Button? removeBtn = null;
        if (onRemove is not null)
        {
            removeBtn = MakeRemoveCircle(item.Id, onRemove, 26);
            removeBtn.Opacity = 0.4;
            removeBtn.VerticalAlignment = VerticalAlignment.Center;
            removeBtn.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumn(removeBtn, 3);
            grid.Children.Add(removeBtn);
        }

        rowBorder.Child = grid;

        // Row hover
        var btn = removeBtn;
        rowBorder.PointerEntered += (_, _) => { rowBorder.Background = AT.B(AT.RowBgHov); if (btn is not null) Fade(btn, 1.0, 120); };
        rowBorder.PointerExited += (_, _) => { rowBorder.Background = null; if (btn is not null) Fade(btn, 0.4, 120); };

        return rowBorder;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    // ── Image loading ─────────────────────────────────────────────────────
    private static Microsoft.UI.Xaml.Controls.Image MakeImageElement(
        AttachmentData item, double w, double h, Stretch stretch)
    {
        var img = new Microsoft.UI.Xaml.Controls.Image
        {
            Width = w,
            Height = h,
            Stretch = stretch,
        };
        _ = LoadImageAsync(img, item);
        return img;
    }

    private static async Task LoadImageAsync(
        Microsoft.UI.Xaml.Controls.Image img, AttachmentData item)
    {
        try
        {
            if (item.Bytes is not null)
            {
                var bmp = new BitmapImage();
                using var ms = new MemoryStream(item.Bytes);
                // Use the built-in WindowsRuntimeStreamExtensions.AsRandomAccessStream()
                // from System.Runtime.InteropServices.WindowsRuntime (in GlobalUsings.cs).
                // It wraps the stream synchronously — no async, no deadlock risk.
                var ras = ms.AsRandomAccessStream();
                await bmp.SetSourceAsync(ras);
                img.Source = bmp;
            }
            else if (!string.IsNullOrEmpty(item.Url))
            {
                img.Source = new BitmapImage(new Uri(item.Url));
            }
        }
        catch { /* silent — icon background remains */ }
    }

    // ── Remove buttons ────────────────────────────────────────────────────

    // Circular button: used on grid (22px) and list rows (26px)
    private static Button MakeRemoveCircle(string id, Action<string> onRemove, double size)
    {
        var btn = new Button
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(size / 2),
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0),
            Content = new FontIcon
            {
                Glyph = "\uE711",
                FontFamily = AT.FluentIcons,
                FontSize = size * 0.38,
                Foreground = AT.B(AT.RemRed),
            },
        };
        // Override WinUI button chrome
        btn.Resources["ButtonBackground"] = AT.B(AT.RemDim);
        btn.Resources["ButtonBackgroundPointerOver"] = AT.B(AT.RemRed);
        btn.Resources["ButtonBackgroundPressed"] = AT.B(Color.FromArgb(220, AT.RemRed.R, AT.RemRed.G, AT.RemRed.B));
        btn.Resources["ButtonBorderBrush"] = new SolidColorBrush(Colors.Transparent);
        btn.Resources["ButtonBorderBrushPointerOver"] = new SolidColorBrush(Colors.Transparent);
        btn.Resources["ButtonForeground"] = AT.B(AT.RemRed);
        btn.Resources["ButtonForegroundPointerOver"] = new SolidColorBrush(Colors.White);
        btn.Resources["ButtonForegroundPressed"] = new SolidColorBrush(Colors.White);
        btn.PointerEntered += (_, _) => { if (btn.Content is FontIcon fi) fi.Foreground = new SolidColorBrush(Colors.White); };
        btn.PointerExited += (_, _) => { if (btn.Content is FontIcon fi) fi.Foreground = AT.B(AT.RemRed); };
        btn.Click += (_, _) => onRemove(id);
        return btn;
    }

    // Tiny transparent × at the right edge of inline pills
    private static UIElement MakeRemoveTiny(string id, Action<string> onRemove)
    {
        var btn = new Button
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            Content = new FontIcon
            {
                Glyph = "\uE711",
                FontFamily = AT.FluentIcons,
                FontSize = 7,
                Foreground = AT.B(AT.TxtMuted),
            },
        };
        btn.Resources["ButtonBackground"] = new SolidColorBrush(Colors.Transparent);
        btn.Resources["ButtonBackgroundPointerOver"] = AT.B(AT.RemDim);
        btn.Resources["ButtonBackgroundPressed"] = AT.B(AT.RemDim);
        btn.Resources["ButtonBorderBrush"] = new SolidColorBrush(Colors.Transparent);
        btn.Resources["ButtonBorderBrushPointerOver"] = new SolidColorBrush(Colors.Transparent);
        btn.Resources["ButtonForeground"] = AT.B(AT.TxtMuted);
        btn.Resources["ButtonForegroundPointerOver"] = AT.B(AT.RemRed);
        btn.Resources["ButtonForegroundPressed"] = AT.B(AT.RemRed);
        btn.PointerEntered += (_, _) => { if (btn.Content is FontIcon fi) fi.Foreground = AT.B(AT.RemRed); };
        btn.PointerExited += (_, _) => { if (btn.Content is FontIcon fi) fi.Foreground = AT.B(AT.TxtMuted); };
        btn.Click += (_, _) => onRemove(id);
        return btn;
    }

    // ── AttachmentHoverCard — image flyout preview ────────────────────────
    private static void ShowFlyout(FrameworkElement anchor, AttachmentData item)
    {
        var previewImg = MakeImageElement(item, 0, 0, Stretch.Uniform);
        previewImg.MaxWidth = 300;
        previewImg.MaxHeight = 220;

        var flyout = new Flyout
        {
            Content = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = AT.B(Color.FromArgb(255, 0x14, 0x14, 0x1E)),
                BorderBrush = AT.B(AT.Border),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Child = previewImg,
            },
            Placement = FlyoutPlacementMode.Top,
        };

        var style = new Style(typeof(FlyoutPresenter));
        style.Setters.Add(new Setter(FlyoutPresenter.PaddingProperty, new Thickness(0)));
        style.Setters.Add(new Setter(FlyoutPresenter.CornerRadiusProperty, new CornerRadius(10)));
        style.Setters.Add(new Setter(FlyoutPresenter.BorderThicknessProperty, new Thickness(0)));
        style.Setters.Add(new Setter(FlyoutPresenter.BackgroundProperty,
            new SolidColorBrush(Colors.Transparent)));
        flyout.FlyoutPresenterStyle = style;

        FlyoutBase.SetAttachedFlyout(anchor, flyout);
        FlyoutBase.ShowAttachedFlyout(anchor);
    }

    // ── Opacity fade animation ────────────────────────────────────────────
    // FIX Bug 10: the original signature took an explicit 'from' value.  When the
    // pointer quickly entered then left (or vice versa) before the previous
    // animation finished, the next animation would jump to the hardcoded 'from'
    // opacity before fading — producing a visible flash.  Reading el.Opacity at
    // call time ensures continuity regardless of mid-flight interrupts.
    private static void Fade(UIElement el, double to, int ms)
    {
        var anim = new DoubleAnimation
        {
            From = el.Opacity,   // start from wherever the element currently is
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(ms)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        var sb = new Storyboard();
        Storyboard.SetTarget(anim, el);
        Storyboard.SetTargetProperty(anim, "Opacity");
        sb.Children.Add(anim);
        sb.Begin();
    }

    // ── Text helpers ──────────────────────────────────────────────────────
    private static string TruncateName(string name, int max)
    {
        if (name.Length <= max) return name;
        var ext = Path.GetExtension(name);
        var stem = Path.GetFileNameWithoutExtension(name);
        var keep = Math.Max(1, max - ext.Length - 1);
        return stem[..Math.Min(stem.Length, keep)] + "…" + ext;
    }

    private static string FormatSize(long b) => b switch
    {
        < 1_024 => $"{b} B",
        < 1_048_576 => $"{b / 1024.0:F1} KB",
        < 1_073_741_824 => $"{b / 1_048_576.0:F1} MB",
        _ => $"{b / 1_073_741_824.0:F1} GB",
    };
}

// ─────────────────────────────────────────────────────────────────────────────
//  Stream extension  (MemoryStream → IRandomAccessStream for BitmapImage)
// ─────────────────────────────────────────────────────────────────────────────

internal static class AttachmentStreamExt
{
    public static async Task<Windows.Storage.Streams.IRandomAccessStream> AsRandomAccessStreamAsync(
        this MemoryStream ms)
    {
        var ras = new Windows.Storage.Streams.InMemoryRandomAccessStream();
        var writer = new Windows.Storage.Streams.DataWriter(ras.GetOutputStreamAt(0));
        writer.WriteBytes(ms.ToArray());
        await writer.StoreAsync();
        ras.Seek(0);
        return ras;
    }
}