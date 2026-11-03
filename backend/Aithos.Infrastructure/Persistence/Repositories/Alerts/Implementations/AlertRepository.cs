using Aithos.Infrastructure.Persistence;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository(AithosDbContext dbContext) : IAlertRepository
{
    public async Task<List<Alert>> GetAsync(
        string? severity,
        string sort,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.Alerts
            .AsNoTracking()
            .Include(alert => alert.Tower)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(alert => alert.Severity == severity);
        }

        query = sort.ToLowerInvariant() switch
        {
            "confidence" => query.OrderByDescending(alert => alert.Confidence),
            "severity" => query
                .OrderByDescending(alert => alert.Severity == "Critical" ? 3
                    : alert.Severity == "Warning" ? 2 : 1)
                .ThenByDescending(alert => alert.CreatedAt),
            _ => query.OrderByDescending(alert => alert.CreatedAt),
        };

        return await query.ToListAsync(cancellationToken);
    }

    public Task<List<Alert>> GetOpenAsync(CancellationToken cancellationToken)
    {
        return dbContext.Alerts
            .AsNoTracking()
            .Include(alert => alert.Tower)
            .Where(alert => !alert.IsResolved)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Alert>> GetRecentOpenByTowerAsync(
        int towerId,
        int count,
        CancellationToken cancellationToken
    )
    {
        return dbContext.Alerts
            .AsNoTracking()
            .Where(alert => alert.TowerId == towerId && !alert.IsResolved)
            .OrderByDescending(alert => alert.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task<Alert?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Alerts
            .Include(alert => alert.Tower)
            .FirstOrDefaultAsync(alert => alert.Id == id, cancellationToken);
    }

    public Task<List<Alert>> GetByTowerIdAsync(
        int towerId,
        CancellationToken cancellationToken
    )
    {
        return dbContext.Alerts
            .Include(alert => alert.Tower)
            .Where(alert => alert.TowerId == towerId)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
