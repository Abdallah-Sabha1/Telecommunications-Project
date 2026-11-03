using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TowersController(ITowerService towerService) : ControllerBase
{
    [HttpGet]
    public Task<List<TowerDto>> Get(CancellationToken cancellationToken)
    {
        return towerService.GetAllAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var tower = await towerService.GetByIdAsync(id, cancellationToken);

        return tower is null ? NotFound() : Ok(tower);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateTowerDto request, CancellationToken cancellationToken)
    {
        var tower = await towerService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = tower.Id }, tower);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, UpdateTowerDto request, CancellationToken cancellationToken)
    {
        var wasUpdated = await towerService.UpdateAsync(id, request, cancellationToken);

        return wasUpdated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var wasDeleted = await towerService.DeleteAsync(id, cancellationToken);

        return wasDeleted ? NoContent() : NotFound();
    }
}
