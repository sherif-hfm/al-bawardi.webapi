using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;

namespace janaez.webapi
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DatabaseHealthCheck> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new())
        {
            try
            {
                using var connection = await _dbConnectionFactory.CreateConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1 from dual";
                command.ExecuteScalar();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return HealthCheckResult.Unhealthy(exception: ex);
            }
        }
    }
}
