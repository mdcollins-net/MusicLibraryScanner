// Data/PostgresConnectionFactory.cs
using System.Data;
using Npgsql;

namespace MusicLibraryScanner.Data
{
    public class PostgresConnectionFactory
    {
        private readonly string _connectionString;

        public PostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}