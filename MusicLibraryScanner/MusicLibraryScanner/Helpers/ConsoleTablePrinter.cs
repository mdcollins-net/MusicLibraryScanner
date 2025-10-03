// Helpers/ConsoleTablePrinter.cs

using System.Text;

namespace MusicLibraryScanner.Helpers {
    public static class ConsoleTablePrinter {
        public static void PrintSideBySide(string[] leftTable, string[] rightTable, int padding = 4) {
            string combined = CombineSideBySide(leftTable, rightTable, padding);
            Console.WriteLine(combined);
        }

        private static string CombineSideBySide(string[] leftTable, string[] rightTable, int padding = 4) {
            var leftRows = leftTable.Length;
            var rightRows = rightTable.Length;
            var totalRows = Math.Max(leftRows, rightRows);

            var leftWidth = leftTable.Length > 0 ? leftTable.Max(line => line.Length) : 0;
            var rightWidth = rightTable.Length > 0 ? rightTable.Max(line => line.Length) : 0;

            var sb = new StringBuilder();

            // Calculate total width of both tables side by side
            var combinedWidth = leftWidth + padding + rightWidth;
            var windowWidth = Console.WindowWidth > 0 ? Console.WindowWidth : combinedWidth;
            var margin = Math.Max(0, (windowWidth - combinedWidth) / 2);

            for (var i = 0; i < totalRows; i++) {
                var leftLine = i < leftRows ? leftTable[i] : "";
                var rightLine = i < rightRows ? rightTable[i] : "";

                var line = leftLine.PadRight(leftWidth + padding) + rightLine;

                sb.AppendLine(new string(' ', margin) + line);
            }

            return sb.ToString();
        }
    }
}