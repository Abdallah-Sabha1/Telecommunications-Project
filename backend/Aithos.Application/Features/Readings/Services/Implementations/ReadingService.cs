using Aithos.Application.Abstractions.Services;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Aithos.Application.Abstractions.Persistence;

namespace Aithos.Application.Features;

public sealed class ReadingService(
    IReadingRepository readingRepository,
    ITowerRepository towerRepository
) : IReadingService
{
    public async Task<List<ReadingDto>> GetAllAsync(int? towerId, CancellationToken cancellationToken)
    {
        var readings = await readingRepository.GetAllAsync(towerId, cancellationToken);

        return readings.Select(MapReading).ToList();
    }

    public async Task<ReadingDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var reading = await readingRepository.GetByIdAsync(id, false, cancellationToken);

        return reading is null ? null : MapReading(reading);
    }

    public async Task<ReadingDto?> CreateAsync(CreateReadingDto request, CancellationToken cancellationToken)
    {
        var towerExists = await towerRepository.ExistsAsync(request.TowerId, cancellationToken);
        if (!towerExists)
        {
            return null;
        }

        var reading = new Reading
        {
            TowerId = request.TowerId,
            PowerOutput = request.PowerOutput,
            ElectricityConsumption = request.ElectricityConsumption,
            Temperature = request.Temperature,
            TrafficLoad = request.TrafficLoad,
            WindSpeed = request.WindSpeed,
            GeneratorStatus = request.GeneratorStatus,
            GridStatus = request.GridStatus,
            RecordedAt = request.RecordedAt ?? DateTime.UtcNow,
        };

        await readingRepository.AddAsync(reading, cancellationToken);
        await readingRepository.SaveChangesAsync(cancellationToken);
        return MapReading(reading);
    }

    public async Task<bool> UpdateAsync(int id, UpdateReadingDto request, CancellationToken cancellationToken)
    {
        var reading = await readingRepository.GetByIdAsync(id, true, cancellationToken);

        if (reading is null)
        {
            return false;
        }

        reading.PowerOutput = request.PowerOutput;
        reading.ElectricityConsumption = request.ElectricityConsumption;
        reading.Temperature = request.Temperature;
        reading.TrafficLoad = request.TrafficLoad;
        reading.WindSpeed = request.WindSpeed;
        reading.GeneratorStatus = request.GeneratorStatus;
        reading.GridStatus = request.GridStatus;
        reading.RecordedAt = request.RecordedAt;

        await readingRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var reading = await readingRepository.GetByIdAsync(id, true, cancellationToken);

        if (reading is null)
        {
            return false;
        }

        readingRepository.Remove(reading);
        await readingRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ReadingDto MapReading(Reading reading)
    {
        return new ReadingDto(
            Id: reading.Id,
            TowerId: reading.TowerId,
            PowerOutput: reading.PowerOutput,
            ElectricityConsumption: reading.ElectricityConsumption,
            Temperature: reading.Temperature,
            TrafficLoad: reading.TrafficLoad,
            WindSpeed: reading.WindSpeed,
            GeneratorStatus: reading.GeneratorStatus,
            GridStatus: reading.GridStatus,
            RecordedAt: reading.RecordedAt
        );
    }
}
