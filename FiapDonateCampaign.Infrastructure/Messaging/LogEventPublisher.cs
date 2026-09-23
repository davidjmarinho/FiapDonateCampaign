using System.Text.Json;
using Microsoft.Extensions.Logging;
using FiapDonateCampaign.Application.Interface;

namespace FiapDonateCampaign.Infrastructure.Messaging;

// Implementação temporária: apenas loga o evento.
// Quando o RabbitMQ for integrado, criar RabbitMqEventPublisher (implementando a mesma interface)
// e trocar o registro no DependencyInjection.cs — nenhuma outra camada precisa mudar.
public class LogEventPublisher : IEventPublisher
{
    private readonly ILogger<LogEventPublisher> _logger;
    public LogEventPublisher(ILogger<LogEventPublisher> logger) => _logger = logger;

    public Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class
    {
        _logger.LogInformation(
            "[EVENTO SIMULADO] RoutingKey: {RoutingKey} | Payload: {Payload}",
            routingKey,
            JsonSerializer.Serialize(@event));

        return Task.CompletedTask;
    }
}