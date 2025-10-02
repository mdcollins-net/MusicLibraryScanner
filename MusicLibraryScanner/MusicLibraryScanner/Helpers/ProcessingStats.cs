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

        public int TrackCount { get; private set; }
        public int AlbumCount { get; private set; }
        public int ArtistCount { get; private set; }

        public void Start() => _stopwatch.Start();
        public void Stop() => _stopwatch.Stop();

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
            sb.AppendLine("+-------------+-----------+");
            sb.AppendLine("| Category    | Value     |");
            sb.AppendLine("+-------------+-----------+");
            sb.AppendLine($"| Artists     | {ArtistCount,9} |");
            sb.AppendLine($"| Albums      | {AlbumCount,9} |");
            sb.AppendLine($"| Tracks      | {TrackCount,9} |");
            sb.AppendLine($"| Duration    | {GetDuration(),9} |");
            sb.AppendLine($"| Speed (s)   | {tracksPerSec,9:F2} |");
            sb.AppendLine($"| Speed (min) | {tracksPerMin,9:F2} |");
            sb.AppendLine("+-------------+-----------+");
            return sb.ToString();
        }

        public void PrintReport()
        {
            var report = BuildReport();

            // Console
            Console.WriteLine(report);

            // Log
            //Log.Info(report);
        }
    }
}
