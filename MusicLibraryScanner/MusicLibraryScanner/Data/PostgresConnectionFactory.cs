// Data/PostgresConnectionFactory.cs

using System.Data;
using Npgsql;

namespace MusicLibraryScanner.Data {
    public class PostgresConnectionFactory(string connectionString) {
        public IDbConnection CreateConnection() {
            var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}