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

    // GET: api/plantas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Planta>> GetPlanta(int id)
    {
        var planta = await _context.Plantas.FindAsync(id);
        if (planta == null) return NotFound();
        return planta;
    }

    // POST: api/plantas
    [HttpPost]
    public async Task<ActionResult<Planta>> CreatePlanta(Planta planta)
    {
        _context.Plantas.Add(planta);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPlanta), new { id = planta.Id }, planta);
    }

    // PUT: api/plantas/5
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