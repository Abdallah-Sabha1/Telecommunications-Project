namespace Aithos.Domain.Entities;

public class Tower
{
    public int Id { get; set; }

    public string SiteId { get; set; } = "";

    public string Name { get; set; } = "";

    public string Location { get; set; } = "";

    public decimal Latitude { get; set; } = 31.95m;

    public decimal Longitude { get; set; } = 35.91m;

    public string Group { get; set; } = "";

    public decimal BaseLoadKwh { get; set; }

    public decimal CoolingCoef { get; set; }

    public decimal TrafficCoef { get; set; }

    public string Status { get; set; } = "Operational";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Reading> Readings { get; set; } = [];

    public List<Alert> Alerts { get; set; } = [];

    public List<TowerAnalysis> Analyses { get; set; } = [];
}
