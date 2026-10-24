using System;
using System.Collections.Generic;

namespace WebAPIPreparation.Models;

public partial class Country
{
    public long Id { get; set; }

    public string CountryName { get; set; } = null!;
}
