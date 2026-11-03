using Aithos.Application.Contracts.Responses;
using Aithos.Application.Abstractions.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Aithos.Infrastructure.Reporting;

public sealed class CarbonIntelligenceService(
    IReadingRepository readingRepository,
    ITowerRepository towerRepository,
    IConfiguration configuration
) : ICarbonIntelligenceService
{
    private readonly int reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    public async Task<CarbonReportDto> GetReportAsync(CancellationToken ct = default)
    {
        var readings = (await readingRepository.GetAllWithTowersAsync(ct))
            .Where(reading => reading.RecordedAt.Year == reportingYear)
            .ToList();
        var towers = await towerRepository.GetAllAsync(ct);
        var factor = configuration.GetValue<decimal?>("Carbon:EmissionFactorKgCo2ePerKwh") ?? 0.4m;
        var evaluated = (configuration.GetValue<bool?>("Carbon:EmissionFactorApproved") ?? true) && factor > 0;
        var ordered = readings.OrderBy(r => r.RecordedAt).ToList();
        var actual = ordered.Sum(r => r.ElectricityConsumption);
        decimal? Calculate(decimal kwh) => evaluated ? decimal.Round(kwh * factor, 2) : null;

        var report = new CarbonReportDto
        {
            TotalCo2Kg = Calculate(actual),
            ActualElectricityKwh = decimal.Round(actual, 2),
            EmissionFactorKgCo2ePerKwh = evaluated ? factor : null,
            CarbonEvaluated = evaluated,
            ReportingPeriod = ordered.Count == 0 ? "No data" : $"{ordered.First().RecordedAt:dd MMM yyyy} – {ordered.Last().RecordedAt:dd MMM yyyy}",
            TotalTowers = towers.Count,
            TotalReadings = ordered.Count,
            TopContributors = ordered.GroupBy(r => new { Name = r.Tower?.Name ?? $"Tower {r.TowerId}", Location = r.Tower?.Location ?? "Unknown" })
                .Select(g => new CarbonContributorDto { TowerName = g.Key.Name, Location = g.Key.Location, ElectricityKwh = decimal.Round(g.Sum(x => x.ElectricityConsumption), 2), TotalKg = Calculate(g.Sum(x => x.ElectricityConsumption)) })
                .OrderByDescending(x => x.ElectricityKwh).Take(5).ToList(),
            MonthlyTrend = ordered.GroupBy(r => new { r.RecordedAt.Year, r.RecordedAt.Month }).OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new CarbonTrendPointDto { Label = $"{g.Key.Year}-{g.Key.Month:00}", ElectricityKwh = decimal.Round(g.Sum(x => x.ElectricityConsumption), 2), TotalKg = Calculate(g.Sum(x => x.ElectricityConsumption)) }).TakeLast(12).ToList()
        };
        report.Narrative = evaluated
            ? new CarbonNarrativeDto { ExecutiveSummary = $"Estimated emissions from {report.ActualElectricityKwh:N0} kWh of purchased electricity.", Interpretation = "The estimate uses the approved electricity emissions factor configured for this deployment.", Recommendations = ["Review the highest-consuming sites and compare them with their operating baseline."] }
            : new CarbonNarrativeDto { ExecutiveSummary = "Carbon is not evaluated yet.", Interpretation = "An approved electricity emissions factor is required before publishing a CO2e value.", Recommendations = ["Confirm the approved Jordan grid emissions factor, then enable Carbon:EmissionFactorApproved."] };
        return report;
    }

    public async Task<byte[]> GeneratePdfAsync(CancellationToken ct = default)
    {
        var report = await GetReportAsync(ct);
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(36); page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
            page.Header().Text("Carbon report").FontSize(22).Bold().FontColor("#111111");
            page.Content().PaddingTop(18).Column(column =>
            {
                column.Spacing(12);
                column.Item().Text(report.ReportingPeriod).FontColor("#555555");
                column.Item().Text(report.CarbonEvaluated ? $"Total estimated CO2e: {report.TotalCo2Kg:N2} kg" : "Carbon evaluation pending approved emissions factor").FontSize(16).Bold();
                column.Item().Text($"Electricity consumption: {report.ActualElectricityKwh:N2} kWh");
                column.Item().Text(report.Narrative.Interpretation);
                column.Item().Text("Top consuming sites").FontSize(14).Bold();
                foreach (var item in report.TopContributors) column.Item().Text($"{item.TowerName} · {item.ElectricityKwh:N2} kWh");
            });
        })).GeneratePdf();
    }
}
