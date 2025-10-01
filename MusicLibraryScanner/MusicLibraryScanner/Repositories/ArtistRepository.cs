// Repositories/ArtistRepository.cs
using System.Threading.Tasks;
using Dapper;
using MusicLibraryScanner.Data;

namespace MusicLibraryScanner.Repositories
{
    public interface IArtistRepository
    {
        Task<int> GetOrCreateArtistIdAsync(string name);
    }

    public class ArtistRepository : IArtistRepository
    {
        private readonly PostgresConnectionFactory _connFactory;

        public ArtistRepository(PostgresConnectionFactory connFactory)
        {
            _connFactory = connFactory;
        }

        public async Task<int> GetOrCreateArtistIdAsync(string name)
        {
            using var conn = _connFactory.CreateConnection();
            const string selectSql = "SELECT ID FROM Artists WHERE Name = @Name LIMIT 1";
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(selectSql, new { Name = name });
            if (existing.HasValue) return existing.Value;

            const string insertSql = "INSERT INTO Artists (Name) VALUES (@Name) RETURNING ID";
            return await conn.ExecuteScalarAsync<int>(insertSql, new { Name = name });
        }
    }
}