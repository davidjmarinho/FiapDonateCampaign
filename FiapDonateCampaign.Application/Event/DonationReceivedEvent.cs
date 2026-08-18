namespace FiapDonateCampaign.Application.Event
{
    public record DonationReceivedEvent(
    Guid DoacaoId,
    Guid CampanhaId,
    string DoadorId,
    decimal ValorDoacao,
    DateTime DataDoacao);
}
