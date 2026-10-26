using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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


public record CustomerResponse(int Id, string CustomerName, string? City, string? State, string? Country);

public record CreateCustomerRequest(
    [Range(1, int.MaxValue)] int Id,
    [Required, StringLength(100)] string CustomerName,
    [StringLength(50)] string? City,
    [StringLength(100)] string? State,
    [StringLength(100)] string? Country);

public record UpdateCustomerRequest(
    [Required, StringLength(100)] string CustomerName,
    [StringLength(50)] string? City,
    [StringLength(100)] string? State,
    [StringLength(100)] string? Country);