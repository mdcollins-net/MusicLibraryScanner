// Repositories/TrackRepository.cs

using Dapper;
using MusicLibraryScanner.Data;

namespace MusicLibraryScanner.Repositories;

public interface ITrackRepository
{
    Task<int> GetOrCreateIdAsync(int albumId, int artistId, string album, string artist, int year, string title,
        int trackNumber, int duration, DateTime? dateTagged, string musicBrainzTrackId, string musicBrainzReleaseId, string musicBrainzArtistId);
}

public class TrackRepository(PostgresConnectionFactory connFactory) : ITrackRepository
{
    public async Task<int> GetOrCreateIdAsync(int albumId, int artistId, string album, string artist, int year,
        string title, int trackNumber, int duration, DateTime? dateTagged, string musicBrainzTrackId, string musicBrainzReleaseId, string musicBrainzArtistId)
    {
        using var conn = connFactory.CreateConnection();
        const string selectSql = "SELECT ID FROM Tracks WHERE Album = @Album AND Artist = @Artist AND Year = @Year AND Title = @Title AND TrackNumber = @TrackNumber LIMIT 1";
        var existing = await conn.QueryFirstOrDefaultAsync<int?>(selectSql,
            new { Album = album, Artist = artist, Year = year, Title = title, TrackNumber = trackNumber });
        if (existing.HasValue) return existing.Value;

        const string insertSql = @"
                INSERT INTO Tracks (Album, AlbumId, Artist, ArtistId, Year, Title, TrackNumber, Duration, DateTagged, 
                                    MusicBrainzTrackId, MusicBrainzReleaseId, MusicBrainzArtistId)
                VALUES (@Album, @AlbumId, @Artist, @ArtistId, @Year, @Title, @TrackNumber, @Duration, @DateTagged, 
                        @MusicBrainzTrackId, @MusicBrainzReleaseId, @MusicBrainzArtistId)
                RETURNING ID";
        return await conn.ExecuteScalarAsync<int>(insertSql,
            new
            {
                Album = album, AlbumId = albumId, Artist = artist, ArtistId = artistId, Year = year, Title = title,
                TrackNumber = trackNumber, Duration = duration, DateTagged = dateTagged, MusicBrainzTrackId = musicBrainzTrackId,
                MusicBrainzReleaseId = musicBrainzReleaseId, MusicBrainzArtistId = musicBrainzArtistId
            });
    }
}