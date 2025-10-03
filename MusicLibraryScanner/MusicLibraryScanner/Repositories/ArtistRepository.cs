// Repositories/ArtistRepository.cs
using System;
using System.Threading.Tasks;
using Dapper;
using MusicLibraryScanner.Data;
using MusicLibraryScanner.Models;

namespace MusicLibraryScanner.Repositories {
    public interface IArtistRepository {
        Task<int> GetOrCreateIdAsync(Artist artist);
    }

    public class ArtistRepository(PostgresConnectionFactory connFactory) : IArtistRepository {
        public async Task<int> GetOrCreateIdAsync(Artist artist) {
            using var conn = connFactory.CreateConnection();

            // Try to find existing artist
            const string selectSql = "SELECT ID FROM Artists WHERE Name = @Name LIMIT 1";
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(selectSql, new { artist.Name });

            if (existing.HasValue) {
                return existing.Value;
            }

            // Insert new artist with extended fields
            const string insertSql = @"
                INSERT INTO Artists (Name, DiscogsArtistId, MusicBrainzArtistId, AudioDbArtistId, Biography)
                VALUES (@Name, @DiscogsArtistId, @MusicBrainzArtistId, @AudioDbArtistId, @Biography)
                RETURNING ID";

            return await conn.ExecuteScalarAsync<int>(insertSql, new {
                artist.Name,
                artist.DiscogsArtistId,
                artist.MusicBrainzArtistId,
                artist.AudioDbArtistId,
                artist.Biography
            });
        }
    }
}