using FiapDonateCampaign.Application.DTOs;

namespace FiapDonateCampaign.Application.Interface
{
    public interface IDonationService
    {
        Task<IntentionDonateResponseDto> RegistrarIntencaoAsync(string doadorId, IntentionDonateRequestDto dto);
    }
}
