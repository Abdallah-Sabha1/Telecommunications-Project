using Aithos.Domain.Entities;

namespace Aithos.Application.Abstractions.Persistence;

public interface IAiAnalysisResultRepository
{
    Task<IReadOnlyList<AiAnalysisResult>> GetAsync(
        string? siteId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}
