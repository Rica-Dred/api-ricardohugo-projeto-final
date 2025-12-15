using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LauGardensApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FuncionarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public FuncionarioController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/funcionario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Utilizador)
                .Include(f => f.Stocks)
                .ToListAsync();

            return Ok(funcionarios);
        }

        // GET: api/funcionario/ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            var dados = await _cache.GetStringAsync($"funcionario_{id}");
            if (!string.IsNullOrEmpty(dados))
            {
                try
                {
                    //temos includes, precisamos de opções para ignorar ciclos
                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<Funcionario>(dados, jsonOptions);
                }
                catch (JsonException)
                {
                    Console.WriteLine("A apagar cache corrompido");
                    await _cache.RemoveAsync($"funcionario_{id}");
                }
            }

            var funcionario = await _context.Funcionarios
                .Include(f => f.Utilizador)
                .Include(f => f.Stocks)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcionario == null) return NotFound();

            return funcionario;
        }

        // POST: api/funcionario
        [HttpPost]
        public async Task<ActionResult<Funcionario>> CreateFuncionario(FuncionarioCreateDto funcionarioDto)
        {

            var novoFuncionario = new Funcionario
            {
                Id = funcionarioDto.Id,
                Nome = funcionarioDto.NomeUtilizador,
                Email = funcionarioDto.Email,
                Funcao = funcionarioDto.Role
            };

            _context.Funcionarios.Add(novoFuncionario);
            await _context.SaveChangesAsync();

            return Ok(novoFuncionario);
        }

        // PUT: api/funcionario/ID
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFuncionario(int id, Funcionario funcionario)
        {
            if (id != funcionario.Id) return BadRequest();

            _context.Entry(funcionario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var existe = await _context.Funcionarios.AnyAsync(f => f.Id == id);
                if (!existe) return NotFound();
                throw;
            }
            await _cache.RemoveAsync($"funcionario_{id}");

            return NoContent();
        }

        // DELETE: api/funcionario/ID
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return NotFound();

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"funcionario_{id}");

            return NoContent();
        }
    }
}

