namespace Aithos.Application.Contracts.Responses;

public sealed class AlertDto
{
    public int Id { get; set; }

    public int TowerId { get; set; }

    public string Tower { get; set; } = "";

    public string Location { get; set; } = "";

    public string Cluster { get; set; } = "";

    public string Severity { get; set; } = "";

    public string Status { get; set; } = "";

    public int Confidence { get; set; }

    public string Message { get; set; } = "";

    public string EvidenceSummary { get; set; } = "";

    public string EstimatedFinancialLoss { get; set; } = "";

    public string EstimatedCarbonImpact { get; set; } = "";

    public string PossibleCause { get; set; } = "";

    public string Classification { get; set; } = "";

    public string Recommendation { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public string Summary
    {
        get => Message;
        set => Message = value;
    }

    public string Evidence
    {
        get => EvidenceSummary;
        set => EvidenceSummary = value;
    }

    public string FinancialLoss
    {
        get => EstimatedFinancialLoss;
        set => EstimatedFinancialLoss = value;
    }

    public string CarbonImpact
    {
        get => EstimatedCarbonImpact;
        set => EstimatedCarbonImpact = value;
    }

    public string RecommendedAction
    {
        get => Recommendation;
        set => Recommendation = value;
    }

    public string? AssignedTo { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }
}
