using System;
using System.Collections.Generic;

namespace LauGardensApi.Data.Models;

public partial class Stock
{
    public int Id { get; set; }

    public int PlantaId { get; set; }

    public int Quantidade { get; set; }

    public DateTime UltimaAtualizacao { get; set; }

    public int? AtualizadoPorFuncionarioId { get; set; }

    public virtual Funcionario? AtualizadoPorFuncionario { get; set; }

    public virtual Planta Planta { get; set; } = null!;
}
