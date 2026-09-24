using FiapDonateCampaign.Application.Interface;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FiapDonateCampaign.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventPublisher> _logger;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint, ILogger<MassTransitEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class
    {
        await _publishEndpoint.Publish(@event);

        _logger.LogInformation(
            "Evento publicado no RabbitMQ. RoutingKey: {RoutingKey} | Tipo: {EventType}",
            routingKey,
            typeof(TEvent).Name);
    }
}
