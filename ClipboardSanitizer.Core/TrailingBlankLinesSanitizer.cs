using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardSanitizer.Core
{
    public sealed class TrailingBlankLinesSanitizer
    {
        public string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Normalize for  processing
            var normalized = input.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = normalized.Split('\n').ToList();

            // Remove trailing blank lines
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
            {
                lines.RemoveAt(lines.Count - 1);
            }

            // Restore Windows new lines
            return string.Join("\r\n", lines);
        }
    }
}
