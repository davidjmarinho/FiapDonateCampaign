namespace FiapDonateCampaign.Application.DTOs
{
    public record IntentionDonateResponseDto(Guid Id, Guid CampanhaId, decimal ValorDoacao, DateTime DataDoacao);
}
