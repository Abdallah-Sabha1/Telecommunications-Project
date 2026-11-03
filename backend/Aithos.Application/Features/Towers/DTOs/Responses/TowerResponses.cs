namespace Aithos.Application.Contracts.Responses;

public record TowerDto(
    int Id,
    string Name,
    string Location,
    string Status,
    DateTime CreatedAt,
    string? SiteCode = null,
    string? Region = null,
    string? Group = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    decimal? BaseLoadKwh = null,
    decimal? CoolingCoef = null,
    decimal? TrafficCoef = null
);
