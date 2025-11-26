using System;
using System.Collections.Generic;

namespace LauGardensApi.Data.Models;

public partial class Planta
{
    public int Id { get; set; }

    public string Nome { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public decimal Preco { get; set; }

    public string? Descricao { get; set; }

    public string? UrlImagem { get; set; }

    public virtual Stock? Stock { get; set; }
}
