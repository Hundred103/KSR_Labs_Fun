using System;

namespace ConsoleCol
{
    public static class ConsoleCol
    {
        public static void Write(string value, ConsoleColor col)
            => Console.Write(ToAnsi(value, col));

        public static void WriteLine(string value, ConsoleColor col)
            => Console.WriteLine(ToAnsi(value, col));

        public static Task WriteAsync(string value, ConsoleColor col)
            => Console.Out.WriteAsync(ToAnsi(value, col));

        public static Task WriteLineAsync(string value, ConsoleColor col)
            => Console.Out.WriteLineAsync(ToAnsi(value, col));

        // Overloads for extended colors (BrightRed, Orange, etc.)
        public static void Write(string value, string colorCode)
            => Console.Write(ToAnsiExtended(value, colorCode));

        public static void WriteLine(string value, string colorCode)
            => Console.WriteLine(ToAnsiExtended(value, colorCode));

        public static Task WriteAsync(string value, string colorCode)
            => Console.Out.WriteAsync(ToAnsiExtended(value, colorCode));

        public static Task WriteLineAsync(string value, string colorCode)
            => Console.Out.WriteLineAsync(ToAnsiExtended(value, colorCode));

        private static string ToAnsi(string text, ConsoleColor col)
        {
            string code = col switch
            {
                ConsoleColor.Red => "31",
                ConsoleColor.Green => "32",
                ConsoleColor.Yellow => "33",
                ConsoleColor.Blue => "34",
                ConsoleColor.Magenta => "35",
                ConsoleColor.Cyan => "36",
                ConsoleColor.Gray => "90",
                ConsoleColor.DarkGray => "90",
                _ => "37" // Domyślny biały
            };
            return $"\x1b[{code}m{text}\x1b[0m";
        }

        private static string ToAnsiExtended(string text, string colorCode)
            => $"\x1b[{colorCode}m{text}\x1b[0m";

        public static class Colors
        {
            public const string BrightRed = "91";
            public const string BrightGreen = "92";
            public const string Orange = "33";        // Yellow/Orange
            public const string BrightYellow = "93";  // Bright orange
            public const string BrightBlue = "94";
            public const string BrightMagenta = "95";
            public const string BrightCyan = "96";
            public const string White = "97";
            public const string DarkRed = "31";
            public const string DarkGreen = "32";
            public const string DarkYellow = "33";
        }
    }
}
