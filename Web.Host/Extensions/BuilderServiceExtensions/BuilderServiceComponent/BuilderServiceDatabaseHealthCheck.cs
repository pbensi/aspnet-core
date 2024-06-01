using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Host.Extensions.BuilderServiceExtensions.BuilderServiceComponent
{
    public class BuilderServiceDatabaseHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public BuilderServiceDatabaseHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database check failed: {ex.Message}");
            }
        }
    }
}
