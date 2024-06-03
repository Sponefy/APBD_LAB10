using System;
using System.Collections.Generic;

namespace Lab10.Models;

public partial class ProcedureAnimal
{
    public int ProcedureId { get; set; }

    public int AnimalId { get; set; }

    public DateOnly Date { get; set; }

    public virtual Animal Animal { get; set; } = null!;

    public virtual Procedure Procedure { get; set; } = null!;
}
