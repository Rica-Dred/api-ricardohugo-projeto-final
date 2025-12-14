using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LauGardensApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerAutenticacao : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ControllerAutenticacao(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Autenticacao/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] PedidoAutenticacao request)
        {
            //Procura o utilizador na BD
            var user = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.NomeUtilizador == request.NomeUtilizador);

            if (user == null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            if (user.PasswordHash != request.Password)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            //Cria as claims do utilizador
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.NomeUtilizador),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NomeUtilizador),
                new Claim(ClaimTypes.Role, user.Role ?? "colaborador")
            };

            //Le configurações do JWT
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = double.Parse(jwtSection["ExpireMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiresInMinutes = expireMinutes,
                role = user.Role,
                username = user.NomeUtilizador
            });
        }
    }
}

