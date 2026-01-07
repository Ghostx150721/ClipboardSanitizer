
namespace ClipboardSanitizer.Core
{
    public sealed class ClipboardSanitizerOptions
    {
        /// <summary>
        /// If true, only trims trailing blank lines. Recommended.
        /// </summary>
        public bool TrimTrailingBlankLines { get; set; } = true;

        /// <summary>
        /// If true, runs only when the app window is active (recommended for safety).
        /// </summary>
        public bool OnlyWhenAppIsActive { get; set; } = true;

        /// <summary>
        /// Max retries when clipboard is locked.
        /// </summary>
        public int ClipboardRetryCount { get; set; } = 5;

        /// <summary>
        /// Delay (ms) between clipboard retries.
        /// </summary>
        public int ClipboardRetryDelayMs { get; set; } = 30;
    }
}
