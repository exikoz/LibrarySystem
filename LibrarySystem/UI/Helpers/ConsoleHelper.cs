
namespace LibrarySystem.UI.Helpers
{
    /// <summary>
    /// Helper class for console output with colors and formatting
    /// </summary>
    public static class ConsoleHelper
    {
        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        public static void WriteHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            int width = Math.Max(title.Length + 8, 50);
            string border = new string('═', width);

            Console.WriteLine($"╔{border}╗");
            Console.WriteLine($"║   {title.PadRight(width - 3)}║");
            Console.WriteLine($"╚{border}╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void WriteSimpleHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=== {title} ===\n");
            Console.ResetColor();
        }

        public static void WriteMenuOption(string number, string description)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{number}. ");
            Console.ResetColor();
            Console.WriteLine(description);
        }

        public static void WriteSeparator(char character = '-', int length = 50)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(character, length));
            Console.ResetColor();
        }

        public static string Prompt(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{message}: ");
            Console.ResetColor();
            return Console.ReadLine() ?? "";
        }

        public static string PromptUntilValid(string message, Func<string, bool> validator, string errorMessage)
        {
            while (true)
            {
                var input = Prompt(message);
                if (validator(input))
                {
                    return input;
                }
                WriteError(errorMessage);
            }
        }

        public static void PauseWithMessage(string message = "Press any key to continue...")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        public static void ClearScreen()
        {
            Console.Clear();
            Console.ResetColor();
        }

        public static T SelectFromList<T>(IEnumerable<T> items, Func<T, string> displaySelector, string prompt)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0) return default;

            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                // Clear the area for the list
                // We will use a simple redraw loop.
                Console.Clear();
                
                WriteSimpleHeader(prompt);

                for (int i = 0; i < itemList.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"> {displaySelector(itemList[i])}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {displaySelector(itemList[i])}");
                    }
                }

                Console.WriteLine("\n(Use Arrow Keys to Navigate, Enter to Select, Esc to Cancel)");

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex--;
                        if (selectedIndex < 0) selectedIndex = itemList.Count - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex++;
                        if (selectedIndex >= itemList.Count) selectedIndex = 0;
                        break;
                }

            } while (key != ConsoleKey.Enter && key != ConsoleKey.Escape);

            if (key == ConsoleKey.Escape) return default;
            return itemList[selectedIndex];
        }

        public static void ShowSpinner(string message, Func<Task> action)
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            // Simple Logo
            Console.WriteLine("    ======================================");
            Console.WriteLine("         L I B R A R Y   S Y S T E M      ");
            Console.WriteLine("    ======================================");
            Console.WriteLine("\n");
            
            Console.Write($"    {message} ");

            var animation = new[] { "/", "-", "\\", "|" };
            var task = action();
            
            int counter = 0;
            // Use a simple polling loop for the spinner
            while (!task.IsCompleted)
            {
                Console.Write(animation[counter++ % animation.Length]);
                Thread.Sleep(100);
                Console.Write("\b");
            }
            
            // Re-throw any exceptions from the task
            if (task.Exception != null)
            {
                throw task.Exception;
            }

            Console.Clear();
        }
    }
}
