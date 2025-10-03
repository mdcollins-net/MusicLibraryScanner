using System;
using System.Linq;
using System.Text;

namespace MusicLibraryScanner.Helpers
{
    public static class ConsoleTablePrinter
    {
        public static void PrintSideBySide(string[] leftTable, string[] rightTable, int padding = 4)
        {
            string combined = CombineSideBySide(leftTable, rightTable, padding);
            Console.WriteLine(combined);
        }

        public static string CombineSideBySide(string[] leftTable, string[] rightTable, int padding = 4)
        {
            int leftRows = leftTable.Length;
            int rightRows = rightTable.Length;
            int totalRows = Math.Max(leftRows, rightRows);

            int leftWidth = leftTable.Length > 0 ? leftTable.Max(line => line.Length) : 0;
            int rightWidth = rightTable.Length > 0 ? rightTable.Max(line => line.Length) : 0;

            var sb = new StringBuilder();

            // Calculate total width of both tables side by side
            int combinedWidth = leftWidth + padding + rightWidth;
            int windowWidth = Console.WindowWidth > 0 ? Console.WindowWidth : combinedWidth;
            int margin = Math.Max(0, (windowWidth - combinedWidth) / 2);

            for (int i = 0; i < totalRows; i++)
            {
                string leftLine = i < leftRows ? leftTable[i] : "";
                string rightLine = i < rightRows ? rightTable[i] : "";

                string line = leftLine.PadRight(leftWidth + padding) + rightLine;

                sb.AppendLine(new string(' ', margin) + line);
            }

            return sb.ToString();
        }
    }
}