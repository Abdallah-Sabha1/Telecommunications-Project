using Aithos.Domain.Entities;

namespace Aithos.Application.Abstractions.Persistence;

public interface IAlertRepository
{
    Task<List<Alert>> GetAsync(string? severity, string sort, CancellationToken cancellationToken);
    Task<List<Alert>> GetOpenAsync(CancellationToken cancellationToken);
    Task<List<Alert>> GetRecentOpenByTowerAsync(int towerId, int count, CancellationToken cancellationToken);
    Task<Alert?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Alert>> GetByTowerIdAsync(int towerId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
