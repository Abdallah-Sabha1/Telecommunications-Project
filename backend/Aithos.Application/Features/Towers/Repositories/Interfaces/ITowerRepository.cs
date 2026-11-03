using Aithos.Domain.Entities;

namespace Aithos.Application.Abstractions.Persistence;

public interface ITowerRepository
{
    Task<List<Tower>> GetAllAsync(CancellationToken cancellationToken);
    Task<Tower?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Tower tower, CancellationToken cancellationToken);
    void Remove(Tower tower);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
