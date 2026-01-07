using ClipboardSanitizer.Core;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClipboardSanitizer.Wpf;

public sealed class ClipboardMonitor : IDisposable
{
    private readonly Window _window;
    private readonly IClipboardSanitizer _sanitizer;
    private readonly ClipboardSanitizerOptions _options;

    private HwndSource? _hwndSource;
    private IntPtr _hwnd;

    private bool _suppressClipboard;
    private string? _lastProcessedText;
    private bool _disposed;

    public ClipboardMonitor(Window window, IClipboardSanitizer sanitizer, ClipboardSanitizerOptions? options = null)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        _options = options ?? new ClipboardSanitizerOptions();
    }

    public void Start()
    {
        if (_hwnd != IntPtr.Zero) return;

        _hwnd = new WindowInteropHelper(_window).Handle;
        if (_hwnd == IntPtr.Zero)
            throw new InvalidOperationException("Window handle is not available. Call Start() after the window is initialized.");

        NativeMethods.AddClipboardFormatListener(_hwnd);

        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource.AddHook(WndProc);
    }

    public void Stop()
    {
        if (_hwnd == IntPtr.Zero) return;

        try
        {
            if (_hwndSource != null)
                _hwndSource.RemoveHook(WndProc);

            NativeMethods.RemoveClipboardFormatListener(_hwnd);
        }
        finally
        {
            _hwndSource = null;
            _hwnd = IntPtr.Zero;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_CLIPBOARDUPDATE && !_suppressClipboard)
        {
            _window.Dispatcher.BeginInvoke(
                new Action(ProcessClipboardSafely),
                DispatcherPriority.Background);
        }

        return IntPtr.Zero;
    }

    private async void ProcessClipboardSafely()
    {
        if (_suppressClipboard) return;

        if (_options.OnlyWhenAppIsActive && !_window.IsActive)
            return;

        for (int i = 0; i < _options.ClipboardRetryCount; i++)
        {
            try
            {
                if (!Clipboard.ContainsText())
                    return;

                var original = Clipboard.GetText();

                // Prevent reprocessing the same content
                if (original == _lastProcessedText)
                    return;

                // Quick guard: only act if it actually ends with blank line(s)
                if (!EndsWithBlankLine(original))
                {
                    _lastProcessedText = original;
                    return;
                }

                var cleaned = _sanitizer.Sanitize(original);

                if (cleaned == original)
                {
                    _lastProcessedText = original;
                    return;
                }

                _suppressClipboard = true;
                Clipboard.SetText(cleaned);
                _lastProcessedText = cleaned;
                return;
            }
            catch (COMException)
            {
                await Task.Delay(_options.ClipboardRetryDelayMs);
            }
            finally
            {
                _suppressClipboard = false;
            }
        }
    }

    private static bool EndsWithBlankLine(string text)
    {
        // Normalize for check only
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        // If last char is newline, or last line is whitespace → likely the bug
        if (normalized.EndsWith("\n")) return true;

        var lastNewline = normalized.LastIndexOf('\n');
        if (lastNewline < 0) return false;

        var lastLine = normalized[(lastNewline + 1)..];
        return string.IsNullOrWhiteSpace(lastLine);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
