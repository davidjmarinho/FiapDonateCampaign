using FiapDonateCampaign.Domain.Entities;

namespace FiapDonateCampaign.Domain.Interfaces;

public interface ICampaignRepository
{
    Task AdicionarAsync(Campaign campaign);
    Task AtualizarAsync(Campaign campaign);
    Task<Campaign?> ObterPorIdAsync(Guid id);
}