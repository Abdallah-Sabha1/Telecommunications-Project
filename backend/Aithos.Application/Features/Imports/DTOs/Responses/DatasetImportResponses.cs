namespace Aithos.Application.Contracts.Responses;

public sealed class DatasetImportResult
{
    public bool Success { get; set; }
    public int TowersImported { get; set; }
    public int DailyTelemetryImported { get; set; }
    public int PricesImported { get; set; }
    public int NonReportingYearRecordsSkipped { get; set; }
    public int RejectedRecords { get; set; }
    public List<DatasetImportError> ValidationErrors { get; set; } = [];
    public TimeSpan ProcessingDuration { get; set; }
}

public sealed class DatasetImportError
{
    public string FileName { get; set; } = "";
    public int RowNumber { get; set; }
    public string Category { get; set; } = "Validation";
    public string Message { get; set; } = "";
}
