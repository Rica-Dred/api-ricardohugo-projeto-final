using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LauGardensApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtilizadorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/utilizadores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetUtilizadores()
        {
            var utilizadores = await _context.Utilizadores.ToListAsync();

            return Ok(utilizadores);
        }

        // GET: api/utilizador/ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Utilizador>> GetUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return NotFound();
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
                Role = utilizadorDto.Role
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
            return NoContent();
        }

        // DELETE: api/utilizador/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return NotFound();

            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}