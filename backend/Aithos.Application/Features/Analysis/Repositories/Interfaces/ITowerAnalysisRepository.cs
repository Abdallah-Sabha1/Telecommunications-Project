using Aithos.Domain.Entities;

namespace Aithos.Application.Abstractions.Persistence;

public interface ITowerAnalysisRepository
{
    Task<List<TowerAnalysis>> GetRecentAsync(int count, CancellationToken cancellationToken);
    Task<List<TowerAnalysis>> GetRecentByTowerAsync(int towerId, int count, CancellationToken cancellationToken);
    Task SaveAnalysisAsync(Alert alert, TowerAnalysis analysis, CancellationToken cancellationToken);
}
