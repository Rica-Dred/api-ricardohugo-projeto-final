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

    public ReservasController(AppDbContext context)
    {
        _context = context;
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
