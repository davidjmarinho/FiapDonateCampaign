
using FiapDonateCampaign.Application.DTOs;
using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Exceptions;
using FiapDonateCampaign.Domain.Interfaces;

namespace FiapDonateCampaign.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _repository;
        public CampaignService(ICampaignRepository repository) => _repository = repository;

        public async Task<CampaignResponseDto> CriarAsync(CampaignRequestDto dto)
        {
            var campaign = new Campaign(dto.Titulo, dto.Descricao, dto.DataInicio, dto.DataFim, dto.MetaFinanceira);
            await _repository.AdicionarAsync(campaign);
            return ToDto(campaign);
        }

        public async Task<CampaignResponseDto> AtualizarAsync(Guid id, CampaignRequestDto dto)
        {
            var campaign = await _repository.ObterPorIdAsync(id)
                ?? throw new DomainException("Campanha não encontrada.");

            campaign.Atualizar(dto.Titulo, dto.Descricao, dto.DataInicio, dto.DataFim, dto.MetaFinanceira, campaign.Status);
            await _repository.AtualizarAsync(campaign);
            return ToDto(campaign);
        }

        public async Task<CampaignResponseDto?> ObterPorIdAsync(Guid id)
        {
            var campaign = await _repository.ObterPorIdAsync(id);
            return campaign is null ? null : ToDto(campaign);
        }

        private static CampaignResponseDto ToDto(Campaign c) =>
            new(c.Id, c.Titulo, c.Descricao, c.DataInicio, c.DataFim, c.MetaFinanceira, c.Status);
    }
}
