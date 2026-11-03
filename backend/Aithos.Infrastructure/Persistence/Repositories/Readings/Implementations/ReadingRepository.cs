using Aithos.Infrastructure.Persistence;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class ReadingRepository(
    AithosDbContext dbContext,
    IConfiguration configuration
) : IReadingRepository
{
    private readonly int reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    public Task<List<Reading>> GetAllAsync(
        int? towerId,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.Readings.AsNoTracking();
        query = query.Where(reading => reading.RecordedAt.Year == reportingYear);

        if (towerId.HasValue)
        {
            query = query.Where(reading => reading.TowerId == towerId.Value);
        }

        return query
            .OrderByDescending(reading => reading.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Reading>> GetAllWithTowersAsync(CancellationToken cancellationToken)
    {
        return dbContext.Readings
            .AsNoTracking()
            .Include(reading => reading.Tower)
            .Where(reading => reading.RecordedAt.Year == reportingYear)
            .OrderBy(reading => reading.RecordedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Reading>> GetRecentByTowerAsync(
        int towerId,
        int count,
        CancellationToken cancellationToken
    )
    {
        return dbContext.Readings
            .AsNoTracking()
            .Where(reading => reading.TowerId == towerId
                && reading.RecordedAt.Year == reportingYear)
            .OrderByDescending(reading => reading.RecordedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task<Reading?> GetByIdAsync(
        int id,
        bool trackChanges,
        CancellationToken cancellationToken
    )
    {
        var query = trackChanges
            ? dbContext.Readings.AsQueryable()
            : dbContext.Readings.AsNoTracking();

        return query.FirstOrDefaultAsync(
            reading => reading.Id == id && reading.RecordedAt.Year == reportingYear,
            cancellationToken
        );
    }

    public async Task AddAsync(Reading reading, CancellationToken cancellationToken)
    {
        await dbContext.Readings.AddAsync(reading, cancellationToken);
    }

    public void Remove(Reading reading)
    {
        dbContext.Readings.Remove(reading);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
