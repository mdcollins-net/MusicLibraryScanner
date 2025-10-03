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
            return ts.TotalMinutes >= 1 ? $"{(int)ts.TotalMinutes}m {ts.Seconds}s" : $"{ts.Seconds}s";
        }

        private string BuildReport() {
            var ts = _stopwatch.Elapsed;
            var tracksPerSec = ts.TotalSeconds > 0 ? TrackCount / ts.TotalSeconds : 0;
            var tracksPerMin = ts.TotalMinutes > 0 ? TrackCount / ts.TotalMinutes : 0;

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

            sb.AppendLine("+-----------------------------------------+");
            sb.AppendLine("| Speeds / Times                          |");
            sb.AppendLine("+-------------------+---------------------+");
            sb.AppendLine($"| Duration          | {GetDuration(),19} |");
            sb.AppendLine($"| Start Time        | {_startTime:yyyy-MM-dd HH:mm:ss} |");
            sb.AppendLine($"| End Time          | {_endTime:yyyy-MM-dd HH:mm:ss} |");
            sb.AppendLine($"| Tracks per second | {tracksPerSec,19:F2} |");
            sb.AppendLine($"| Tracks per minute | {tracksPerMin,19:F2} |");
            sb.AppendLine("+-------------------+---------------------+");
            sb.AppendLine("====================================");
            return sb.ToString();
        }

        public void PrintReport() {
            var report = BuildReport();

            // Console
            Console.WriteLine(report);

            // Log
            Log.Info(report);
        }
    }
}