using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Enums;
using FiapDonateCampaign.Domain.Exceptions;

namespace FiapDonateCampaign.UnitTests;

public class CampaignTests
{
    private static Campaign CriarCampanhaAtiva(decimal metaFinanceira = 1000m) =>
        new("Campanha de teste", "Descricao", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), metaFinanceira);

    [Fact]
    public void DefinirValorArrecadado_AbaixoDaMeta_AtualizaValorEMantemAtiva()
    {
        var campanha = CriarCampanhaAtiva(metaFinanceira: 1000m);

        campanha.DefinirValorArrecadado(400m);

        Assert.Equal(400m, campanha.ValorArrecadado);
        Assert.Equal(StatusCampaign.Ativa, campanha.Status);
    }

    [Fact]
    public void DefinirValorArrecadado_AtingeMeta_ConcluiCampanha()
    {
        var campanha = CriarCampanhaAtiva(metaFinanceira: 1000m);

        campanha.DefinirValorArrecadado(1000m);

        Assert.Equal(1000m, campanha.ValorArrecadado);
        Assert.Equal(StatusCampaign.Concluida, campanha.Status);
    }

    [Fact]
    public void DefinirValorArrecadado_ValorNegativo_LancaDomainException()
    {
        var campanha = CriarCampanhaAtiva();

        Assert.Throws<DomainException>(() => campanha.DefinirValorArrecadado(-1m));
    }
}
