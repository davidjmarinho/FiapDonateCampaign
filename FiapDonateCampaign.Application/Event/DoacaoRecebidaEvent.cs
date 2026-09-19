namespace FiapDonateCampaign.Application.Event;

/// <summary>
/// Contrato de evento para RabbitMQ - Publicado quando uma doação é recebida.
/// 
/// Este record define a estrutura da mensagem que será enviada para a fila "campanha.doacao.recebida"
/// no RabbitMQ. Ao usar record (imutável), garantimos que o evento não poderá ser modificado após criação.
/// 
/// RoutingKey: "campanha.doacao.recebida"
/// Exchange: Será configurado em appsettings.json
/// 
/// Campo DataHoraRecebida usa DateTimeOffset para incluir informações de timezone,
/// essencial para eventos distribuídos em diferentes regiões geográficas.
/// </summary>
public record DoacaoRecebidaEvent(
    Guid DoacaoId,
    Guid IdCampanha,
    decimal ValorDoacao,
    DateTimeOffset DataHoraRecebida)
{
    /// <summary>
    /// Cria uma nova instância de DoacaoRecebidaEvent.
    /// </summary>
    /// <example>
    /// var evento = new DoacaoRecebidaEvent(
    ///     doacaoId: Guid.NewGuid(),
    ///     idCampanha: Guid.NewGuid(),
    ///     valorDoacao: 100.50m,
    ///     dataHoraRecebida: DateTimeOffset.UtcNow
    /// );
    /// 
    /// // Serializado para JSON (automaticamente pelo RabbitMQ):
    /// // {
    /// //   "doacaoId": "...",
    /// //   "idCampanha": "...",
    /// //   "valorDoacao": 100.50,
    /// //   "dataHoraRecebida": "2025-01-15T10:30:00+00:00"
    /// // }
    /// </example>
}
