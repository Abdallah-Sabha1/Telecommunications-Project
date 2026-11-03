using Aithos.Infrastructure.Persistence;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class MonthlyPriceRepository(AithosDbContext dbContext)
    : IMonthlyPriceRepository
{
    public Task<List<MonthlyPrice>> GetAllAsync(CancellationToken cancellationToken)
    {
        return dbContext.MonthlyPrices
            .AsNoTracking()
            .OrderBy(price => price.Month)
            .ToListAsync(cancellationToken);
    }
}
