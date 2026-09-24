using FiapDonateCampaign.Domain.Interfaces;
using FiapDonateWorker.Api.Events;
using MassTransit;

namespace FiapDonateCampaign.API.Consumers;

/// <summary>
/// Consome o retorno do Worker após ele creditar uma doação, atualizando
/// Campaigns.ValorArrecadado (fonte da verdade) para que o Painel de
/// Transparência reflita o valor processado.
/// </summary>
public class ValorArrecadadoAtualizadoConsumer : IConsumer<ValorArrecadadoAtualizadoEvent>
{
    private readonly ICampaignRepository _repository;
    private readonly ILogger<ValorArrecadadoAtualizadoConsumer> _logger;

    public ValorArrecadadoAtualizadoConsumer(ICampaignRepository repository, ILogger<ValorArrecadadoAtualizadoConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ValorArrecadadoAtualizadoEvent> context)
    {
        var evento = context.Message;

        var campanha = await _repository.ObterPorIdAsync(evento.IdCampanha);
        if (campanha is null)
        {
            _logger.LogWarning(
                "Campanha {IdCampanha} nao encontrada ao processar ValorArrecadadoAtualizadoEvent.",
                evento.IdCampanha);
            return;
        }

        campanha.DefinirValorArrecadado(evento.ValorArrecadado);
        await _repository.AtualizarAsync(campanha);
    }
}
