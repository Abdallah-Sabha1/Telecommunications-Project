using Aithos.Application.Abstractions.Services;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Entities;
using Aithos.Application.Abstractions.Persistence;

namespace Aithos.Application.Features;

public sealed class TowerService(ITowerRepository towerRepository) : ITowerService
{
    public async Task<List<TowerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var towers = await towerRepository.GetAllAsync(cancellationToken);

        return towers.Select(MapTower).ToList();
    }

    public async Task<TowerDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var tower = await towerRepository.GetByIdAsync(id, false, cancellationToken);

        return tower is null ? null : MapTower(tower);
    }

    public async Task<TowerDto> CreateAsync(CreateTowerDto request, CancellationToken cancellationToken)
    {
        var tower = new Tower
        {
            SiteId = request.SiteCode.Trim(),
            Name = request.Name,
            Location = request.Location,
            Status = request.Status ?? "Operational",
        };

        await towerRepository.AddAsync(tower, cancellationToken);
        await towerRepository.SaveChangesAsync(cancellationToken);
        return MapTower(tower);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTowerDto request, CancellationToken cancellationToken)
    {
        var tower = await towerRepository.GetByIdAsync(id, true, cancellationToken);

        if (tower is null)
        {
            return false;
        }

        tower.Name = request.Name;
        tower.Location = request.Location;
        tower.Status = request.Status;

        await towerRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var tower = await towerRepository.GetByIdAsync(id, true, cancellationToken);

        if (tower is null)
        {
            return false;
        }

        towerRepository.Remove(tower);
        await towerRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static TowerDto MapTower(Tower tower)
    {
        return new TowerDto(
            Id: tower.Id,
            Name: tower.Name,
            Location: tower.Location,
            Status: tower.Status,
            CreatedAt: tower.CreatedAt,
            SiteCode: tower.SiteId,
            Region: tower.Group,
            Group: tower.Group,
            Latitude: tower.Latitude,
            Longitude: tower.Longitude,
            BaseLoadKwh: tower.BaseLoadKwh,
            CoolingCoef: tower.CoolingCoef,
            TrafficCoef: tower.TrafficCoef
        );
    }
}
