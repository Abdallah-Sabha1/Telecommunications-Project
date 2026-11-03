namespace Aithos.Application.Contracts.Responses;

public record ReadingDto(
    int Id,
    int TowerId,
    decimal? PowerOutput,
    decimal ElectricityConsumption,
    decimal Temperature,
    decimal TrafficLoad,
    decimal? WindSpeed,
    string GeneratorStatus,
    string GridStatus,
    DateTime RecordedAt
);
