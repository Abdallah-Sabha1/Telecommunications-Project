using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public Task<DashboardSummaryDto> Get(CancellationToken cancellationToken)
    {
        return dashboardService.GetSummaryAsync(cancellationToken);
    }

    [HttpGet("tower/{towerId:int}")]
    public async Task<ActionResult<TowerDetailsDto>> GetTowerDetails(
        int towerId,
        CancellationToken cancellationToken
    )
    {
        var tower = await dashboardService.GetTowerDetailsAsync(towerId, cancellationToken);
        return tower is null ? NotFound() : Ok(tower);
    }
}
