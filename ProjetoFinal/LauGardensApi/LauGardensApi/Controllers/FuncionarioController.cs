using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LauGardensApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FuncionarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/funcionario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios()
        {
            // Se quiseres incluir o Utilizador e Stocks, podes usar Include/ThenInclude
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
            var funcionario = await _context.Funcionarios
                .Include(f => f.Utilizador)
                .Include(f => f.Stocks)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcionario == null) return NotFound();

            return funcionario;
        }

        // POST: api/funcionario
        // Usa o FuncionarioCreateDto
        [HttpPost]
        public async Task<ActionResult<Funcionario>> CreateFuncionario(FuncionarioCreateDto funcionarioDto)
        {
            // Aqui assumo que o FuncionarioCreateDto tem estas propriedades:
            // int UtilizadorId, string Nome, string? Email, string? Telefone, string? Funcao
            // Ajusta os nomes se no teu DTO forem diferentes.
            var novoFuncionario = new Funcionario
            {
                Id = funcionarioDto.Id,
                Nome = funcionarioDto.NomeUtilizador,
                Email = funcionarioDto.Email,
                Funcao = funcionarioDto.Role
            };

            _context.Funcionarios.Add(novoFuncionario);
            await _context.SaveChangesAsync();

            // Podes devolver CreatedAtAction se quiseres seguir mais à risca o REST
            // return CreatedAtAction(nameof(GetFuncionario), new { id = novoFuncionario.Id }, novoFuncionario);
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

            return NoContent();
        }
    }
}

