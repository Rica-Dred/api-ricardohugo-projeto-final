namespace LauGardensApi.Data.Models
{
    public class UtilizadorCreateDto
    {
        public string NomeUtilizador { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = null!;

    }
}