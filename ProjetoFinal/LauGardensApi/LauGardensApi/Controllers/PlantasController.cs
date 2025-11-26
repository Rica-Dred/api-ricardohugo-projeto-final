using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LauGardensApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PlantasController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlantasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/plantas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Planta>>> GetPlantas()
    {
        var plantas = await _context.Plantas.Include(p => p.Stock).ToListAsync();

        return Ok(plantas);
    }

    // GET: api/plantas/ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Planta>> GetPlanta(int id)
    {
        var planta = await _context.Plantas.FindAsync(id);
        if (planta == null) return NotFound();
        return planta;
    }

    // POST: api/plantas
    [HttpPost]
    public async Task<ActionResult<Planta>> CreatePlanta(PlantaCreateDto plantaDto)
    {
        var novaPlanta = new Planta
        {
            Nome = plantaDto.Nome,
            Categoria = plantaDto.Categoria,
            Preco = plantaDto.Preco,
            Descricao = plantaDto.Descricao,
            UrlImagem = plantaDto.UrlImagem
        };

        _context.Plantas.Add(novaPlanta);
        await _context.SaveChangesAsync();
        return Ok(novaPlanta);
    }

    // PUT: api/plantas/ID
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePlanta(int id, Planta planta)
    {
        if (id != planta.Id) return BadRequest();

        _context.Entry(planta).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/plantas/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePlanta(int id)
    {
        var planta = await _context.Plantas.FindAsync(id);
        if (planta == null) return NotFound();

        _context.Plantas.Remove(planta);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}