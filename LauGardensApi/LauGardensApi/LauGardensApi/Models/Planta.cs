//using Microsoft.EntityFrameworkCore
//using Microsoft.FrameworkCore.InMemory

namespace LauGardensApi.Models
{
    public class Planta
    {
        int Id { get; set; }

        public string? Nome { get; set; }

        public string? Tipo { get; set; }

        public string? Cor {  get; set; }

        public string? Aroma { get; set; }
        
        public int? Altura { get; set; }

        public float? Preco { get; set; }
    }
}
