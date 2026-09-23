using FiapDonateCampaign.Domain.Enums;

namespace FiapDonateCampaign.Application.DTOs
{
    public record CampaignResponseDto(
    Guid Id,
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal MetaFinanceira,
    decimal ValorArrecadado,
    StatusCampaign Status);

}
