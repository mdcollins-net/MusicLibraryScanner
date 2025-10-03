using System.Collections.Concurrent;
using log4net;
using MusicLibraryScanner.Helpers;
using MusicLibraryScanner.Models;
using MusicLibraryScanner.Repositories;

namespace MusicLibraryScanner.Services {
    public class MusicScanner(
        IArtistRepository artistRepo,
        IAlbumRepository albumRepo,
        ITrackRepository trackRepo,
        DiscogsApiClient discogsClient
    ) {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MusicScanner));

        private const int MaxConcurrentAlbums = 4;
        private const int MaxConcurrentTracks = 8;

        // ✅ store injected dependencies in fields
        private readonly IArtistRepository _artistRepo = artistRepo;
        private readonly IAlbumRepository _albumRepo = albumRepo;
        private readonly ITrackRepository _trackRepo = trackRepo;
        private readonly DiscogsApiClient _discogsClient = discogsClient;

        // Caches to minimize DB roundtrips
        private readonly ConcurrentDictionary<string, int> _artistCache = new();
        private readonly ConcurrentDictionary<(int artistId, string albumTitle), int> _albumCache = new();

        public async Task ScanAsync(string rootPath) {
            Log.Info($"Starting scan of music library at '{rootPath}'");

            if (!Directory.Exists(rootPath)) {
                Log.Error($"Root path '{rootPath}' does not exist.");
                return;
            }

            var stats = new ProcessingStats();
            stats.Start();

            try {
                var artistDirs = Directory.GetDirectories(rootPath);

                foreach (var artistDir in artistDirs) {
                    var artist = await LoadArtistFromDirectoryAsync(artistDir);
                    var artistId = await GetOrCreateArtistIdAsync(artist);

                    Log.Debug($"Processing artist: {artist.Name} (ID={artistId})");

                    var albumDirs = Directory.GetDirectories(artistDir);

                    using var albumSemaphore = new SemaphoreSlim(MaxConcurrentAlbums);

                    var albumTasks = albumDirs.Select(albumDir =>
                        ProcessAlbumWithSemaphoreAsync(albumDir, artistId, artist.Name, albumSemaphore, stats));
                    await Task.WhenAll(albumTasks);

                    stats.IncrementArtist();
                }

                Log.Info("Scan complete.");
            }
            finally {
                stats.Stop();
                stats.PrintReport();
            }
        }

        private static async Task<Artist> LoadArtistFromDirectoryAsync(string artistDir) {
            var artistName = ParsingHelpers.ParseArtistName(artistDir);
            var nfoPath = Path.Combine(artistDir, "artist.nfo");

            if (!File.Exists(nfoPath)) return new Artist { Name = artistName };
            try {
                var parsed = ArtistNfoParser.Parse(nfoPath);
                if (parsed != null) {
                    Log.Debug($"Loaded artist metadata from NFO for '{artistName}'");
                    return parsed;
                }
            }
            catch (Exception ex) {
                Log.Warn($"Failed to parse artist.nfo for '{artistName}': {ex.Message}");
            }

            return new Artist { Name = artistName };
        }

        private async Task<int> GetOrCreateArtistIdAsync(Artist artist) {
            return _artistCache.GetOrAdd(
                artist.Name,
                _ => _artistRepo.GetOrCreateIdAsync(artist).Result
            );
        }

        private async Task<int> GetOrCreateAlbumIdAsync(Album album) {
            var key = (album.ArtistID, album.Title);
            return _albumCache.GetOrAdd(
                key,
                _ => _albumRepo.GetOrCreateIdAsync(album.ArtistID, album.Year, album.Title, album.DiscogsReleaseId)
                    .Result
            );
        }

        private async Task ProcessAlbumWithSemaphoreAsync(string albumDir, int artistId, string artistName,
            SemaphoreSlim albumSemaphore, ProcessingStats stats) {
            await albumSemaphore.WaitAsync();
            try {
                await ProcessAlbumAsync(albumDir, artistId, artistName, stats);
            }
            finally {
                albumSemaphore.Release();
            }
        }

        private async Task ProcessAlbumAsync(string albumDir, int artistId, string artistName, ProcessingStats stats) {
            var (year, albumTitle) = ParsingHelpers.ParseAlbumFolder(albumDir);
            var nfoPath = Path.Combine(albumDir, "album.nfo");

            Album album;
            if (File.Exists(nfoPath)) {
                var parsed = AlbumNfoParser.Parse(nfoPath, artistId);
                album = parsed ?? new Album { ArtistID = artistId, Title = albumTitle, Year = year };

                if (parsed != null)
                    Log.Debug($"Loaded album metadata from NFO for '{albumTitle}'");
            }
            else {
                album = new Album { ArtistID = artistId, Title = albumTitle, Year = year };
            }

            if (album.DiscogsReleaseId == null) {
                try {
                    var releaseId = await _discogsClient.FindReleaseIdAsync(artistName, album.Title, album.Year);
                    if (releaseId.HasValue) {
                        album.DiscogsReleaseId = releaseId;
                        Log.Info(
                            $"Looked up DiscogsReleaseId={releaseId} for album '{album.Title}' ({album.Year}) by {artistName}");
                    }
                }
                catch (Exception ex) {
                    Log.Warn($"Discogs lookup failed for '{album.Title}' ({album.Year}): {ex.Message}");
                }
            }

            var albumId = await GetOrCreateAlbumIdAsync(album);

            Log.Debug($"  Processing album: {album.Year} - {album.Title} (ID={albumId})");

            var trackFiles = Directory.GetFiles(albumDir, "*.flac");

            using var trackSemaphore = new SemaphoreSlim(MaxConcurrentTracks);
            var trackTasks = trackFiles.Select(trackFile =>
                ProcessTrackWithSemaphoreAsync(trackFile, artistId, albumId,
                    artistName, album.Title, album.Year ?? 0, trackSemaphore, stats));

            await Task.WhenAll(trackTasks);

            stats.IncrementAlbum();
        }

        private async Task ProcessTrackWithSemaphoreAsync(string trackFile, int artistId, int albumId,
            string defaultArtist, string defaultAlbum, int defaultYear, SemaphoreSlim trackSemaphore,
            ProcessingStats stats) {
            await trackSemaphore.WaitAsync();
            try {
                await ProcessTrackAsync(trackFile, artistId, albumId, defaultArtist, defaultAlbum, defaultYear, stats);
            }
            finally {
                trackSemaphore.Release();
            }
        }

        private async Task ProcessTrackAsync(string trackFile, int artistId, int albumId, string defaultArtist,
            string defaultAlbum, int defaultYear, ProcessingStats stats) {
            try {
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

                var trackId = await _trackRepo.GetOrCreateIdAsync(
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

                stats.IncrementTrack();

                Log.Debug($"    Track added: {trackNumber:00} - {trackTitle} (ID={trackId}, {duration}s)");
            }
            catch (Exception ex) {
                Log.Error($"    Failed to process track '{trackFile}': {ex.Message}", ex);
            }
        }
    }
}