using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aithos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatasetImportController(
    IDatasetImportService datasetImportService,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DatasetImportResult>> Import(
        [FromBody] DatasetImportRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await datasetImportService.ImportAsync(
            request.DatasetDirectory,
            cancellationToken
        );

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
