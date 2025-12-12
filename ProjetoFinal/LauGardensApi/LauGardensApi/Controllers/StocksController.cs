using LauGardensApi.Data;
using LauGardensApi.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LauGardensApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly AppDbContext _context;

    public StocksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("Checkout")]
    public async Task<IActionResult> Checkout([FromBody] List<CheckoutItemDto> items)
    {
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
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Compra realizada e stock atualizado com sucesso." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erro ao processar checkout: {ex.Message}");
        }
    }
    [HttpPut("{plantaId}")]
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
