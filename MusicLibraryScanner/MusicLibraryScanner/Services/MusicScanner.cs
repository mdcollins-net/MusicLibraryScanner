// Services/MusicScanner.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MusicLibraryScanner.Helpers;
using MusicLibraryScanner.Repositories;
using TagLib;

namespace MusicLibraryScanner.Services
{
    public class MusicScanner(IArtistRepository artistRepo, IAlbumRepository albumRepo, ITrackRepository trackRepo)
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MusicScanner));

        private const int MaxConcurrentAlbums = 4;
        private const int MaxConcurrentTracks = 8;

        // Caches to minimize DB roundtrips
        private readonly ConcurrentDictionary<string, int> _artistCache = new();
        private readonly ConcurrentDictionary<(int artistId, string albumTitle), int> _albumCache = new();

        public async Task ScanAsync(string rootPath)
        {
            Log.Info($"Starting scan of music library at '{rootPath}'");

            if (!Directory.Exists(rootPath))
            {
                Log.Error($"Root path '{rootPath}' does not exist.");
                return;
            }

            var artistDirs = Directory.GetDirectories(rootPath);

            foreach (var artistDir in artistDirs)
            {
                var artistName = ParsingHelpers.ParseArtistName(artistDir);
                int artistId = await GetOrCreateArtistIdAsync(artistName);

                Log.Info($"Processing artist: {artistName} (ID={artistId})");

                var albumDirs = Directory.GetDirectories(artistDir);

                using var albumSemaphore = new SemaphoreSlim(MaxConcurrentAlbums);

                var albumTasks = albumDirs.Select(albumDir => ProcessAlbumWithSemaphoreAsync(albumDir, artistId, artistName, albumSemaphore));
                await Task.WhenAll(albumTasks);
            }

            Log.Info("Scan complete.");
        }

        private async Task<int> GetOrCreateArtistIdAsync(string artistName)
        {
            return _artistCache.GetOrAdd(artistName, _ => artistRepo.GetOrCreateIdAsync(artistName).Result);
        }

        private async Task<int> GetOrCreateAlbumIdAsync(int artistId, string albumTitle, int year)
        {
            var key = (artistId, albumTitle);
            return _albumCache.GetOrAdd(key, _ => albumRepo.GetOrCreateIdAsync(artistId, year, albumTitle).Result);
        }

        private async Task ProcessAlbumWithSemaphoreAsync(string albumDir, int artistId, string artistName, SemaphoreSlim albumSemaphore)
        {
            await albumSemaphore.WaitAsync();
            try
            {
                await ProcessAlbumAsync(albumDir, artistId, artistName);
            }
            finally
            {
                albumSemaphore.Release();
            }
        }

        private async Task ProcessAlbumAsync(string albumDir, int artistId, string artistName)
        {
            var (year, albumTitle) = ParsingHelpers.ParseAlbumFolder(albumDir);
            var albumId = await GetOrCreateAlbumIdAsync(artistId, albumTitle, year);

            Log.Info($"  Processing album: {year} - {albumTitle} (ID={albumId})");

            var trackFiles = Directory.GetFiles(albumDir, "*.flac");

            using var trackSemaphore = new SemaphoreSlim(MaxConcurrentTracks);

            var trackTasks = trackFiles.Select(trackFile => ProcessTrackWithSemaphoreAsync(trackFile, artistId, albumId, artistName, albumTitle, year, trackSemaphore));
            await Task.WhenAll(trackTasks);
        }

        private async Task ProcessTrackWithSemaphoreAsync(string trackFile, int artistId, int albumId, string defaultArtist, string defaultAlbum, int defaultYear, SemaphoreSlim trackSemaphore)
        {
            await trackSemaphore.WaitAsync();
            try
            {
                await ProcessTrackAsync(trackFile, artistId, albumId, defaultArtist, defaultAlbum, defaultYear);
            }
            finally
            {
                trackSemaphore.Release();
            }
        }

        private async Task ProcessTrackAsync(string trackFile, int artistId, int albumId, string defaultArtist, string defaultAlbum, int defaultYear)
        {
            try
            {
                var (trackNumber, parsedTrackTitle) = ParsingHelpers.ParseTrackFile(trackFile);

                using var file = TagLib.File.Create(trackFile);
                var duration = (int)file.Properties.Duration.TotalSeconds;
                var tag = file.Tag;
                var trackTitle = tag.Title ?? parsedTrackTitle;
                var trackArtist = tag.FirstAlbumArtist ?? defaultArtist;
                var trackAlbum = tag.Album ?? defaultAlbum;
                var trackYear = tag.Year > 0 ? (int)tag.Year : defaultYear;
                var trackDateTagged = tag.DateTagged;
                var trackMusicBrainzTrackId = tag.MusicBrainzTrackId;
                var trackMusicBrainzReleaseId = tag.MusicBrainzReleaseId;
                var trackMusicBrainzArtistId = tag.MusicBrainzArtistId;


                var trackId = await trackRepo.GetOrCreateIdAsync(
                    albumId,
                    artistId,
                    trackAlbum,
                    trackArtist,
                    trackYear,
                    trackTitle,
                    trackNumber,
                    duration,
                    trackDateTagged,
                    trackMusicBrainzTrackId,
                    trackMusicBrainzReleaseId,
                    trackMusicBrainzArtistId
                );

                var t = trackId;
                var t2 = trackId;

                Log.Info($"    Track added: {trackNumber:00} - {trackTitle} (ID={trackId}, {duration}s)");
            }
            catch (Exception ex)
            {
                Log.Error($"    Failed to process track '{trackFile}': {ex.Message}", ex);
            }
        }
    }
}
