using Aithos.Domain.Entities;

namespace Aithos.Application.Abstractions.Persistence;

public interface IMonthlyPriceRepository
{
    Task<List<MonthlyPrice>> GetAllAsync(CancellationToken cancellationToken);
}
