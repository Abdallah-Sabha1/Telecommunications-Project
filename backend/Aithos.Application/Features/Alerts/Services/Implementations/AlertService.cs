using Aithos.Application.Abstractions.Services;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Aithos.Application.Abstractions.Persistence;

namespace Aithos.Application.Features;

public sealed class AlertService(IAlertRepository alertRepository) : IAlertService
{
    public async Task<List<AlertDto>> GetAsync(string? severity, string sort, CancellationToken cancellationToken) =>
        (await alertRepository.GetAsync(severity, sort, cancellationToken)).Select(Map).ToList();

    public async Task<AlertDto?> UpdateStatusAsync(int id, AlertWorkflowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status)) return null;
        var alert = await GetByIdAsync(id, cancellationToken);
        if (alert is null) return null;

        var status = request.Status.Trim().ToLowerInvariant();
        if (status is not ("open" or "acknowledged" or "resolved"))
            throw new ArgumentException("Status must be open, acknowledged, or resolved.");

        alert.Status = status;
        alert.IsResolved = status == "resolved";
        if (status == "open")
        {
            alert.AcknowledgedAt = null;
            alert.ResolvedAt = null;
        }
        else if (status == "acknowledged")
        {
            alert.AcknowledgedAt ??= DateTime.UtcNow;
            alert.ResolvedAt = null;
        }
        else
        {
            alert.ResolvedAt = DateTime.UtcNow;
        }
        await alertRepository.SaveChangesAsync(cancellationToken);
        return Map(alert);
    }

    public async Task<AlertDto?> UpdateTowerStatusAsync(
        int towerId,
        AlertWorkflowRequest request,
        CancellationToken cancellationToken
    )
    {
        var alerts = await alertRepository.GetByTowerIdAsync(towerId, cancellationToken);
        if (alerts.Count == 0) return null;

        var status = ValidateStatus(request.Status);
        var now = DateTime.UtcNow;

        foreach (var alert in alerts)
        {
            ApplyStatus(alert, status, now);
        }

        await alertRepository.SaveChangesAsync(cancellationToken);
        return Map(alerts.First());
    }

    public async Task<AlertDto?> UpdateAssigneeAsync(int id, AlertAssigneeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AssignedTo)) throw new ArgumentException("Assignee is required.");
        var alert = await GetByIdAsync(id, cancellationToken);
        if (alert is null) return null;
        alert.AssignedTo = request.AssignedTo.Trim();
        if (alert.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
        {
            alert.Status = "acknowledged";
            alert.IsResolved = false;
            alert.AcknowledgedAt ??= DateTime.UtcNow;
            alert.ResolvedAt = null;
        }
        await alertRepository.SaveChangesAsync(cancellationToken);
        return Map(alert);
    }

    public async Task<AlertDto?> UpdateTowerAssigneeAsync(
        int towerId,
        AlertAssigneeRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.AssignedTo))
            throw new ArgumentException("Assignee is required.");

        var alerts = await alertRepository.GetByTowerIdAsync(towerId, cancellationToken);
        if (alerts.Count == 0) return null;

        var now = DateTime.UtcNow;
        foreach (var alert in alerts)
        {
            alert.AssignedTo = request.AssignedTo.Trim();
            if (alert.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                alert.Status = "acknowledged";
                alert.IsResolved = false;
                alert.AcknowledgedAt ??= now;
                alert.ResolvedAt = null;
            }
        }

        await alertRepository.SaveChangesAsync(cancellationToken);
        return Map(alerts.First());
    }

    private static string ValidateStatus(string? value)
    {
        var status = value?.Trim().ToLowerInvariant();
        if (status is not ("open" or "acknowledged" or "resolved"))
            throw new ArgumentException("Status must be open, acknowledged, or resolved.");
        return status;
    }

    private static void ApplyStatus(Alert alert, string status, DateTime now)
    {
        alert.Status = status;
        alert.IsResolved = status == "resolved";
        if (status == "open")
        {
            alert.AcknowledgedAt = null;
            alert.ResolvedAt = null;
        }
        else if (status == "acknowledged")
        {
            alert.AcknowledgedAt ??= now;
            alert.ResolvedAt = null;
        }
        else
        {
            alert.ResolvedAt ??= now;
        }
    }

    private Task<Alert?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return alertRepository.GetByIdAsync(id, cancellationToken);
    }

    private static AlertDto Map(Alert alert) => new()
    {
        Id = alert.Id,
        TowerId = alert.TowerId,
        Tower = alert.Tower?.Name ?? $"Tower {alert.TowerId}",
        Location = alert.Tower?.Location ?? "Unknown",
        Cluster = alert.Tower?.Group ?? "",
        Severity = alert.Severity,
        Status = alert.Status.ToLowerInvariant(),
        Confidence = alert.Confidence,
        Message = alert.Message,
        EvidenceSummary = alert.EvidenceSummary,
        EstimatedFinancialLoss = alert.EstimatedFinancialLoss,
        EstimatedCarbonImpact = alert.EstimatedCarbonImpact,
        PossibleCause = alert.PossibleCause,
        Classification = alert.PossibleCause,
        Recommendation = alert.Recommendation,
        CreatedAt = alert.CreatedAt,
        AssignedTo = alert.AssignedTo,
        AcknowledgedAt = alert.AcknowledgedAt,
        ResolvedAt = alert.ResolvedAt,
    };
}
