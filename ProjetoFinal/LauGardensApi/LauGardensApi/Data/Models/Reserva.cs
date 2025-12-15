using System.Text.Json.Serialization;

namespace LauGardensApi.Data.Models;

public class Reserva
{
    public int Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public int PlantaId { get; set; }
    public DateTime DataReserva { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pendente";
}
