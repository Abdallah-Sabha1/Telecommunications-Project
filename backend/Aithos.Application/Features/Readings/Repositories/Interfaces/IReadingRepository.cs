using Aithos.Domain.Entities;

namespace Aithos.Application.Abstractions.Persistence;

public interface IReadingRepository
{
    Task<List<Reading>> GetAllAsync(int? towerId, CancellationToken cancellationToken);
    Task<List<Reading>> GetAllWithTowersAsync(CancellationToken cancellationToken);
    Task<List<Reading>> GetRecentByTowerAsync(int towerId, int count, CancellationToken cancellationToken);
    Task<Reading?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken);
    Task AddAsync(Reading reading, CancellationToken cancellationToken);
    void Remove(Reading reading);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
