using System;
using System.Collections.Generic;

namespace LauGardensApi.Data.Models;

public partial class Utilizador
{
    public int Id { get; set; }

    public string NomeUtilizador { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual Funcionario? Funcionario { get; set; }
}
