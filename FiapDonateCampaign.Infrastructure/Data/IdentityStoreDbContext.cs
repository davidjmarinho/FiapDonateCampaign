using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FiapDonateCampaign.Infrastructure.Identity;

namespace FiapDonateCampaign.Infrastructure.Data;

// Usado exclusivamente para o UserManager/SignInManager conseguirem consultar
// a tabela de usuários (criada e migrada pela FiapDonateUsers).
// NUNCA rode Add-Migration usando este DbContext — o schema de Identity
// pertence à FiapDonateUsers.
public class IdentityStoreDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityStoreDbContext(DbContextOptions<IdentityStoreDbContext> options) : base(options) { }
}