
namespace LibrarySystem.UI.Helpers
{
    public static class TableFormatter
    {
        public static void PrintRow(params string[] columns)
        {
            int width = (Console.WindowWidth - 10) / columns.Length;
            string row = "|";
            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }
            Console.WriteLine(row);
        }

        public static void PrintLine()
        {
            Console.WriteLine(new string('-', Console.WindowWidth - 10)); // Adjusted to match typical row width logic roughly, or just simple separator
        }

        private static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;
            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
        }
    }
}
