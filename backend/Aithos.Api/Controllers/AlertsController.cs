using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController(IAlertService alertService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> Get([FromQuery] string? severity = null, [FromQuery] string sort = "latest", CancellationToken cancellationToken = default) =>
        Ok(await alertService.GetAsync(severity, sort, cancellationToken));

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<AlertDto>> UpdateStatus(int id, [FromBody] AlertWorkflowRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await alertService.UpdateStatusAsync(id, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [HttpPatch("tower/{towerId:int}/status")]
    public async Task<ActionResult<AlertDto>> UpdateTowerStatus(
        int towerId,
        [FromBody] AlertWorkflowRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await alertService.UpdateTowerStatusAsync(towerId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [HttpPatch("{id:int}/assignee")]
    public async Task<ActionResult<AlertDto>> UpdateAssignee(int id, [FromBody] AlertAssigneeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await alertService.UpdateAssigneeAsync(id, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [HttpPatch("tower/{towerId:int}/assignee")]
    public async Task<ActionResult<AlertDto>> UpdateTowerAssignee(
        int towerId,
        [FromBody] AlertAssigneeRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await alertService.UpdateTowerAssigneeAsync(towerId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }
}
