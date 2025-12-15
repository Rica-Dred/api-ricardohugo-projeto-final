using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // Para o Redis
using Polly;
using Polly.Caching;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

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
    public async Task<ActionResult<IEnumerable<Planta>>> GetPlantas([FromServices] IAsyncCacheProvider cacheProvider)
    {
        var cachePolicy = Policy.CacheAsync<object>(cacheProvider, TimeSpan.FromMinutes(10));

        try
        {
            var resulFinal = await cachePolicy.ExecuteAsync(async (context) =>
            {
                //BD
                var plantas = await _context.Plantas.Include(p => p.Stock).ToListAsync();
                return (object)plantas;

            }, new Context("lista_plantas_completa")); //Passa a chave do cache para a política usar

            if (resulFinal == null) return NotFound();
            return Ok(resulFinal);
        }
        catch (Exception)
        {
            // Tratamento de erro, se a BD falhar, isto apanha.
            return StatusCode(500, $"Erro ao carregar plantas.");
        }
    }

    // GET: api/plantas/ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>>GetPlanta(int id, [FromServices] IAsyncCacheProvider cacheProvider)
    {
        //politica de cache, Polly vai usar 'cacheProvider' para verificar o Redis
        var cachePolicy = Policy.CacheAsync<object>(cacheProvider, TimeSpan.FromMinutes(10));

        try
        {
            //so executado se o Polly não encontrar nada no cache
            var resulFinal = await cachePolicy.ExecuteAsync(async (context) =>
            {
                //BD
                var planta = await _context.Plantas.FindAsync(id);
                if (planta == null) return null;

                //Polly Resiliência
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

                //Agregação
                var result = new
                {
                    DadosPlanta = planta,
                    StockExterno = stockInfo
                };

                //Retornar para que o Polly guarde no Redis
                return (object)result;

            }, new Context($"planta_{id}"));

            if (resulFinal == null) return NotFound();

            return Ok(resulFinal);
        }
        catch (Exception)
        {
            return StatusCode(500, $"Erro ao carregar plantas.");
        }
    }

    // POST: api/plantas
    [HttpPost]
    [Authorize(Roles = "admin")]
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

        // Criar entrada de Stock automaticamente
        var novoStock = new Stock
        {
            PlantaId = novaPlanta.Id,
            Quantidade = plantaDto.QuantidadeInicial,
            UltimaAtualizacao = DateTime.Now
        };

        _context.Stocks.Add(novoStock);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync("lista_plantas_completa"); // Invalida a lista de plantas

        return Ok(novaPlanta);
    }

    // PUT: api/plantas/ID
    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdatePlanta(int id, Planta planta)
    {

        if (id != planta.Id) return BadRequest();

        _context.Entry(planta).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"planta_{id}"); //Remove cache antigo mantendo sempre atualizado
        await _cache.RemoveAsync("lista_plantas_completa"); // Invalida a lista de plantas

        return NoContent();
    }

    // DELETE: api/plantas/ID
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeletePlanta(int id)
    {

        var planta = await _context.Plantas.FindAsync(id);
        if (planta == null) return NotFound();

        _context.Plantas.Remove(planta);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"planta_{id}"); //Remove cache antigo mantendo sempre atualizado
        await _cache.RemoveAsync("lista_plantas_completa"); // Invalida a lista de plantas

        return NoContent();
    }
}