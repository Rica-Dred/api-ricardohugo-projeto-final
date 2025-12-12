using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using Microsoft.Extensions.Caching.Distributed; // Para o Redis
using System.Numerics;
using System.Text.Json; //deserializar/serializar dados 

namespace LauGardensApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public UtilizadorController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/utilizadores
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetUtilizadores()
        {
            var utilizadores = await _context.Utilizadores.ToListAsync();

            return Ok(utilizadores);
        }

        // GET: api/utilizador/ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Utilizador>> GetUtilizador(int id)
        {
            var dados = await _cache.GetStringAsync($"utilizador_{id}");
            if(!string.IsNullOrEmpty(dados))
            {
                try
                {
                    return JsonSerializer.Deserialize<Utilizador>(dados);
                }
                catch (JsonException)
                {
                    Console.WriteLine("A apagar cache corrompido");
                    await _cache.RemoveAsync($"utilizador_{id}");
                }
            }

            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return NotFound();

            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            await _cache.SetStringAsync($"utilizador_{id}", JsonSerializer.Serialize(utilizador), options);

            return utilizador;
        }

        // POST: api/utilizador
        [HttpPost]
        public async Task<ActionResult<Utilizador>> CreateUtilizador(UtilizadorCreateDto utilizadorDto)
        {
            var novoUtilizador = new Utilizador
            {
                NomeUtilizador = utilizadorDto.NomeUtilizador,
                PasswordHash = utilizadorDto.PasswordHash,
                // Se o utilizador no estiver autenticado (ex: registo pblico), fora o role "cliente"
                Role = User.Identity?.IsAuthenticated == true && User.IsInRole("admin") 
                       ? utilizadorDto.Role 
                       : "cliente" 
            };

            _context.Utilizadores.Add(novoUtilizador);
            await _context.SaveChangesAsync();
            return Ok(novoUtilizador);
        }

        // PUT: api/utilizador/ID - faz sentido
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUtilizador(int id, Utilizador utilizador)
        {
            if (id != utilizador.Id) return BadRequest();

            _context.Entry(utilizador).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Utilizadores.Any(e => e.Id == id)) 
                    return NotFound();
                else throw;
            }

            await _cache.RemoveAsync($"utilizador_{id}");
            return NoContent();
        }

        // DELETE: api/utilizador/id - faz sentido
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return NotFound();

            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"utilizador_{id}");
            return NoContent();
        }
    }
}