using System;
using System.Collections.Generic;

namespace Lab10.Models;

public partial class Procedure
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<ProcedureAnimal> ProcedureAnimals { get; set; } = new List<ProcedureAnimal>();
}
