using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Interfaces;
using FiapDonateCampaign.Infrastructure.Data;

namespace FiapDonateCampaign.Infrastructure.Repositories
{
    public class DonationRepository : IDonationRepository
    {
        private readonly AppDbContext _context;
        public DonationRepository(AppDbContext context) => _context = context;

        public async Task AdicionarAsync(Donation doacao)
        {
            await _context.Donation.AddAsync(doacao);
            await _context.SaveChangesAsync();
        }
    }
}
