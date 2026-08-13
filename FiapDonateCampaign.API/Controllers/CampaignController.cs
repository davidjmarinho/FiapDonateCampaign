using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FiapDonateCampaign.Application.DTOs;
using FiapDonateCampaign.Application.Interface;

namespace FiapDonateCampaign.API.Controllers;

[ApiController]
[Route("api/campaign")]
[Authorize] // exige token válido em todos os endpoints deste controller, por padrão
public class CampaignController : ControllerBase
{
    private readonly ICampaignService _service;
    public CampaignController(ICampaignService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "GestorONG")] // sobrescreve: só GestorONG pode criar
    public async Task<IActionResult> Criar(CampaignRequestDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "GestorONG")] // só GestorONG pode editar
    public async Task<IActionResult> Editar(Guid id, CampaignRequestDto dto)
    {
        var resultado = await _service.AtualizarAsync(id, dto);
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id) // GestorONG e Doador podem consultar
    {
        var resultado = await _service.ObterPorIdAsync(id);
        return resultado is null ? NotFound() : Ok(resultado);
    }
}