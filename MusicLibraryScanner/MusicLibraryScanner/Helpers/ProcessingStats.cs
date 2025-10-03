using System;
using System.Diagnostics;
using System.Text;
using log4net;

namespace MusicLibraryScanner.Helpers
{
    public class ProcessingStats
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessingStats));
        private readonly Stopwatch _stopwatch = new();

        private DateTime _startTime;
        private DateTime _endTime;

        public int TrackCount { get; private set; }
        public int AlbumCount { get; private set; }
        public int ArtistCount { get; private set; }

        public void Start()
        {
            _startTime = DateTime.Now;
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
            _endTime = DateTime.Now;
        }

        public void IncrementTrack() => TrackCount++;
        public void IncrementAlbum() => AlbumCount++;
        public void IncrementArtist() => ArtistCount++;

        private string GetDuration()
        {
            var ts = _stopwatch.Elapsed;
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s";
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }

        private string BuildReport()
        {
            var ts = _stopwatch.Elapsed;
            double tracksPerSec = ts.TotalSeconds > 0 ? TrackCount / ts.TotalSeconds : 0;
            double tracksPerMin = ts.TotalMinutes > 0 ? TrackCount / ts.TotalMinutes : 0;

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("====================================");
            sb.AppendLine("  🎵 Music Library Scan Completed 🎵");
            sb.AppendLine("====================================");

            sb.AppendLine("+-------------+---------------------+");
            sb.AppendLine("| Statistics  |              Counts |");
            sb.AppendLine("+-------------+---------------------+");
            sb.AppendLine($"| Artists     | {ArtistCount,19} |");
            sb.AppendLine($"| Albums      | {AlbumCount,19} |");
            sb.AppendLine($"| Tracks      | {TrackCount,19} |");
            sb.AppendLine("+-------------+---------------------+");

            sb.AppendLine("+-------------------+---------------------+");
            sb.AppendLine("| Times / Speeds                          |");
            sb.AppendLine("+-------------------+---------------------+");
            sb.AppendLine($"| Start Time        | {_startTime:yyyy-MM-dd HH:mm:ss} |");
            sb.AppendLine($"| End Time          | {_endTime:yyyy-MM-dd HH:mm:ss} |");
            sb.AppendLine($"| Duration          | {GetDuration(),19} |");
            sb.AppendLine("+-------------------+---------------------+");
            sb.AppendLine($"| Tracks per second | {tracksPerSec,19:F2} |");
            sb.AppendLine($"| Tracks per minute | {tracksPerMin,19:F2} |");
            sb.AppendLine("+-------------------+---------------------+");

            sb.AppendLine("====================================");
            return sb.ToString();
        }

        public void PrintReport()
        {
            var report = BuildReport();

            // Console: clean table (no prefixes)
            Console.WriteLine(report);

            // File log only: write at INFO level
            // Use the root logger directly (still goes to file, console suppressed if ConsoleAppender threshold > INFO)
            Log.Info(report);
        }
    }
}
