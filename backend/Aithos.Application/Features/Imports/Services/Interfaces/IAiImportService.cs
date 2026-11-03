using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IAiImportService
{
    Task<AiImportResult> ImportAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
