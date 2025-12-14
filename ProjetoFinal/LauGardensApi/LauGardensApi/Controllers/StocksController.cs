using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LauGardensApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _clientFactory;

    public StocksController(AppDbContext context, IHttpClientFactory clientFactory)
    {
        _context = context;
        _clientFactory = clientFactory;
    }

    [HttpPost("Checkout")]
    [Authorize(Roles = "cliente")]
    public async Task<IActionResult> Checkout([FromBody] List<CheckoutItemDto> items)
    {
        try
        {
            var client = _clientFactory.CreateClient("ImposterApi");
            // Payload simples (podia vir do frontend, mas para demo usamos fixo/calculado)
            var paymentData = new { cliente = "Cliente Checkout", valor = 100 }; 
            
            var response = await client.PostAsJsonAsync("/payments", paymentData);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Pagamento Recusado pela entidade financeira (Simulação).");
            }
        }
        catch (Exception ex)
        {
             return StatusCode(500, "Erro ao validar pagamento (Mock): " + ex.Message);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in items)
            {
                var stock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.PlantaId == item.PlantaId);

                if (stock == null)
                {
                    return BadRequest($"Stock não encontrado para a planta ID {item.PlantaId}");
                }

                if (stock.Quantidade < item.Quantidade)
                {
                    return BadRequest($"Stock insuficiente para a planta ID {item.PlantaId}. Stock disponível: {stock.Quantidade}");
                }

                stock.Quantidade -= item.Quantidade;
                stock.UltimaAtualizacao = DateTime.Now;

                //Cria Reserva
                var reserva = new Reserva
                {
                    PlantaId = item.PlantaId,
                    NomeCliente = User.Identity?.Name ?? "Cliente Web",
                    Contacto = "Checkout Online",
                    DataReserva = DateTime.UtcNow,
                    Status = "Pago" // Define o status como pago, já que vem do checkout
                };
                _context.Reservas.Add(reserva);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Compra realizada, stock atualizado e reservas criadas." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erro ao processar checkout: {ex.Message}");
        }
    }
    [HttpPut("{plantaId}")]
    [Authorize(Roles = "admin,func")]
    public async Task<IActionResult> UpdateStock(int plantaId, [FromBody] StockUpdateDto stockDto)
    {
        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.PlantaId == plantaId);

        if (stock == null)
        {
            return NotFound($"Stock não encontrado para a planta ID {plantaId}");
        }

        stock.Quantidade = stockDto.Quantidade;
        stock.UltimaAtualizacao = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(new { message = "Stock atualizado com sucesso.", novoStock = stock.Quantidade });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao atualizar stock: {ex.Message}");
        }
    }
}
