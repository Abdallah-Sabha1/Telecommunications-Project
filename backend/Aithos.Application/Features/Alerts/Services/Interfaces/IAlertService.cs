using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Application.Abstractions.Services;

public interface IAlertService
{
    Task<List<AlertDto>> GetAsync(string? severity, string sort, CancellationToken cancellationToken);
    Task<AlertDto?> UpdateStatusAsync(int id, AlertWorkflowRequest request, CancellationToken cancellationToken);
    Task<AlertDto?> UpdateTowerStatusAsync(int towerId, AlertWorkflowRequest request, CancellationToken cancellationToken);
    Task<AlertDto?> UpdateAssigneeAsync(int id, AlertAssigneeRequest request, CancellationToken cancellationToken);
    Task<AlertDto?> UpdateTowerAssigneeAsync(int towerId, AlertAssigneeRequest request, CancellationToken cancellationToken);
}
