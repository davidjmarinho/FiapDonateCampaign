using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Infrastructure.Messaging;
using FiapDonateWorker.Api.Events;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FiapDonateCampaign.IntegrationTests;

public class MassTransitEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_PublicaEventoNoBus()
    {
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddScoped<IEventPublisher, MassTransitEventPublisher>()
            .AddMassTransitTestHarness()
            .BuildServiceProvider(validateScopes: true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            var idCampanha = Guid.NewGuid();
            using var scope = provider.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            await publisher.PublishAsync(
                new DoacaoRecebidaEvent(Guid.NewGuid(), idCampanha, 50m, DateTimeOffset.UtcNow),
                "campanha.doacao.recebida");

            Assert.True(await harness.Published.Any<DoacaoRecebidaEvent>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
