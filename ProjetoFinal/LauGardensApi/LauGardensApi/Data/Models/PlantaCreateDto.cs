namespace LauGardensApi.Data.Models
{
    public class PlantaCreateDto
    {
        //(DTO - Data Transfer Object) pedimos apenas aquilo que queremos
        public string Nome { get; set; } = null!;

        public string Categoria { get; set; } = null!;

        public decimal Preco { get; set; }

        public string? Descricao { get; set; }

        public string? UrlImagem { get; set; }

        public int QuantidadeInicial { get; set; }

    }
}