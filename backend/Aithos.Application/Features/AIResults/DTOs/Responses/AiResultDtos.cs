namespace Aithos.Application.Contracts.Responses;

public sealed record AiResultDto(
    string SiteId,
    DateTime Date,
    decimal ActualKwh,
    decimal ExpectedKwh,
    decimal Residual,
    decimal ScoreZ,
    string Classification,
    decimal? Slope,
    string Reason,
    decimal WastedKwh,
    decimal WastedCostJod,
    decimal? CarbonImpactKgCo2e,
    bool CarbonEvaluated,
    string ModelVersion
);
