using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IClaudeService
{
    Task<AnalysisResult> AnalyzeTowerAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default
    );
}
