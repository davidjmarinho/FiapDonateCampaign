using FiapDonateCampaign.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FiapDonateCampaign.Application.DTOs;

namespace FiapDonateCampaign.API.Controllers;

[ApiController]
[Route("api/doacoes")]
[Authorize(Roles = "Doador")] // classe inteira restrita a Doador
public class DoacoesController : ControllerBase
{
    private readonly IDonationService _service;
    public DoacoesController(IDonationService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> RegistrarIntencao(IntentionDonateRequestDto dto)
    {
        var doadorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var resultado = await _service.RegistrarIntencaoAsync(doadorId, dto);
        return CreatedAtAction(nameof(RegistrarIntencao), new { id = resultado.Id }, resultado);
    }
}