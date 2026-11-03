using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/ai-import")]
public class AiImportController(IAiImportService aiImportService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AiImportResult>> Import(
        IFormFile file,
        CancellationToken cancellationToken
    )
    {
        await using var fileStream = file.OpenReadStream();
        var result = await aiImportService.ImportAsync(fileStream, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
