using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IAnalysisService
{
    Task<AnalysisResult> AnalyzeAsync(int towerId, CancellationToken cancellationToken);
}
