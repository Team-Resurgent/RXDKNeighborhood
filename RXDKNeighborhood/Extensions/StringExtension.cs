using System;
using System.Linq;

namespace RXDKNeighborhood.Extensions
{
    public static class StringExtension
    {
        public static void FormatXboxPath(this string value, out string ipAddress, out string path)
        {
            var parts = value.Split("\\").ToList();
            ipAddress = parts[0];
            parts.RemoveAt(0);
            if (parts.Count > 0)
            {
                parts[0] = $"{parts[0]}:";
            }
            path = System.IO.Path.Combine(parts.ToArray());
        }

        public static string ParentXboxPath(this string value)
        {
            var parts = value.Split("\\", StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count > 0)
            {
                parts.RemoveAt(parts.Count - 1);
            }
            return string.Join("\\", parts);
        }

        public static string FormatBytes(ulong bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
