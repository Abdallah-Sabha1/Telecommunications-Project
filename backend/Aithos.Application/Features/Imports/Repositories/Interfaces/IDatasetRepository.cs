using Aithos.Domain.Import;

namespace Aithos.Application.Abstractions.Persistence;

public interface IDatasetRepository
{
    Task SaveAsync(DatasetImportData dataset, CancellationToken cancellationToken);
}
