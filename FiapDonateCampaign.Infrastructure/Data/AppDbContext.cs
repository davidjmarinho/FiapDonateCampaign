using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FiapDonateCampaign.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Campaign> Campaigns => Set<Campaign>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // obrigatório: cria as tabelas do Identity (AspNetUsers, AspNetRoles, etc.)
            // Configure the Campaign entity
            builder.Entity<Campaign>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Titulo).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Descricao).IsRequired().HasMaxLength(500);
                entity.Property(c => c.MetaFinanceira).HasColumnType("decimal(18,2)");
                entity.Property(c => c.Status).HasConversion<string>(); // salva o enum como texto (Ativa/Concluida/Cancelada).
            });
        }
    }
}
