using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Enums;
using FiapDonateCampaign.Domain.Interfaces;
using FiapDonateCampaign.Infrastructure.Data;
using FiapDonateCampaign.Infrastructure.Repositories;
using FiapDonateCampaign.API.Consumers;
using FiapDonateWorker.Api.Events;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FiapDonateCampaign.IntegrationTests;

public class ValorArrecadadoAtualizadoConsumerTests
{
    private static Campaign CriarCampanhaAtiva(decimal metaFinanceira = 1000m) =>
        new("Campanha de teste", "Descricao", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), metaFinanceira);

    [Fact]
    public async Task Consume_CampanhaExistente_AtualizaValorArrecadado()
    {
        var nomeBanco = Guid.NewGuid().ToString();
        var campanha = CriarCampanhaAtiva(metaFinanceira: 1000m);

        await using (var contexto = CriarContexto(nomeBanco))
        {
            contexto.Campaigns.Add(campanha);
            await contexto.SaveChangesAsync();
        }

        await using var provider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(nomeBanco))
            .AddScoped<ICampaignRepository, CampaignRepository>()
            .AddLogging()
            .AddMassTransitTestHarness(x => x.AddConsumer<ValorArrecadadoAtualizadoConsumer>())
            .BuildServiceProvider(validateScopes: true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            await harness.Bus.Publish(new ValorArrecadadoAtualizadoEvent(
                IdCampanha: campanha.Id,
                ValorArrecadado: 350m,
                AtualizadoEm: DateTimeOffset.UtcNow));

            Assert.True(await harness.Consumed.Any<ValorArrecadadoAtualizadoEvent>());
        }
        finally
        {
            await harness.Stop();
        }

        await using (var contexto = CriarContexto(nomeBanco))
        {
            var atualizada = await contexto.Campaigns.SingleAsync(c => c.Id == campanha.Id);
            Assert.Equal(350m, atualizada.ValorArrecadado);
            Assert.Equal(StatusCampaign.Ativa, atualizada.Status);
        }
    }

    [Fact]
    public async Task Consume_ValorAtingeMeta_ConcluiCampanha()
    {
        var nomeBanco = Guid.NewGuid().ToString();
        var campanha = CriarCampanhaAtiva(metaFinanceira: 1000m);

        await using (var contexto = CriarContexto(nomeBanco))
        {
            contexto.Campaigns.Add(campanha);
            await contexto.SaveChangesAsync();
        }

        await using var provider = new ServiceCollection()
            .AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(nomeBanco))
            .AddScoped<ICampaignRepository, CampaignRepository>()
            .AddLogging()
            .AddMassTransitTestHarness(x => x.AddConsumer<ValorArrecadadoAtualizadoConsumer>())
            .BuildServiceProvider(validateScopes: true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            await harness.Bus.Publish(new ValorArrecadadoAtualizadoEvent(
                IdCampanha: campanha.Id,
                ValorArrecadado: 1000m,
                AtualizadoEm: DateTimeOffset.UtcNow));

            Assert.True(await harness.Consumed.Any<ValorArrecadadoAtualizadoEvent>());
        }
        finally
        {
            await harness.Stop();
        }

        await using (var contexto = CriarContexto(nomeBanco))
        {
            var atualizada = await contexto.Campaigns.SingleAsync(c => c.Id == campanha.Id);
            Assert.Equal(StatusCampaign.Concluida, atualizada.Status);
        }
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }
}
