using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Aithos.Application.Contracts.Requests;
using Aithos.Application.Contracts.Responses;

namespace Aithos.Infrastructure.AI;

public sealed class ClaudeService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ClaudeService> logger
) : IClaudeService
{
    private readonly string _apiKey = configuration["Claude:ApiKey"] ?? "";
    private readonly string _model =
        configuration["Claude:Model"] ?? "claude-sonnet-4-6";

    public async Task<AnalysisResult> AnalyzeTowerAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new ClaudeApiException(
                "Claude is not configured. Add Claude:ApiKey to appsettings.json or user secrets."
            );
        }

        try
        {
            var analysisJson = await SendPromptAsync(
                systemPrompt: BuildSystemPrompt(),
                userPrompt: BuildUserPrompt(request),
                maxTokens: 3000,
                cancellationToken
            );
            var result = JsonSerializer.Deserialize<AnalysisResult>(
                analysisJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result is null)
            {
                throw new ClaudeApiException(
                    "Telecommunications Project Core AI returned an unreadable analysis."
                );
            }

            return NormalizeResult(result, request);
        }
        catch (ClaudeApiException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Claude returned invalid JSON.");
            throw new ClaudeApiException(
                "Claude returned incomplete or invalid JSON. Please try again."
            );
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Claude request failed.");
            throw new ClaudeApiException(
                "Unable to reach Telecommunications Project Core AI at this time."
            );
        }
    }

    private static string BuildSystemPrompt()
    {
        return """
You are Telecommunications Project Core AI, an industrial telecom tower analysis engine.

You are not a chatbot.
You are not a conversational assistant.
You produce concise, operationally grounded analysis for telecom tower engineers and operations leaders.

Rules:
- Use only the telemetry and context provided.
- Never invent missing telemetry.
- Never mention data that was not provided.
- Never conclude theft, fraud, or tampering directly unless explicit telemetry proves it.
- Derive the operational assessment from the raw telemetry window provided in the request.
- Compute comparisons, trends, spikes, and averages yourself from the supplied readings.
- Rank hypotheses by likelihood.
- Justify every conclusion using telemetry evidence.
- Keep outputs concise and engineering-focused.
- Return valid JSON only.
- Do not wrap JSON in Markdown.
- Return no more than 2 possible causes.
- Keep every string concise but informative; do not omit the evidence needed for an operational decision.
- Return no more than 6 evidence items, 4 key findings, and 4 recommended actions.
- Keep executiveSummary to 2-4 sentences and keep each evidence, finding, cause justification, and action to 1-2 sentences.
- Each recommended action must contain one clear operational step and, when available, an owner or timing signal separated by " · ".
- Never stop before closing all JSON objects and arrays.
- The final character must be }.
""";
    }

    private string BuildUserPrompt(AnalysisRequest request)
    {
        var readingsJson = JsonSerializer.Serialize(
            request.RecentReadings,
            new JsonSerializerOptions { WriteIndented = true }
        );

        return $$"""
Analyze the telecom tower operating snapshot and simulate the Telecommunications Project Core AI reasoning pipeline.

Tower context:
- Tower ID: {{request.TowerId}}
- Site ID: {{request.SiteId}}
- Tower name: {{request.TowerName}}
- Location: {{request.Location}}
- Group: {{request.Group}}
- Latitude: {{request.Latitude}}
- Longitude: {{request.Longitude}}
- Base load: {{request.BaseLoadKwh}} kWh
- Cooling coefficient: {{request.CoolingCoef}}
- Traffic coefficient: {{request.TrafficCoef}}
- Telemetry timestamp UTC: {{request.RecordedAt:O}}
- Weather source: {{request.WeatherSource}}
- Weather temperature: {{request.WeatherTemperature}} °C

Recent telemetry window (newest first):
{{readingsJson}}

Assessment rules:
1. Evaluate electricity, temperature, traffic load, grid state, and outage hours together.
2. Confidence must reflect the strength of the evidence, not certainty theater.
3. If evidence is weak, lower confidence.
4. Financial impact must be operational and conservative, not dramatic.
5. Carbon impact must reference energy use and emissions implications.
6. Executive summary must be brief and readable in under 15 seconds.
7. Evidence must be direct telemetry facts only; return at most 6 items.
8. Key findings must be short, practical statements; return at most 4 items.
9. Possible causes must be ranked with justification; return at most 2 causes.
10. Recommended actions must be actionable for operations teams; return at most 4 actions.
11. Keep each list item to 1-2 sentences so the result remains scannable without losing operational value.

Return ONLY JSON in this exact structure:
{
  "status": "Normal | Warning | Critical",
  "severity": "Low | Medium | High",
  "confidence": 0,
  "executiveSummary": "",
  "evidence": ["", ""],
  "keyFindings": ["", ""],
  "possibleCauses": [
    {
      "rank": 1,
      "hypothesis": "",
      "confidence": 0,
      "justification": ""
    }
  ],
  "operationalImpact": "",
  "estimatedFinancialLoss": "",
  "estimatedCarbonImpact": "",
  "recommendedActions": ["", ""],
  "metadata": {
    "analyzedAtUtc": "{{DateTime.UtcNow:O}}",
    "engine": "Telecommunications Project Core AI",
    "model": "{{_model}}",
    "weatherSource": "{{request.WeatherSource}}",
    "telemetryTimestampUtc": "{{request.RecordedAt:O}}"
  }
}
""";
    }

    private AnalysisResult NormalizeResult(
        AnalysisResult result,
        AnalysisRequest request
    )
    {
        var metadata = result.Metadata ?? new AnalysisMetadata();
        var possibleCauses = result.PossibleCauses
            .Where((cause) => !string.IsNullOrWhiteSpace(cause.Hypothesis))
            .OrderBy((cause) => cause.Rank)
            .ToList();

        var recommendedActions = result.RecommendedActions
            .Where((action) => !string.IsNullOrWhiteSpace(action))
            .ToList();

        var evidence = result.Evidence
            .Where((item) => !string.IsNullOrWhiteSpace(item))
            .ToList();

        var keyFindings = result.KeyFindings
            .Where((item) => !string.IsNullOrWhiteSpace(item))
            .ToList();

        if (possibleCauses.Count == 0 && !string.IsNullOrWhiteSpace(result.PossibleCause))
        {
            possibleCauses.Add(
                new AnalysisHypothesis
                {
                    Rank = 1,
                    Confidence = Math.Max(45, result.Confidence),
                    Hypothesis = result.PossibleCause,
                    Justification = "Mapped from legacy cause field.",
                }
            );
        }

        if (recommendedActions.Count == 0 && !string.IsNullOrWhiteSpace(result.Recommendation))
        {
            recommendedActions = SplitList(result.Recommendation);
        }

        return new AnalysisResult
        {
            Status = NormalizeStatus(result.Status),
            Severity = NormalizeSeverity(result.Severity, result.Status),
            Confidence = Math.Clamp(result.Confidence, 0, 100),
            ExecutiveSummary =
                FirstNonEmpty(result.ExecutiveSummary, result.Reason)
                ?? "Telemetry reviewed. Additional analysis detail was limited.",
            Evidence = evidence,
            KeyFindings = keyFindings,
            PossibleCauses = possibleCauses,
            OperationalImpact =
                result.OperationalImpact
                ?? "No operational impact assessment was returned.",
            EstimatedFinancialLoss =
                result.EstimatedFinancialLoss
                ?? "No material financial loss estimated.",
            EstimatedCarbonImpact =
                result.EstimatedCarbonImpact
                ?? "No additional carbon impact identified.",
            RecommendedActions = recommendedActions,
            Metadata = new AnalysisMetadata
            {
                AnalyzedAtUtc =
                    metadata.AnalyzedAtUtc == default
                        ? DateTime.UtcNow
                        : metadata.AnalyzedAtUtc,
                Engine = FirstNonEmpty(metadata.Engine, "Telecommunications Project Core AI")!,
                Model = FirstNonEmpty(metadata.Model, _model)!,
                WeatherSource =
                    FirstNonEmpty(metadata.WeatherSource, request.WeatherSource)!,
                TelemetryTimestampUtc =
                    metadata.TelemetryTimestampUtc == default
                        ? request.RecordedAt
                        : metadata.TelemetryTimestampUtc,
            },
            Reason = result.Reason,
            Recommendation = result.Recommendation,
            PossibleCause = result.PossibleCause,
        };
    }

    private static string ExtractJsonBlock(string rawResponse)
    {
        var cleaned = rawResponse.Trim().Trim('`').Replace("json\n", "", StringComparison.OrdinalIgnoreCase).Trim();
        var firstBrace = cleaned.IndexOf('{');
        var lastBrace = cleaned.LastIndexOf('}');

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            return cleaned[firstBrace..(lastBrace + 1)];
        }

        return cleaned;
    }

    private static List<string> SplitList(string value)
    {
        return value
            .Split(['\n', ';', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select((item) => item.Trim())
            .Where((item) => !string.IsNullOrWhiteSpace(item))
            .Take(4)
            .ToList();
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = (status ?? "").Trim().ToLowerInvariant();

        return normalized switch
        {
            "normal" => "Normal",
            "warning" => "Warning",
            "critical" => "Critical",
            _ => "Warning",
        };
    }

    private static string NormalizeSeverity(string? severity, string? status)
    {
        var normalized = (severity ?? "").Trim().ToLowerInvariant();

        if (normalized is "low" or "medium" or "high")
        {
            return char.ToUpper(normalized[0]) + normalized[1..];
        }

        return NormalizeStatus(status) switch
        {
            "Normal" => "Low",
            "Critical" => "High",
            _ => "Medium",
        };
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault((value) => !string.IsNullOrWhiteSpace(value));
    }

    private async Task<string> SendPromptAsync(
        string systemPrompt,
        string userPrompt,
        int maxTokens,
        CancellationToken cancellationToken
    )
    {
        var payload = new
        {
            model = _model,
            max_tokens = Math.Max(maxTokens, 3000),
            temperature = 0.1,
            system = systemPrompt,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = userPrompt,
                },
            },
        };

        using var request = CreateRequest(payload);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Claude API returned {StatusCode}: {Response}",
                response.StatusCode,
                responseContent
            );

            throw new ClaudeApiException(
                "Telecommunications Project Core AI could not complete the request. Please try again later."
            );
        }

        using var responseJson = JsonDocument.Parse(responseContent);

        var stopReason = responseJson.RootElement
            .GetProperty("stop_reason")
            .GetString();

        if (stopReason == "max_tokens")
        {
            throw new ClaudeApiException(
                "Claude returned incomplete JSON because the token limit was reached."
            );
        }

        var responseText = responseJson.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        return ExtractJsonBlock(responseText ?? "");
    }

    private HttpRequestMessage CreateRequest(object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            ),
        };

        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        return request;
    }
}

public sealed class ClaudeApiException(string message) : Exception(message);
