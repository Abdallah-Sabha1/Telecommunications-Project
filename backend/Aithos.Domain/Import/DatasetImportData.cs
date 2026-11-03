namespace Aithos.Domain.Import;

public sealed record TowerImportRow(
    string SiteId,
    string SiteName,
    decimal Latitude,
    decimal Longitude,
    string Group,
    decimal BaseLoadKwh,
    decimal CoolingCoef,
    decimal TrafficCoef
);

public sealed record ReadingImportRow(
    string SiteId,
    DateTime Date,
    decimal Temperature,
    decimal Cdd,
    decimal TrafficAveragePct,
    decimal TrafficPeakPct,
    decimal OutageHours,
    decimal GridKwh
);

public sealed record PriceImportRow(
    DateTime Month,
    decimal ElectricityPricePerKwh
);

public sealed record DatasetImportData(
    IReadOnlyCollection<TowerImportRow> Towers,
    IReadOnlyCollection<ReadingImportRow> Readings,
    IReadOnlyCollection<PriceImportRow> Prices
);
