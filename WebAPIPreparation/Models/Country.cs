using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPIPreparation.Models;

public partial class Country
{
    public long Id { get; set; }

    public string CountryName { get; set; } = null!;
}

public record CountryResponse(long Id, string CountryName);

public record CreateCountryRequest(
[Range(1, long.MaxValue)] long Id,                 // Id is not IDENTITY, so the caller sends it
[Required, StringLength(100)] string CountryName);

public record UpdateCountryRequest(
[Required, StringLength(100)] string CountryName); // Id comes from the URL, not the body