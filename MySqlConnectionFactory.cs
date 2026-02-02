using janaez.webapi.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;

namespace janaez.webapi
{
    public interface IDbConnectionFactory
    {
        public IDbConnection CreateConnection();

        Task<IDbConnection> CreateConnectionAsync();
    }

    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly DatabaseSettings settings;
        private readonly string _connectionString;

        public MySqlConnectionFactory(IOptions<DatabaseSettings> _settings)
        {
            this.settings = _settings.Value;
            _connectionString = settings.MySqlConnectionStrings;
        }
        public IDbConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            //connection.Open();
            return connection;
        }

        public async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
