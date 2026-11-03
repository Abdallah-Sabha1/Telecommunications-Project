using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface ITowerService
{
    Task<List<TowerDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TowerDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TowerDto> CreateAsync(CreateTowerDto request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, UpdateTowerDto request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
