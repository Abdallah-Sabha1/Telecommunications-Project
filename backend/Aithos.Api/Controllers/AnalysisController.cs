using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController(IAnalysisService analysisService) : ControllerBase
{
    [HttpPost("{towerId:int}")]
    public async Task<IActionResult> Analyze(int towerId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await analysisService.AnalyzeAsync(towerId, cancellationToken));
        }
        catch (AnalysisNotFoundException) { return NotFound(); }
        catch (AnalysisInputException exception) { return BadRequest(exception.Message); }
        catch (ClaudeApiException exception) { return StatusCode(StatusCodes.Status502BadGateway, new { error = exception.Message }); }
    }
}
