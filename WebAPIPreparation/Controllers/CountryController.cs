using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIPreparation.Auth;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

namespace WebAPIPreparation.Controllers;

[ApiController]
[Route("api/[controller]")]          // → api/countries
public class CountryController(ICountryService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.MastersView)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct) =>
        Ok(await service.GetAllAsync(search, ct));

    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.MastersView)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct) =>
        await service.GetByIdAsync(id, ct) is { } country ? Ok(country) : NotFound();

    [HttpPost]
    [Authorize(Policy = Permissions.MastersEdit)]
    public async Task<IActionResult> Create(CreateCountryRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Permissions.MastersEdit)]
    public async Task<IActionResult> Update(long id, UpdateCountryRequest request, CancellationToken ct) =>
        await service.UpdateAsync(id, request, ct) is { } country ? Ok(country) : NotFound();

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Permissions.MastersEdit)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}