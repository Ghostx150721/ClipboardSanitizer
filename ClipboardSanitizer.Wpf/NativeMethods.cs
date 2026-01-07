using System.Runtime.InteropServices;

namespace ClipboardSanitizer.Wpf
{
    internal static class NativeMethods
    {
        internal const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    }
}
