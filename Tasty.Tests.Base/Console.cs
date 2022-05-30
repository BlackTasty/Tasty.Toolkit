using System;

namespace Tasty.Tests.Base
{
    public static class Console
    {
        public static int titlePosition = 4;
        public static int statusPosition = 80;
        public static char fillCharacter = '.';

        public static void WriteLine(string message, ConsoleColor backgroundColor)
        {
            System.Console.BackgroundColor = backgroundColor;
            System.Console.ForegroundColor = ConsoleColor.Black;
            System.Console.WriteLine(message);
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void WriteLine_Status(string message, bool success)
        {
            WriteLine_Status(message, success ? Status.Success : Status.Fail);
        }

        public static void WriteLine_Status(string message, Status status)
        {
            if (titlePosition > 0)
            {
                System.Console.Write(new string(' ', titlePosition));
            }

            if (!message.EndsWith("!"))
            {
                message = string.Format("{0}:", message);
            }

            System.Console.Write(message);
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            int calcStatusPosition = statusPosition - message.Length - titlePosition;

            if (calcStatusPosition > 0)
            {
                System.Console.Write(new string(fillCharacter, calcStatusPosition));
            }
            System.Console.ForegroundColor = ConsoleColor.Gray;

            switch (status)
            {
                case Status.Success:
                    WriteLine("  OK  ", ConsoleColor.DarkGreen);
                    break;
                case Status.Fail:
                    WriteLine(" FAIL ", ConsoleColor.DarkRed);
                    break;
                case Status.Warning:
                    WriteLine(" WARN ", ConsoleColor.DarkYellow);
                    break;
            }
        }
    }
}
