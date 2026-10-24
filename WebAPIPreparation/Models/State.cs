using System;
using System.Collections.Generic;

namespace WebAPIPreparation.Models;

public partial class State
{
    public long Id { get; set; }

    public string StateName { get; set; } = null!;

    public long CountryId { get; set; }
}
