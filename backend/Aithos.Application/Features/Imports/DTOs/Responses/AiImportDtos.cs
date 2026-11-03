namespace Aithos.Application.Contracts.Responses;

public sealed class AiImportResult
{
    public bool Success { get; init; }
    public int ResultsImported { get; init; }
    public int AlertsCreated { get; init; }
    public int DieselRecordsSkipped { get; init; }
    public int NonReportingYearRecordsSkipped { get; init; }
    public List<string> Errors { get; init; } = [];
}
