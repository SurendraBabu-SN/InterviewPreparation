using System;
using System.Collections.Generic;

namespace WebAPIPreparation.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Address { get; set; }

    public string? Pincode { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }
}
