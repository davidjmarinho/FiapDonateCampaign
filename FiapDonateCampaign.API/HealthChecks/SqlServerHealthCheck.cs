using FiapDonateCampaign.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiapDonateCampaign.API.HealthChecks;

public sealed class SqlServerHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public SqlServerHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("SQL Server disponível.")
                : HealthCheckResult.Unhealthy("SQL Server indisponível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Falha ao conectar no SQL Server.", exception);
        }
    }
}