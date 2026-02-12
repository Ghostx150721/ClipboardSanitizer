# ClipboardSanitizer (WPF)

A small library + demo that listens for `WM_CLIPBOARDUPDATE` and sanitizes clipboard text at the application boundary.

## Why
Some PDF viewers and other apps may copy text with unintended trailing blank lines. This project trims trailing blank lines while keeping internal formatting intact.

## Projects
- ClipboardSanitizer.Core: Sanitizer interfaces + implementations
- ClipboardSanitizer.Wpf: WPF clipboard monitor (window message hook + retry handling)
- ClipboardSanitizer.Demo.Wpf: Demo app

## Usage
```csharp
var monitor = new ClipboardMonitor(
    window: this,
    sanitizer: new TrailingBlankLinesSanitizer(),
    options: new ClipboardSanitizerOptions { OnlyWhenAppIsActive = true });

monitor.Start();
