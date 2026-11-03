namespace Aithos.Domain.Entities;

public class Alert
{
    public int Id { get; set; }
    public int TowerId { get; set; }
    public Tower? Tower { get; set; }
    public string Message { get; set; } = "";
    public string Severity { get; set; } = "Info";
    public string Status { get; set; } = "open";
    public int Confidence { get; set; }
    public string EvidenceSummary { get; set; } = "";
    public string EstimatedFinancialLoss { get; set; } = "";
    public string EstimatedCarbonImpact { get; set; } = "";
    public string PossibleCause { get; set; } = "";
    public string Recommendation { get; set; } = "";
    public string? AssignedTo { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
