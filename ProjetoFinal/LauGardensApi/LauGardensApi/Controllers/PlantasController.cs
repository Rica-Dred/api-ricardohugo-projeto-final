using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // Para o Redis
using Polly;
using Polly.Caching;
using System.Text.Json;
using System.Text.Json.Serialization; //deserializar/serializar dados 

namespace LauGardensApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache; // Para o Redis
    private readonly IHttpClientFactory _clientFactory; // P/Polly

    public PlantasController(AppDbContext context, IDistributedCache cache, IHttpClientFactory clientFactory)
    {
        _context = context;
        _cache = cache; //guardar cache na variavel
        _clientFactory = clientFactory; //"chefe logistica"
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
    public async Task<ActionResult<object>>GetPlanta(int id, [FromServices] IAsyncCacheProvider cacheProvider)
    {
        // 1. DEFINIR A POLÍTICA DE CACHE (Isto substitui o teu if manual)
        // O Polly vai usar o teu 'cacheProvider' para verificar o Redis automaticamente
        var cachePolicy = Policy.CacheAsync<object>(cacheProvider, TimeSpan.FromMinutes(10));

        try
        {
            // 2. EXECUTAR VIA POLLY
            // O código dentro deste bloco SÓ corre se o Polly não encontrar nada no cache
            var finalResult = await cachePolicy.ExecuteAsync(async (context) =>
            {
                // === A TUA LÓGICA DE DADOS (Caminho Lento) ===

                // A. Base de Dados
                var planta = await _context.Plantas.FindAsync(id);
                if (planta == null) return null;

                // B. Polly Resiliência (Imposter)
                var clientPolly = _clientFactory.CreateClient("ImposterApi");
                object stockInfo = "Indisponivel";

                try
                {
                    var response = await clientPolly.GetAsync($"/inventory/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        stockInfo = JsonSerializer.Deserialize<object>(jsonString);
                    }
                }
                catch (Exception) { }

                // C. Agregação
                var result = new
                {
                    DadosPlanta = planta,
                    StockExterno = stockInfo
                };

                // Retornar para que o Polly guarde no Redis
                return (object)result;

            }, new Context($"planta_{id}"));

            // 3. RESULTADO FINAL
            if (finalResult == null) return NotFound();

            return Ok(finalResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
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