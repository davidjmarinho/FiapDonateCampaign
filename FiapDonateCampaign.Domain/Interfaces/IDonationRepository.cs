using FiapDonateCampaign.Domain.Entities;

namespace FiapDonateCampaign.Domain.Interfaces
{
    public interface IDonationRepository
    {
        Task AdicionarAsync(Donation doacao);
    }
}
