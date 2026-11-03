namespace Aithos.Domain.Entities;

public class Reading
{
    public int Id { get; set; }

    public int TowerId { get; set; }

    public Tower? Tower { get; set; }

    public decimal? PowerOutput { get; set; }

    public decimal ElectricityConsumption { get; set; }

    public decimal Temperature { get; set; }

    public decimal Cdd { get; set; }

    public decimal TrafficLoad { get; set; }

    public decimal TrafficPeakPct { get; set; }

    public decimal OutageHours { get; set; }

    public decimal? WindSpeed { get; set; }

    public string GeneratorStatus { get; set; } = "Unknown";

    public string GridStatus { get; set; } = "Unknown";

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
