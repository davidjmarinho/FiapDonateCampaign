using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using FiapDonateCampaign.Application.Interface;

namespace FiapDonateCampaign.Infrastructure.Messaging;

/// <summary>
/// Implementação RabbitMQ do publicador de eventos.
/// 
/// Esta classe será responsável por publicar eventos para o RabbitMQ.
/// Atualmente, é um scaffold/placeholder que será preenchido com a integração real.
/// 
/// Dependencies que serão necessárias:
/// - NuGet: RabbitMQ.Client (https://www.nuget.org/packages/RabbitMQ.Client/)
/// 
/// Configuração necessária em appsettings.json:
/// {
///   "RabbitMQ": {
///     "HostName": "localhost",
///     "Port": 5672,
///     "UserName": "guest",
///     "Password": "guest",
///     "ExchangeName": "donations.exchange",
///     "AutomaticRecovery": true,
///     "ConnectionRetryCount": 3
///   }
/// }
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    // private readonly IConnection _connection;
    // private readonly IModel _channel;
    // private readonly string _exchangeName;

    /// <summary>
    /// Inicializa a instância do publicador RabbitMQ.
    /// </summary>
    /// <remarks>
    /// TODO: Implementar inicialização com RabbitMQ.Client
    /// - Criar ConnectionFactory
    /// - Estabelecer conexão
    /// - Criar channel
    /// - Declarar exchange e filas
    /// </remarks>
    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        // Implementação futura
    }

    /// <summary>
    /// Publica um evento para o RabbitMQ.
    /// </summary>
    /// <typeparam name="TEvent">Tipo do evento (deve ser um record)</typeparam>
    /// <param name="event">Instância do evento a publicar</param>
    /// <param name="routingKey">Routing key para entregar a mensagem à fila correta</param>
    /// <returns>Task completada assincronamente</returns>
    /// 
    /// <remarks>
    /// Fluxo esperado:
    /// 1. Serializar o evento para JSON
    /// 2. Converter para bytes (UTF-8)
    /// 3. Publicar na fila com routing key
    /// 4. Logar sucesso/erro
    /// 5. Implementar retry policy se falhar
    /// 
    /// Exemplo de routing key: "campanha.doacao.recebida"
    /// Este routing key vai direcionar o evento para subscribers apropriados.
    /// </remarks>
    public Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class
    {
        try
        {
            // TODO: Implementar publicação real
            // 1. Serializar evento
            var json = JsonSerializer.Serialize(@event);
            var messageBody = Encoding.UTF8.GetBytes(json);

            // 2. Publicar para exchange
            // _channel.BasicPublish(
            //     exchange: _exchangeName,
            //     routingKey: routingKey,
            //     basicProperties: null,
            //     body: messageBody);

            _logger.LogInformation(
                "[RabbitMQ] Evento publicado com sucesso. RoutingKey: {RoutingKey} | Tipo: {EventType} | Payload: {Payload}",
                routingKey,
                @event.GetType().Name,
                json);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[RabbitMQ] Erro ao publicar evento. RoutingKey: {RoutingKey} | Tipo: {EventType}",
                routingKey,
                @event.GetType().Name);

            // TODO: Implementar circuit breaker ou Dead Letter Queue
            throw;
        }
    }

    /// <summary>
    /// Libera recursos da conexão RabbitMQ.
    /// </summary>
    public void Dispose()
    {
        // TODO: Implementar
        // _channel?.Dispose();
        // _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
