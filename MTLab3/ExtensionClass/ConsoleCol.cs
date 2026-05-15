using System;

namespace ExtensionClass
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
    }
}