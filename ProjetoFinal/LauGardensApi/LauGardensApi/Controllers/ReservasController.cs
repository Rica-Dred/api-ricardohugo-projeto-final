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

    // Certifique-se de que o ReservasController injeta o IAsyncCacheProvider
    // O método GetReserva irá recebê-lo via [FromServices]

    [HttpGet("{id}")]
    public async Task<ActionResult<Reserva>> GetReserva(int id, [FromServices] IAsyncCacheProvider cacheProvider)
    {
        // 1. Política de cache: Usa o adaptador para falar com o Redis, TTL de 10 minutos
        var cachePolicy = Policy.CacheAsync<object>(cacheProvider, TimeSpan.FromMinutes(10));

        try
        {
            // 3. EXECUÇÃO PROTEGIDA: Tenta obter do cache primeiro.
            var resulFinal = await cachePolicy.ExecuteAsync(async (context) =>
            {
                // ESTE CÓDIGO SÓ É EXECUTADO SE O CACHE FALHAR (Cache Miss)

                // BD: Acede à Base de Dados para obter a Reserva
                var reserva = await _context.Reservas.FindAsync(id);

                // Verifica se encontrou (necessário porque o Polly não guarda null por defeito,
                // mas o adaptador PollyRedisAdapt está configurado para lidar com null)
                if (reserva == null) return null;

                // Retorna o objeto para que o Polly guarde no Redis
                return (object)reserva;

            }, new Context($"reserva_{id}")); // Passa a chave dinâmica para a política

            if (resulFinal == null)
            {
                // Se o cache hit ou a BD retornaram null (reserva não existe)
                return NotFound();
            }

            // 4. Retorna o resultado obtido (do Cache ou da BD)
            // O Ok() vai desserializar o 'resulFinal' (que é do tipo Reserva)
            return Ok(resulFinal);
        }
        catch (Exception)
        {
            // Tratamento de erros (consistente com os outros Controllers)
            return StatusCode(500, $"Erro ao carregar a reserva.");
        }
    }
}
