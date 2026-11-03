using Aithos.Infrastructure.Persistence;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class TowerRepository(AithosDbContext dbContext) : ITowerRepository
{
    public Task<List<Tower>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.Towers
            .AsNoTracking()
            .OrderBy(tower => tower.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Tower?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken
    )
    {
        var query = trackChanges
            ? dbContext.Towers.AsQueryable()
            : dbContext.Towers.AsNoTracking();

        return query.FirstOrDefaultAsync(tower => tower.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Towers.AnyAsync(tower => tower.Id == id, cancellationToken);
    }

    public async Task AddAsync(Tower tower, CancellationToken cancellationToken)
    {
        await dbContext.Towers.AddAsync(tower, cancellationToken);
    }

    public void Remove(Tower tower)
    {
        dbContext.Towers.Remove(tower);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
