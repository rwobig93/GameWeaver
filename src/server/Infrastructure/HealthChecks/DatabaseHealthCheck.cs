using Application.Services.Database;
using Infrastructure.Database.MsSql.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ISqlDataService _database;
    private readonly ILogger _logger;

    public DatabaseHealthCheck(ISqlDataService database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            await _database.LoadData<int, dynamic>(GeneralTableMsSql.VerifyConnectivity, new { }, timeoutSeconds: 2);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Database healthcheck failed, which is REALLY bad!");
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}