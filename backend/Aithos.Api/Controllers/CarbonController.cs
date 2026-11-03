using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarbonController(
    ICarbonIntelligenceService carbonService,
    IMemoryCache cache) : ControllerBase
{
    private const string CacheKey = "carbon_report";

    [HttpGet("report")]
    public async Task<CarbonReportDto> Report(CancellationToken cancellationToken)
    {
        // Carbon calculations are cached because they read the complete reporting dataset.
        return await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await carbonService.GetReportAsync(cancellationToken);
        }) ?? await carbonService.GetReportAsync(cancellationToken);
    }

    [HttpPost("report/invalidate")]
    public IActionResult Invalidate()
    {
        cache.Remove(CacheKey);
        return Ok(new { message = "Carbon report cache cleared." });
    }

    [HttpGet("report/pdf")]
    public async Task<IActionResult> Pdf(CancellationToken cancellationToken)
    {
        var pdfBytes = await carbonService.GeneratePdfAsync(cancellationToken);
        return File(pdfBytes, "application/pdf",
            $"Telecommunications-Project-Carbon-Report-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
