using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPIPreparation.Models;

public partial class State
{
    public long Id { get; set; }

    public string StateName { get; set; } = null!;

    public long CountryId { get; set; }
}

public record StateResponse(long Id, string StateName, long CountryId, string? CountryName);

public record CreateStateRequest(
    [Range(1, long.MaxValue)] long Id,
    [Required, StringLength(100)] string StateName,
    [Range(1, long.MaxValue)] long CountryId);

public record UpdateStateRequest(
    [Required, StringLength(100)] string StateName,
    [Range(1, long.MaxValue)] long CountryId);