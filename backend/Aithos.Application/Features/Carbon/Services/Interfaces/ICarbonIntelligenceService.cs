using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface ICarbonIntelligenceService
{
    Task<CarbonReportDto> GetReportAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePdfAsync(CancellationToken cancellationToken = default);
}
