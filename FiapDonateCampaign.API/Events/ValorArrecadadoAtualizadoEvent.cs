// Mesmo motivo do namespace "estrangeiro" em
// FiapDonateCampaign.Application/Event/DoacaoRecebidaEvent.cs: este tipo
// precisa ter namespace + nome idênticos ao publicado pelo Worker
// (FiapDonateWorker.Api.Events.ValorArrecadadoAtualizadoEvent, em
// FiapDonateWorker/src/FiapDonateWorker.Api/Events/ValorArrecadadoAtualizadoEvent.cs)
// para que o MassTransit roteie a mensagem até este consumer.
namespace FiapDonateWorker.Api.Events;

/// <summary>
/// Evento publicado pelo Worker após creditar uma doação, devolvendo o valor total
/// arrecadado ATUAL (absoluto, não um delta) da campanha. Consumido por
/// ValorArrecadadoAtualizadoConsumer para atualizar Campaigns.ValorArrecadado.
/// </summary>
public record ValorArrecadadoAtualizadoEvent(
    Guid IdCampanha,
    decimal ValorArrecadado,
    DateTimeOffset AtualizadoEm);
