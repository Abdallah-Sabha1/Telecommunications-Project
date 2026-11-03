using Aithos.Infrastructure.Persistence;
using Aithos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aithos.Infrastructure.Persistence.Repositories;

public sealed class TowerAnalysisRepository(AithosDbContext dbContext)
    : ITowerAnalysisRepository
{
    public Task<List<TowerAnalysis>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken
    )
    {
        return dbContext.TowerAnalyses
            .AsNoTracking()
            .Include(analysis => analysis.Tower)
            .OrderByDescending(analysis => analysis.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task<List<TowerAnalysis>> GetRecentByTowerAsync(
        int towerId,
        int count,
        CancellationToken cancellationToken
    )
    {
        return dbContext.TowerAnalyses
            .AsNoTracking()
            .Where(analysis => analysis.TowerId == towerId)
            .OrderByDescending(analysis => analysis.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAnalysisAsync(
        Alert alert,
        TowerAnalysis analysis,
        CancellationToken cancellationToken
    )
    {
        var existingAnalysis = await dbContext.TowerAnalyses
            .FirstOrDefaultAsync(item => item.TowerId == analysis.TowerId
                && item.TelemetryTimestampUtc == analysis.TelemetryTimestampUtc, cancellationToken);

        if (existingAnalysis is null)
        {
            await dbContext.TowerAnalyses.AddAsync(analysis, cancellationToken);
        }
        else
        {
            existingAnalysis.Status = analysis.Status;
            existingAnalysis.Severity = analysis.Severity;
            existingAnalysis.Confidence = analysis.Confidence;
            existingAnalysis.ExecutiveSummary = analysis.ExecutiveSummary;
            existingAnalysis.EvidenceJson = analysis.EvidenceJson;
            existingAnalysis.KeyFindingsJson = analysis.KeyFindingsJson;
            existingAnalysis.PossibleCausesJson = analysis.PossibleCausesJson;
            existingAnalysis.OperationalImpact = analysis.OperationalImpact;
            existingAnalysis.EstimatedFinancialLoss = analysis.EstimatedFinancialLoss;
            existingAnalysis.EstimatedCarbonImpact = analysis.EstimatedCarbonImpact;
            existingAnalysis.RecommendedActionsJson = analysis.RecommendedActionsJson;
            existingAnalysis.Engine = analysis.Engine;
            existingAnalysis.Model = analysis.Model;
            existingAnalysis.CreatedAt = DateTime.UtcNow;
        }

        var existingAlert = await dbContext.Alerts
            .Where(item => item.TowerId == alert.TowerId
                && item.CreatedAt.Date == alert.CreatedAt.Date)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAlert is null)
        {
            await dbContext.Alerts.AddAsync(alert, cancellationToken);
        }
        else
        {
            var status = existingAlert.Status;
            var assignedTo = existingAlert.AssignedTo;
            var acknowledgedAt = existingAlert.AcknowledgedAt;
            var resolvedAt = existingAlert.ResolvedAt;
            existingAlert.Message = alert.Message;
            existingAlert.Severity = alert.Severity;
            existingAlert.Confidence = alert.Confidence;
            existingAlert.EvidenceSummary = alert.EvidenceSummary;
            existingAlert.EstimatedFinancialLoss = alert.EstimatedFinancialLoss;
            existingAlert.EstimatedCarbonImpact = alert.EstimatedCarbonImpact;
            existingAlert.PossibleCause = alert.PossibleCause;
            existingAlert.Recommendation = alert.Recommendation;
            existingAlert.Status = status;
            existingAlert.IsResolved = status.Equals("resolved", StringComparison.OrdinalIgnoreCase);
            existingAlert.AssignedTo = assignedTo;
            existingAlert.AcknowledgedAt = acknowledgedAt;
            existingAlert.ResolvedAt = resolvedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
