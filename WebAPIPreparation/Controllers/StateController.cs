using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIPreparation.Auth;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

namespace WebAPIPreparation.Controllers;

[ApiController]
[Route("api/[controller]")]          // → api/states
public class StateController(IStateService service) : ControllerBase
{
    /// <summary>All states, or only one country's states: GET api/states?countryId=1</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.MastersView)]
    public async Task<IActionResult> GetAll([FromQuery] long? countryId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(countryId, ct));

    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.MastersView)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct) =>
        await service.GetByIdAsync(id, ct) is { } state ? Ok(state) : NotFound();

    [HttpPost]
    [Authorize(Policy = Permissions.MastersEdit)]
    public async Task<IActionResult> Create(CreateStateRequest request, CancellationToken ct)
    {
        var created = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = Permissions.MastersEdit)]
    public async Task<IActionResult> Update(long id, UpdateStateRequest request, CancellationToken ct) =>
        await service.UpdateAsync(id, request, ct) is { } state ? Ok(state) : NotFound();

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Permissions.MastersEdit)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}