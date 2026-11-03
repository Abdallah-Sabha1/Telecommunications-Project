using Aithos.Application.Abstractions.Persistence;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class AiAnalysisResultRepository(AithosDbContext db) : IAiAnalysisResultRepository
{
    public async Task<IReadOnlyList<AiAnalysisResult>> GetAsync(
        string? siteId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = db.AiAnalysisResults.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(siteId))
            query = query.Where(result => result.SiteId == siteId);
        if (from.HasValue)
            query = query.Where(result => result.ObservationDate >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(result => result.ObservationDate <= to.Value.Date);

        return await query
            .OrderByDescending(result => result.ObservationDate)
            .ToListAsync(cancellationToken);
    }
}
