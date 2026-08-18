using FiapDonateCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FiapDonateCampaign.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Donation> Donation => Set<Donation>();

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
                entity.Property(c => c.ValorArrecadado).HasColumnType("decimal(18,2)");
                entity.Property(c => c.Status).HasConversion<string>(); // salva o enum como texto (Ativa/Concluida/Cancelada).
            });

            builder.Entity<Donation>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.ValorDoacao).HasColumnType("decimal(18,2)");
                entity.Property(d => d.DoadorId).IsRequired();
                entity.HasIndex(d => d.CampanhaId); // acelera consultas futuras tipo "doações de uma campanha"
            });
        }
    }
}
