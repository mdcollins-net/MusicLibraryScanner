using System;
using System.Diagnostics;

namespace MusicLibraryScanner.Helpers
{
    public class ProcessingStats
    {
        private readonly Stopwatch _stopwatch = new();

        public int TrackCount { get; private set; }
        public int AlbumCount { get; private set; }
        public int ArtistCount { get; private set; }

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();

        public void IncrementTrack() => TrackCount++;
        public void IncrementAlbum() => AlbumCount++;
        public void IncrementArtist() => ArtistCount++;

        public string GetDuration()
        {
            var ts = _stopwatch.Elapsed;
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s";
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
            return $"{ts.Seconds}s";
        }

        public void PrintReport()
        {
            Console.WriteLine();
            Console.WriteLine("Processing Report");
            Console.WriteLine("=================");
            Console.WriteLine($"Artists: {ArtistCount}");
            Console.WriteLine($"Albums : {AlbumCount}");
            Console.WriteLine($"Tracks : {TrackCount}");
            Console.WriteLine($"Time   : {GetDuration()}");
            Console.WriteLine();
        }
    }
}