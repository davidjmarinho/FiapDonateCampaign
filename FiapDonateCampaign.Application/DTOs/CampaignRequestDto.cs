namespace FiapDonateCampaign.Application.DTOs
{
    public record CampaignRequestDto(
        string Titulo,
        string Descricao,
        DateTime DataInicio,
        DateTime DataFim,
        decimal MetaFinanceira
    );
}
