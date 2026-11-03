using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnergyController(IEnergyAnalyticsService energyAnalyticsService) : ControllerBase
{
    [HttpGet("analytics")]
    public Task<EnergyAnalyticsDto> GetAnalytics(CancellationToken cancellationToken)
    {
        return energyAnalyticsService.GetAnalyticsAsync(cancellationToken);
    }
}
