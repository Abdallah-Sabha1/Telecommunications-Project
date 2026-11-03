using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IReadingService
{
    Task<List<ReadingDto>> GetAllAsync(int? towerId, CancellationToken cancellationToken);
    Task<ReadingDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ReadingDto?> CreateAsync(CreateReadingDto request, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, UpdateReadingDto request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
