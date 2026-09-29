using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace FiapDonateCampaign.API.HealthChecks;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RabbitMqHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMq:Host"] ?? "localhost",
                VirtualHost = _configuration["RabbitMq:VirtualHost"] ?? "/",
                UserName = _configuration["RabbitMq:Username"] ?? "guest",
                Password = _configuration["RabbitMq:Password"] ?? "guest",
                AutomaticRecoveryEnabled = false,
                TopologyRecoveryEnabled = false
            };

            return CheckConnectionAsync(factory, cancellationToken);
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Falha ao conectar no RabbitMQ.", exception));
        }
    }

    private static async Task<HealthCheckResult> CheckConnectionAsync(ConnectionFactory factory, CancellationToken cancellationToken)
    {
        using var connection = await factory.CreateConnectionAsync(cancellationToken);
        return connection is not null
            ? HealthCheckResult.Healthy("RabbitMQ disponível.")
            : HealthCheckResult.Unhealthy("RabbitMQ indisponível.");
    }
}