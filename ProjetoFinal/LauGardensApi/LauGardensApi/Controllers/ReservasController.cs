using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

    [HttpPost]
    [Authorize(Roles = "cliente")]
    public async Task<ActionResult<Reserva>> CreateReserva(Reserva reserva)
    {
        // Validação básica
        if (string.IsNullOrWhiteSpace(reserva.NomeCliente) || string.IsNullOrWhiteSpace(reserva.Contacto))
        {
            return BadRequest("Nome e Contacto são obrigatórios.");
        }

        // Verifica se a planta existe
        var plantaExists = await _context.Plantas.AnyAsync(p => p.Id == reserva.PlantaId);
        if (!plantaExists)
        {
            return BadRequest("Planta inválida.");
        }

        // --- MOCK PAGAMENTO (IMPOSTER) ---
        // Aqui simulamos uma chamada a um serviço externo de pagamentos
        // Se o pagamento falhar, a reserva não é criada.
        try
        {
            var client = _clientFactory.CreateClient("ImposterApi");
            // Payload simples para simular dados de pagamento
            var paymentData = new { cliente = reserva.NomeCliente, valor = 100 }; 
            
            var response = await client.PostAsJsonAsync("/payments", paymentData);

            if (!response.IsSuccessStatusCode)
            {
                // Pagamento Recusado (Simulação: 402 ou 400)
                return BadRequest("Pagamento Recusado pela entidade financeira (Simulação).");
            }
        }
        catch (Exception ex)
        {
            // Em caso de erro de comunicação com o Mock, decidimos se falha ou deixa passar.
            // Para segurança, vamos bloquear.
             return StatusCode(500, "Erro ao validar pagamento: " + ex.Message);
        }
        // --- FIM MOCK PAGAMENTO ---

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReserva), new { id = reserva.Id }, reserva);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reserva>> GetReserva(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);

        if (reserva == null)
        {
            return NotFound();
        }

        return reserva;
    }
}
