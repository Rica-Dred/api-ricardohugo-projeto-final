using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Caching;
using Microsoft.Extensions.Caching.Distributed; // Para o Redis
using System.Text.Json;

namespace LauGardensApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _clientFactory;

    public ReservasController(AppDbContext context, IHttpClientFactory clientFactory)
    {
        _context = context;
        _clientFactory = clientFactory;
    }

    [HttpPost][Authorize(Roles = "cliente")] //Restringe acesso
    public async Task<ActionResult<Reserva>> CreateReserva(Reserva reserva)
    {
        // Validação básica
        if (string.IsNullOrWhiteSpace(reserva.NomeCliente) || string.IsNullOrWhiteSpace(reserva.Contacto)) //verificacao de dados
        {
            return BadRequest("Nome e Contacto obrigatórios.");
        }

        // Verifica se a planta existe
        var plantaExists = await _context.Plantas.AnyAsync(p => p.Id == reserva.PlantaId);
        if (!plantaExists)
        {
            return BadRequest("Planta inválida.");
        }

        // Se o pagamento falhar, a reserva não é criada.
        try
        {
            //Caso falhe a comunicação com o serviço será tratada pelas políticas de Retry e Circuit Breaker
            var client = _clientFactory.CreateClient("ImposterApi"); 
            
            //Simular dados de pagamento
            var paymentData = new { cliente = reserva.NomeCliente, valor = 100 }; 
            
            //Simulacao de envio
            var response = await client.PostAsJsonAsync("/payments", paymentData);

            if (!response.IsSuccessStatusCode)
            {
                //Pagamento Recusado
                return BadRequest("Pagamento Recusado");
            }
        }
        catch (Exception ex)
        {
            //Caso de erro de comunicação com o Mock, erro 
             return StatusCode(500, "Erro ao validar pagamento: " + ex.Message);
        }

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        // Persistência reativada.

        return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, reserva);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reserva>> GetReserva(int id, [FromServices] IAsyncCacheProvider cacheProvider)
    {
        //Política de cache
        var cachePolicy = Policy.CacheAsync<object>(cacheProvider, TimeSpan.FromMinutes(10));

        try
        {
            var resulFinal = await cachePolicy.ExecuteAsync(async (context) =>
            {
                //Executado caso cache falhe
                //Acede bd
                var reserva = await _context.Reservas.FindAsync(id);

                // Verifica se encontrou 
                if (reserva == null) return null;

                //Retorna o objeto para que o Polly guarde no Redis
                return (object)reserva;

            }, new Context($"reserva_{id}")); // Passa a chave dinâmica

            if (resulFinal == null)
            {
                return NotFound();
            }

            //Retorna o resultado obtido (do Cache ou da BD)
            return Ok(resulFinal);
        }
        catch (Exception)
        {
            return StatusCode(500, $"Erro ao carregar a reserva.");
        }
    }
}
