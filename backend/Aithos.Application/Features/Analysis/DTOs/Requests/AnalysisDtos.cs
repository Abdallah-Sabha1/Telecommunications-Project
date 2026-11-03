namespace Aithos.Application.Contracts.Requests;

public record AnalysisRequest(
    int TowerId,
    string SiteId,
    string TowerName,
    string Location,
    string Group,
    decimal Latitude,
    decimal Longitude,
    decimal BaseLoadKwh,
    decimal CoolingCoef,
    decimal TrafficCoef,
    decimal WeatherTemperature,
    string WeatherSource,
    DateTime RecordedAt,
    List<AnalysisReadingSnapshot> RecentReadings
);
public record AnalysisReadingSnapshot(
    int Id,
    DateTime RecordedAt,
    decimal? PowerOutput,
    decimal ElectricityConsumption,
    decimal Temperature,
    decimal Cdd,
    decimal TrafficLoad,
    decimal TrafficPeakPct,
    decimal OutageHours,
    decimal? WindSpeed,
    string GeneratorStatus,
    string GridStatus
);

