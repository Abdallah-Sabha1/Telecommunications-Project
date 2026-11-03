using System.Globalization;
using System.Text;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;
using Aithos.Domain.Import;
using Aithos.Application.Abstractions.Persistence;
using Microsoft.VisualBasic.FileIO;

namespace Aithos.Infrastructure.Imports;

public sealed class DatasetImportService(
    IDatasetRepository datasetRepository,
    ILogger<DatasetImportService> logger,
    IConfiguration configuration
) : IDatasetImportService
{
    private readonly int _reportingYear = configuration.GetValue("Data:ReportingYear", 2025);

    public async Task<DatasetImportResult> ImportAsync(
        string datasetDirectory,
        CancellationToken cancellationToken = default
    )
    {
        var startedAt = DateTime.UtcNow;
        var errors = new List<DatasetImportError>();

        if (string.IsNullOrWhiteSpace(datasetDirectory))
        {
            return Failure(startedAt, "Dataset directory is required.");
        }

        var towersPath = Path.Combine(datasetDirectory, "towers.csv");
        var readingsPath = Path.Combine(datasetDirectory, "daily_data.csv");
        var pricesPath = Path.Combine(datasetDirectory, "prices.csv");

        CheckFile(towersPath, "towers.csv", errors);
        CheckFile(readingsPath, "daily_data.csv", errors);
        CheckFile(pricesPath, "prices.csv", errors);

        if (errors.Count > 0)
        {
            return Failure(startedAt, errors);
        }

        try
        {
            var towerRows = ReadCsv(towersPath, "towers.csv");
            var readingRows = ReadCsv(readingsPath, "daily_data.csv");
            var priceRows = ReadCsv(pricesPath, "prices.csv");

            var towers = ParseTowers(towerRows, errors);
            var allReadings = ParseReadings(readingRows, errors);
            var allPrices = ParsePrices(priceRows, errors);
            var skippedNonReportingYear = allReadings.Count(row => row.Date.Year != _reportingYear)
                + allPrices.Count(row => row.Month.Year != _reportingYear);
            var readings = allReadings
                .Where(row => row.Date.Year == _reportingYear)
                .ToList();
            var prices = allPrices
                .Where(row => row.Month.Year == _reportingYear)
                .ToList();

            CheckTowerReferences(towers, readings, errors);

            if (errors.Count > 0)
            {
                return Failure(startedAt, errors);
            }

            await datasetRepository.SaveAsync(
                new DatasetImportData(towers, readings, prices),
                cancellationToken
            );

            logger.LogInformation(
                "Dataset imported for reporting year {Year}. Towers={Towers}, Readings={Readings}, Prices={Prices}",
                _reportingYear,
                towers.Count,
                readings.Count,
                prices.Count
            );

            return new DatasetImportResult
            {
                Success = true,
                TowersImported = towers.Count,
                DailyTelemetryImported = readings.Count,
                PricesImported = prices.Count,
                NonReportingYearRecordsSkipped = skippedNonReportingYear,
                ProcessingDuration = DateTime.UtcNow - startedAt,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Dataset import failed.");
            return Failure(startedAt, "Dataset import failed. No partial data was saved.");
        }
    }

    private static List<TowerImportRow> ParseTowers(
        CsvFile file,
        ICollection<DatasetImportError> errors
    )
    {
        var requiredColumns = new[]
        {
            "site_id", "site_name", "latitude", "longitude", "group",
            "base_load_kwh", "cooling_coef", "traffic_coef",
        };

        if (!CheckColumns(file, requiredColumns, errors))
        {
            return [];
        }

        var result = new List<TowerImportRow>();
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in file.Rows)
        {
            if (!TryText(row, "site_id", errors, out var siteId)
                || !TryText(row, "site_name", errors, out var siteName)
                || !TryDecimal(row, "latitude", errors, out var latitude)
                || !TryDecimal(row, "longitude", errors, out var longitude)
                || !TryText(row, "group", errors, out var group)
                || !TryDecimal(row, "base_load_kwh", errors, out var baseLoad)
                || !TryDecimal(row, "cooling_coef", errors, out var coolingCoefficient)
                || !TryDecimal(row, "traffic_coef", errors, out var trafficCoefficient))
            {
                continue;
            }

            if (!keys.Add(siteId))
            {
                AddError(row, errors, $"Duplicate site_id '{siteId}'.");
                continue;
            }

            result.Add(new TowerImportRow(
                siteId,
                siteName,
                latitude,
                longitude,
                group,
                baseLoad,
                coolingCoefficient,
                trafficCoefficient
            ));
        }

        return result;
    }

    private static List<ReadingImportRow> ParseReadings(
        CsvFile file,
        ICollection<DatasetImportError> errors
    )
    {
        var requiredColumns = new[]
        {
            "site_id", "date", "temp_avg", "cdd", "traffic_avg_pct",
            "traffic_peak_pct", "outage_hours", "grid_kwh",
        };

        if (!CheckColumns(file, requiredColumns, errors))
        {
            return [];
        }

        var result = new List<ReadingImportRow>();
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in file.Rows)
        {
            if (!TryText(row, "site_id", errors, out var siteId)
                || !TryDate(row, "date", errors, out var date)
                || !TryDecimal(row, "temp_avg", errors, out var temperature)
                || !TryDecimal(row, "cdd", errors, out var cdd)
                || !TryDecimal(row, "traffic_avg_pct", errors, out var trafficAverage)
                || !TryDecimal(row, "traffic_peak_pct", errors, out var trafficPeak)
                || !TryDecimal(row, "outage_hours", errors, out var outageHours)
                || !TryDecimal(row, "grid_kwh", errors, out var gridKwh))
            {
                continue;
            }

            var key = $"{siteId}|{date:yyyy-MM-dd}";
            if (!keys.Add(key))
            {
                AddError(row, errors, $"Duplicate reading for '{siteId}' on {date:yyyy-MM-dd}.");
                continue;
            }

            result.Add(new ReadingImportRow(
                siteId,
                date,
                temperature,
                cdd,
                trafficAverage,
                trafficPeak,
                outageHours,
                gridKwh
            ));
        }

        return result;
    }

    private static List<PriceImportRow> ParsePrices(
        CsvFile file,
        ICollection<DatasetImportError> errors
    )
    {
        var requiredColumns = new[]
        {
            "month", "electricity_price_per_kwh",
        };

        if (!CheckColumns(file, requiredColumns, errors))
        {
            return [];
        }

        var result = new List<PriceImportRow>();
        var months = new HashSet<DateTime>();

        foreach (var row in file.Rows)
        {
            if (!TryDate(row, "month", errors, out var month)
                || !TryDecimal(row, "electricity_price_per_kwh", errors, out var electricityPrice))
            {
                continue;
            }

            month = new DateTime(month.Year, month.Month, 1);
            if (!months.Add(month))
            {
                AddError(row, errors, $"Duplicate price for {month:yyyy-MM}.");
                continue;
            }

            result.Add(new PriceImportRow(month, electricityPrice));
        }

        return result;
    }

    private static void CheckTowerReferences(
        IReadOnlyCollection<TowerImportRow> towers,
        IReadOnlyCollection<ReadingImportRow> readings,
        ICollection<DatasetImportError> errors
    )
    {
        var towerIds = towers
            .Select(tower => tower.SiteId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var reading in readings.Where(reading => !towerIds.Contains(reading.SiteId)))
        {
            errors.Add(new DatasetImportError
            {
                FileName = "daily_data.csv",
                Category = "Relationship",
                Message = $"Unknown site_id '{reading.SiteId}'.",
            });
        }
    }

    private static CsvFile ReadCsv(string path, string fileName)
    {
        using var parser = new TextFieldParser(path, Encoding.UTF8)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true,
        };
        parser.SetDelimiters(",");

        var headers = parser.ReadFields() ?? [];
        var rows = new List<CsvRow>();
        var rowNumber = 1;

        while (!parser.EndOfData)
        {
            rowNumber++;
            var values = parser.ReadFields() ?? [];
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < headers.Length; index++)
            {
                fields[headers[index]] = index < values.Length ? values[index] : "";
            }

            rows.Add(new CsvRow(fileName, rowNumber, fields));
        }

        return new CsvFile(fileName, headers, rows);
    }

    private static bool CheckColumns(
        CsvFile file,
        IEnumerable<string> requiredColumns,
        ICollection<DatasetImportError> errors
    )
    {
        var headers = file.Headers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingColumns = requiredColumns.Where(column => !headers.Contains(column)).ToList();

        foreach (var column in missingColumns)
        {
            errors.Add(new DatasetImportError
            {
                FileName = file.Name,
                Category = "Structure",
                Message = $"Missing required column '{column}'.",
            });
        }

        if (file.Rows.Count == 0)
        {
            errors.Add(new DatasetImportError
            {
                FileName = file.Name,
                Category = "Structure",
                Message = "File contains no data rows.",
            });
        }

        return missingColumns.Count == 0 && file.Rows.Count > 0;
    }

    private static bool TryText(
        CsvRow row,
        string column,
        ICollection<DatasetImportError> errors,
        out string value
    )
    {
        value = row.Fields.GetValueOrDefault(column)?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        AddError(row, errors, $"Missing value for '{column}'.");
        return false;
    }

    private static bool TryDecimal(
        CsvRow row,
        string column,
        ICollection<DatasetImportError> errors,
        out decimal value
    )
    {
        var text = row.Fields.GetValueOrDefault(column);
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        AddError(row, errors, $"Invalid number in '{column}'.");
        return false;
    }

    private static bool TryDate(
        CsvRow row,
        string column,
        ICollection<DatasetImportError> errors,
        out DateTime value
    )
    {
        var text = row.Fields.GetValueOrDefault(column);
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
        {
            value = value.Date;
            return true;
        }

        AddError(row, errors, $"Invalid date in '{column}'.");
        return false;
    }

    private static bool TryBoolean(
        CsvRow row,
        string column,
        ICollection<DatasetImportError> errors,
        out bool value
    )
    {
        var text = row.Fields.GetValueOrDefault(column);

        if (text is "0" or "1")
        {
            value = text == "1";
            return true;
        }

        if (bool.TryParse(text, out value))
        {
            return true;
        }

        AddError(row, errors, $"Invalid true/false value in '{column}'.");
        return false;
    }

    private static void CheckFile(
        string path,
        string fileName,
        ICollection<DatasetImportError> errors
    )
    {
        if (!File.Exists(path))
        {
            errors.Add(new DatasetImportError
            {
                FileName = fileName,
                Category = "File",
                Message = "Required file not found.",
            });
        }
    }

    private static void AddError(
        CsvRow row,
        ICollection<DatasetImportError> errors,
        string message
    )
    {
        errors.Add(new DatasetImportError
        {
            FileName = row.FileName,
            RowNumber = row.RowNumber,
            Category = "Structure",
            Message = message,
        });
    }

    private static DatasetImportResult Failure(DateTime startedAt, string message)
    {
        return Failure(startedAt,
        [
            new DatasetImportError { Category = "Import", Message = message },
        ]);
    }

    private static DatasetImportResult Failure(
        DateTime startedAt,
        IEnumerable<DatasetImportError> errors
    )
    {
        var errorList = errors.ToList();
        return new DatasetImportResult
        {
            Success = false,
            RejectedRecords = errorList.Count,
            ValidationErrors = errorList,
            ProcessingDuration = DateTime.UtcNow - startedAt,
        };
    }

    private sealed record CsvFile(string Name, string[] Headers, List<CsvRow> Rows);

    private sealed record CsvRow(
        string FileName,
        int RowNumber,
        Dictionary<string, string> Fields
    );
}
