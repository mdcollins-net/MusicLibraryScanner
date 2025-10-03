// Helpers/ProcessingStats.cs

using System.Diagnostics;
using System.Text;
using log4net;

namespace MusicLibraryScanner.Helpers {
    public class ProcessingStats {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessingStats));
        private readonly Stopwatch _stopwatch = new();

        private DateTime _startTime;
        private DateTime _endTime;

        private int TrackCount { get; set; }
        private int AlbumCount { get; set; }
        private int ArtistCount { get; set; }

        /// <summary>
        /// Controls how many spaces separate the two tables in the console report.
        /// </summary>
        private int TableSpacing { get; set; } = 4;

        /// <summary>
        /// If true, the report is written only to the log file (no console output).
        /// </summary>
        public bool LogOnly { get; set; } = false;

        public void Start() {
            _startTime = DateTime.Now;
            _stopwatch.Start();
        }

        public void Stop() {
            _stopwatch.Stop();
            _endTime = DateTime.Now;
        }

        public void IncrementTrack() => TrackCount++;
        public void IncrementAlbum() => AlbumCount++;
        public void IncrementArtist() => ArtistCount++;

        private string GetDuration() {
            var ts = _stopwatch.Elapsed;
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s";
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }

        private List<string> BuildStatsTable() {
            return new List<string> {
                "+-------------+---------------------+",
                "| Statistics  | Count               |",
                "+-------------+---------------------+",
                $"| Artists     | {ArtistCount,19} |",
                $"| Albums      | {AlbumCount,19} |",
                $"| Tracks      | {TrackCount,19} |",
                "+-------------+---------------------+"
            };
        }

        private List<string> BuildTimesTable(double tracksPerSec, double tracksPerMin) {
            return new List<string> {
                "+-------------------+---------------------+",
                "| Metric            | Value               |",
                "+-------------------+---------------------+",
                $"| Start Time        | {_startTime:yyyy-MM-dd HH:mm:ss} |",
                $"| End Time          | {_endTime:yyyy-MM-dd HH:mm:ss} |",
                $"| Duration          | {GetDuration(),19} |",
                $"| Tracks per second | {tracksPerSec,19:F2} |",
                $"| Tracks per minute | {tracksPerMin,19:F2} |",
                "+-------------------+---------------------+"
            };
        }

        private static string CombineTables(List<string> left, List<string> right, int spacing) {
            var sb = new StringBuilder();
            var leftWidth = left.Max(l => l.Length);

            var maxLines = Math.Max(left.Count, right.Count);
            for (var i = 0; i < maxLines; i++) {
                var leftLine = i < left.Count ? left[i].PadRight(leftWidth) : new string(' ', leftWidth);
                var rightLine = i < right.Count ? right[i] : string.Empty;

                sb.AppendLine(leftLine + new string(' ', spacing) + rightLine);
            }

            return sb.ToString();
        }

        private string BuildReport() {
            var ts = _stopwatch.Elapsed;
            var tracksPerSec = ts.TotalSeconds > 0 ? TrackCount / ts.TotalSeconds : 0;
            var tracksPerMin = ts.TotalMinutes > 0 ? TrackCount / ts.TotalMinutes : 0;

            // Dynamic title and banner
            const string title = "* Music Library Scan Completed *";
            var bannerWidth = title.Length + 10;
            var bannerLine = new string('=', bannerWidth);
            var padding = (bannerWidth - title.Length) / 2;
            var centeredTitle = new string(' ', padding) + title;

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(bannerLine);
            sb.AppendLine(centeredTitle);
            sb.AppendLine(bannerLine);
            sb.AppendLine();

            var statsTable = BuildStatsTable();
            var timesTable = BuildTimesTable(tracksPerSec, tracksPerMin);

            var requiredWidth = statsTable.Max(l => l.Length) + TableSpacing + timesTable.Max(l => l.Length);

            if (!LogOnly && Console.WindowWidth >= requiredWidth + 2) {
                // Side-by-side, no padding of shorter table
                sb.AppendLine(CombineTables(statsTable, timesTable, TableSpacing));
            } else {
                // Stacked layout (or log-only mode)
                foreach (var line in statsTable) sb.AppendLine(line);
                sb.AppendLine();
                foreach (var line in timesTable) sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public void PrintReport() {
            var report = BuildReport();

            if (!LogOnly) {
                Console.WriteLine(report);
            }

            Log.Info(report);
        }
    }
}
