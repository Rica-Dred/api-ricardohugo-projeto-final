using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // Para o Redis
using Polly;
using Polly.Caching;
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
        [HttpGet][Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetUtilizadores([FromServices] IAsyncCacheProvider cacheProvider)
        {
            // Define a política de cache com TTL de 10 minutos
            var cachePolicy = Policy.CacheAsync<object>(cacheProvider, TimeSpan.FromMinutes(10));

            try
            {
                // EXECUÇÃO PROTEGIDA: Tenta obter do cache (Redis) primeiro.
                var resulFinal = await cachePolicy.ExecuteAsync(async (context) =>
                {
                    // ESTE CÓDIGO SÓ É EXECUTADO SE O CACHE FALHAR

                    // BD: Acede à Base de Dados para obter a lista de Utilizadores
                    var utilizadores = await _context.Utilizadores.ToListAsync();

                    // Retorna o objeto para que o Polly guarde no Redis
                    return (object)utilizadores;

                }, new Context("lista_utilizadores_completa")); // Passa a chave do cache para a política

                if (resulFinal == null) return NotFound();

                // Retorna o resultado obtido (do Cache ou da BD)
                return Ok(resulFinal);
            }
            catch (Exception)
            {
                // Se houver uma falha no Redis ou na BD, retorna 500.
                return StatusCode(500, $"Erro ao carregar a lista de utilizadores.");
            }
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

        // PUT: api/utilizador/ID
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

        // DELETE: api/utilizador/id
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