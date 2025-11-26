using System;
using System.Collections.Generic;

namespace LauGardensApi.Data.Models;

public partial class Funcionario
{
    public int Id { get; set; }

    public int UtilizadorId { get; set; }

    public string Nome { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string? Funcao { get; set; }

    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();

    public virtual Utilizador Utilizador { get; set; } = null!;
}
