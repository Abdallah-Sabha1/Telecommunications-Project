namespace Aithos.Application.Contracts.Requests;

public record CreateReadingDto(
    int TowerId,
    decimal? PowerOutput,
    decimal ElectricityConsumption,
    decimal Temperature,
    decimal TrafficLoad,
    decimal? WindSpeed,
    string GeneratorStatus,
    string GridStatus,
    DateTime? RecordedAt
);

public record UpdateReadingDto(
    decimal? PowerOutput,
    decimal ElectricityConsumption,
    decimal Temperature,
    decimal TrafficLoad,
    decimal? WindSpeed,
    string GeneratorStatus,
    string GridStatus,
    DateTime RecordedAt
);
