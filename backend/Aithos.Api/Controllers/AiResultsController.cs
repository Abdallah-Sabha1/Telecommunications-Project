using Aithos.Application.Abstractions.Persistence;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/ai-results")]
public class AiResultsController(IAiAnalysisResultRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AiResultDto>>> Get(
        [FromQuery] string? siteId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken
    )
    {
        var entities = await repository.GetAsync(siteId, from, to, cancellationToken);
        var results = entities.Select(result => new AiResultDto(
                result.SiteId,
                result.ObservationDate,
                result.ActualKwh,
                result.ExpectedKwh,
                result.Residual,
                result.ScoreZ,
                result.Classification,
                result.Slope,
                result.Reason,
                result.WastedKwh,
                result.WastedCostJod,
                result.CarbonImpactKgCo2e,
                result.CarbonEvaluated,
                result.ModelVersion))
            .ToList();

        return Ok(results);
    }
}
