using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FiapDonateCampaign.Infrastructure.Auth;
using FiapDonateCampaign.Infrastructure.Identity;

namespace FiapDonateCampaign.API.Controllers;

public record RegisterDto(string Nome, string Email, string Senha, string Role); // Role: "GestorONG" ou "Doador"
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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (dto.Role != "GestorONG" && dto.Role != "Doador")
            return BadRequest("Role inválida. Use 'GestorONG' ou 'Doador'.");

        var usuario = new ApplicationUser { UserName = dto.Email, Email = dto.Email, Nome = dto.Nome };
        var resultado = await _userManager.CreateAsync(usuario, dto.Senha);

        if (!resultado.Succeeded)
            return BadRequest(resultado.Errors);

        await _userManager.AddToRoleAsync(usuario, dto.Role);

        return Ok(new { mensagem = "Usuário registrado com sucesso." });
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