using FiapDonateCampaign.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FiapDonateCampaign.Infrastructure.Auth
{
    public interface ITokenService 
    {
        string GerarToken(ApplicationUser usuario, IList<string> roles);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration) => _configuration = configuration;

        public string GerarToken(ApplicationUser usuario, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id),
                new(ClaimTypes.NameIdentifier, usuario.Id),
                new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
                new("nome", usuario.Nome),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // é essa linha que faz o [Authorize(Roles = "GestorONG")] funcionar
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
