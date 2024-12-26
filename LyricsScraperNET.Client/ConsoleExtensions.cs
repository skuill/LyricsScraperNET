using System;

namespace LyricsScraperNET.Client
{
    public static class ConsoleExtensions
    {
        private static readonly char indentSymbol = '-';

        public static void WriteLineColored(this string str, ConsoleColor color = ConsoleColor.Gray, bool reset = true)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            if (reset)
                Console.ResetColor();
        }

        public static void WriteLineDelimeter(int repeatAmount = 20, ConsoleColor color = ConsoleColor.Gray, bool reset = true)
        {
            Console.ForegroundColor = color;
            Console.WriteLine();
            Console.WriteLine(new String(indentSymbol, repeatAmount));
            Console.WriteLine();
            if (reset)
                Console.ResetColor();
        }
    }
}
