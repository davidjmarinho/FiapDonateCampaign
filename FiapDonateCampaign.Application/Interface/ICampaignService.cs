using FiapDonateCampaign.Application.DTOs;

namespace FiapDonateCampaign.Application.Interface
{
    public interface ICampaignService
    {
        Task<CampaignResponseDto> CriarAsync(CampaignRequestDto dto);
        Task<CampaignResponseDto> AtualizarAsync(Guid id, CampaignRequestDto dto);
        Task<CampaignResponseDto?> ObterPorIdAsync(Guid id);
        Task<IEnumerable<CampaignListDto>> ListarAtivasAsync();
    }
}
