using System;
using System.Collections.Generic;

namespace Lab10.Models;

public partial class Animal
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateOnly AdmissionDate { get; set; }

    public int OwnerId { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<ProcedureAnimal> ProcedureAnimals { get; set; } = new List<ProcedureAnimal>();
}
