using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FiapDonateCampaign.Infrastructure.Auth;
using FiapDonateCampaign.Infrastructure.Identity;

namespace FiapDonateCampaign.API.Controllers;

public record LoginDto(string Email, string Senha);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);
        if (usuario is null || !await _userManager.CheckPasswordAsync(usuario, dto.Senha))
            return Unauthorized("Email ou senha inválidos.");

        var roles = await _userManager.GetRolesAsync(usuario);
        var token = _tokenService.GerarToken(usuario, roles);

        return Ok(new { token });
    }
}