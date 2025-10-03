// Repositories/AlbumRepository.cs

using System.Threading.Tasks;
using Dapper;
using MusicLibraryScanner.Data;

namespace MusicLibraryScanner.Repositories {
    public interface IAlbumRepository {
        Task<int> GetOrCreateIdAsync(int artistId, int? year, string title, int? discogsReleaseId = null);
    }

    public class AlbumRepository(PostgresConnectionFactory connFactory) : IAlbumRepository {
        public async Task<int> GetOrCreateIdAsync(int artistId, int? year, string title, int? discogsReleaseId = null) {
            using var conn = connFactory.CreateConnection();
            const string selectSql =
                "SELECT ID FROM Albums WHERE ArtistId = @ArtistId AND Year = @Year AND Title = @Title LIMIT 1";
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(selectSql,
                new { ArtistId = artistId, Year = year, Title = title });
            if (existing.HasValue) return existing.Value;

            const string insertSql =
                "INSERT INTO Albums (ArtistId, Year, Title, DiscogsReleaseId) " +
                "VALUES (@ArtistId, @Year, @Title, @DiscogsReleaseId) RETURNING ID";


            return await conn.ExecuteScalarAsync<int>(insertSql,
                new { ArtistId = artistId, Year = year, Title = title, DiscogsReleaseId = discogsReleaseId });
        }
    }
}