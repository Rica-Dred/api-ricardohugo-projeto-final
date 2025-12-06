using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Distributed; // Para o Redis
using System.Text.Json; //deserializar/serializar dados 

namespace LauGardensApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PlantasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache; // Para o Redis

    public PlantasController(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache; //guardar cache na variavel
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
        var dados = await _cache.GetStringAsync($"planta_{id}");
        //caso haja em cache retorna o valor
        if (!string.IsNullOrEmpty($"planta_{id}"))
        {
            try
            {
                return JsonSerializer.Deserialize<Planta>($"planta_{id}");
            }
            catch (JsonException)
            {
                // SE FALHAR
                // Ignora o erro, apaga o cache estragado e segue para a BD
                Console.WriteLine("Cache corrompido, a apagar...");
                await _cache.RemoveAsync($"planta_{id}");
            }
        }

        //caso nao haja em cache vai buscar à base de dados (caminho mais longo)
        var planta = await _context.Plantas.FindAsync(id);
        if (planta == null) return NotFound();

        //guardar em cache o valor obtido da base de dados
        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        await _cache.SetStringAsync($"planta_{id}", JsonSerializer.Serialize(planta), options);

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

        await _cache.RemoveAsync($"planta_{id}"); //Remove cache antigo mantendo sempre atualizado

        return NoContent();
    }

    // DELETE: api/plantas/ID
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePlanta(int id)
    {

        var planta = await _context.Plantas.FindAsync(id);
        if (planta == null) return NotFound();

        _context.Plantas.Remove(planta);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"planta_{id}"); //Remove cache antigo mantendo sempre atualizado

        return NoContent();
    }
}