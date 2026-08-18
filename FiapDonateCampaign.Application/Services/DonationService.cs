using FiapDonateCampaign.Application.DTOs;
using FiapDonateCampaign.Application.Event;
using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Exceptions;
using FiapDonateCampaign.Domain.Interfaces;

namespace FiapDonateCampaign.Application.Services;

public class DonationService : IDonationService
{
    private readonly IDonationRepository _doacaoRepository;
    private readonly ICampaignRepository _campanhaRepository;
    private readonly IEventPublisher _eventPublisher;

    public DonationService(
        IDonationRepository doacaoRepository,
        ICampaignRepository campanhaRepository,
        IEventPublisher eventPublisher)
    {
        _doacaoRepository = doacaoRepository;
        _campanhaRepository = campanhaRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<IntentionDonateResponseDto> RegistrarIntencaoAsync(string doadorId, IntentionDonateRequestDto dto)
    {
        var campanha = await _campanhaRepository.ObterPorIdAsync(dto.IdCampanha)
            ?? throw new DomainException("Campanha não encontrada.");

        campanha.ValidarPodeReceberDoacao(); // lança DomainException se encerrada/cancelada

        var doacao = new Donation(dto.IdCampanha, doadorId, dto.ValorDoacao);
        await _doacaoRepository.AdicionarAsync(doacao);

        var evento = new DonationReceivedEvent(doacao.Id, doacao.CampanhaId, doacao.DoadorId, doacao.ValorDoacao, doacao.DataDoacao);
        await _eventPublisher.PublishAsync(evento, "campanha.doacao.recebida");

        return new IntentionDonateResponseDto(doacao.Id, doacao.CampanhaId, doacao.ValorDoacao, doacao.DataDoacao);
    }
}