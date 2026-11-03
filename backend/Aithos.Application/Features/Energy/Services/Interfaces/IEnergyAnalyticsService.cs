using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IEnergyAnalyticsService
{
    Task<EnergyAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default);
}
