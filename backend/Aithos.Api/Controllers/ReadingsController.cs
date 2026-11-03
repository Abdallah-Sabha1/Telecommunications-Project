using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController(IReadingService readingService) : ControllerBase
{
    [HttpGet]
    public Task<List<ReadingDto>> Get([FromQuery] int? towerId, CancellationToken cancellationToken)
    {
        return readingService.GetAllAsync(towerId, cancellationToken);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var reading = await readingService.GetByIdAsync(id, cancellationToken);

        return reading is null ? NotFound() : Ok(reading);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateReadingDto request, CancellationToken cancellationToken)
    {
        var reading = await readingService.CreateAsync(request, cancellationToken);

        if (reading is null)
        {
            return BadRequest("Tower does not exist.");
        }

        return CreatedAtAction(nameof(Get), new { id = reading.Id }, reading);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, UpdateReadingDto request, CancellationToken cancellationToken)
    {
        var wasUpdated = await readingService.UpdateAsync(id, request, cancellationToken);

        return wasUpdated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var wasDeleted = await readingService.DeleteAsync(id, cancellationToken);

        return wasDeleted ? NoContent() : NotFound();
    }
}
