// Repositories/ArtistRepository.cs
using System.Threading.Tasks;
using Dapper;
using MusicLibraryScanner.Data;

namespace MusicLibraryScanner.Repositories
{
    public interface IArtistRepository
    {
        Task<int> GetOrCreateIdAsync(string name);
    }

    public class ArtistRepository(PostgresConnectionFactory connFactory) : IArtistRepository
    {
        public async Task<int> GetOrCreateIdAsync(string name)
        {
            using var conn = connFactory.CreateConnection();
            const string selectSql = "SELECT ID FROM Artists WHERE Name = @Name LIMIT 1";
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(selectSql, new { Name = name });
            if (existing.HasValue) return existing.Value;

            const string insertSql = "INSERT INTO Artists (Name) VALUES (@Name) RETURNING ID";
            return await conn.ExecuteScalarAsync<int>(insertSql, new { Name = name });
        }
    }
}