using Microsoft.EntityFrameworkCore;
using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Interfaces;
using FiapDonateCampaign.Infrastructure.Data;
using FiapDonateCampaign.Domain.Enums;

namespace FiapDonateCampaign.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly AppDbContext _context;
    public CampaignRepository(AppDbContext context) => _context = context;

    public async Task AdicionarAsync(Campaign campaign)
    {
        await _context.Campaigns.AddAsync(campaign);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
        await _context.SaveChangesAsync();
    }

    public Task<Campaign?> ObterPorIdAsync(Guid id) =>
        _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Campaign>> ObterAtivasAsync() =>
    await _context.Campaigns
        .Where(c => c.Status == StatusCampaign.Ativa)
        .AsNoTracking() // é uma consulta só de leitura, não precisa rastrear as entidades — mais rápido
        .ToListAsync();
}