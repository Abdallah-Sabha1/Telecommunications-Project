using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IDatasetImportService
{
    Task<DatasetImportResult> ImportAsync(
        string datasetDirectory,
        CancellationToken cancellationToken = default
    );
}
