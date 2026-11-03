import { useEffect, useMemo, useState } from "react";
import { ChevronLeft, ListChecks, Loader2, Sparkles } from "lucide-react";

import { CardSkeleton, TableSkeleton } from "@/components/app/skeletons";
import { StatCard } from "@/components/app/stat-card";
import { StatusBadge } from "@/components/app/status-badge";
import { EmptyState, SectionHeader } from "@/components/app/states";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { fetchAlerts, fetchReadings, runAnalysis } from "@/lib/api";
import { formatDateTime } from "@/lib/format";
import { currentTowerStatus } from "@/lib/incidents";
import { getTone } from "@/lib/status";
import { cn } from "@/lib/utils";
import type { Analysis, AnalysisCause, Reading, Tower } from "@/lib/types";

const ROW_CAP = 10;
const ELECTRICITY_THRESHOLD = 200;

export function TowerDetailsPage({
  tower,
  towers,
  backLabel = "Overview",
  onBack,
  onSelectTower,
  onAnalysisComplete,
}: {
  tower: Tower;
  towers: Tower[];
  backLabel?: string;
  onBack: () => void;
  onSelectTower: (tower: Tower) => void;
  onAnalysisComplete: () => void;
}) {
  const [readings, setReadings] = useState<Reading[]>([]);
  const [incidentAlerts, setIncidentAlerts] = useState<import("@/lib/types").Alert[]>([]);
  const [loading, setLoading] = useState(true);
  const [analysis, setAnalysis] = useState<Analysis | null>(null);
  const [analyzing, setAnalyzing] = useState(false);
  const [message, setMessage] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);

  useEffect(() => {
    let alive = true;

    setLoading(true);
    setAnalysis(null);
    setMessage(null);

    fetchReadings(tower.id)
      .then((result: { data: Reading[] }) => {
        if (alive) {
          setReadings(result.data);
        }
      })
      .catch(() => {
        if (alive) {
          setReadings([]);
        }
      })
      .finally(() => {
        if (alive) {
          setLoading(false);
        }
      });

    fetchAlerts()
      .then((result) => {
        if (alive) setIncidentAlerts(result.data);
      })
      .catch(() => {
        if (alive) setIncidentAlerts([]);
      });

    return () => {
      alive = false;
    };
  }, [tower.id]);

  const latestReading = readings[0];
  const displayStatus = currentTowerStatus(tower, incidentAlerts);

  const sortedReadings = useMemo(
    () =>
      [...readings]
        .sort((a, b) => new Date(b.recordedAt).getTime() - new Date(a.recordedAt).getTime())
        .slice(0, ROW_CAP),
    [readings],
  );

  const view = buildView(analysis);

  const handleRunAnalysis = async () => {
    setAnalyzing(true);
    setMessage(null);

    try {
      const result = await runAnalysis(tower.id);

      setAnalysis(result.data);
      setMessage({
        type: "success",
        text: "Assessment completed successfully.",
      });

      onAnalysisComplete();
    } catch {
      setMessage({
        type: "error",
        text: "The assessment could not be completed. Please try again.",
      });
    } finally {
      setAnalyzing(false);
    }
  };

  return (
    <div className="min-w-0">
      <div className="mx-auto w-full max-w-[1400px] px-4 py-6 sm:px-6 lg:px-8">
        <div className="mb-6 flex flex-col gap-4">
          <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
            <button
              className="inline-flex items-center gap-1 rounded-sm px-1 py-1 transition-colors hover:bg-muted hover:text-foreground"
              onClick={onBack}
              type="button"
            >
              <ChevronLeft aria-hidden="true" className="size-4" />
              {backLabel}
            </button>

            <span aria-hidden="true">/</span>

            <span>{tower.name}</span>
          </div>

          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="min-w-0">
              <p className="text-[11px] font-semibold uppercase tracking-[0.08em] text-muted-foreground">
                Tower details
              </p>

              <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
                {tower.name}
              </h1>

              <p className="mt-1 text-sm text-muted-foreground">{tower.location}</p>
            </div>

            <div className="flex flex-wrap items-center gap-2">
              <Label className="sr-only" htmlFor="tower-select">
                Select tower
              </Label>

              <Select
                value={String(tower.id)}
                onValueChange={(value: string) => {
                  const next = towers.find((entry) => String(entry.id) === value);

                  if (next) {
                    onSelectTower(next);
                  }
                }}
              >
                <SelectTrigger className="w-[210px]" id="tower-select">
                  <SelectValue placeholder="Select tower" />
                </SelectTrigger>

                <SelectContent>
                  {towers.map((entry) => (
                    <SelectItem key={entry.id} value={String(entry.id)}>
                      {entry.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Button disabled={analyzing} onClick={handleRunAnalysis}>
                {analyzing ? (
                  <Loader2 aria-hidden="true" className="size-4 animate-spin" />
                ) : (
                  <Sparkles aria-hidden="true" className="size-4" />
                )}

                {analyzing ? "Running..." : "Run assessment"}
              </Button>
            </div>
          </div>

          {message ? (
            <div
              className={cn(
                "rounded-md border px-4 py-3 text-sm",
                message.type === "success"
                  ? "border-success/30 bg-success-subtle text-success"
                  : "border-danger/25 bg-danger-subtle text-danger",
              )}
              role="status"
            >
              {message.text}
            </div>
          ) : null}
        </div>

        <section aria-label="Tower summary">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard
              label="Current status"
              value={displayStatus}
              hint="Latest reported operating state"
            />

            <StatCard
              label="Latest electricity"
              value={latestReading ? `${latestReading.electricityConsumption} kWh` : "—"}
              hint="Most recent reading"
            />

            <StatCard
              label="Temperature"
              value={latestReading ? `${latestReading.temperature} °C` : "—"}
              hint="Latest site temperature"
            />

            <StatCard
              label="Traffic load"
              value={latestReading ? `${latestReading.trafficLoad}%` : "—"}
              hint="Latest network load"
            />
          </div>
        </section>

        <section className="mt-8" aria-label="Tower status">
          <SectionHeader
            description="The current operational state reported for this tower."
            title="Current status"
          />

          <div className="mt-4 rounded-md border border-border bg-card p-5">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p className="text-sm font-semibold text-foreground">{tower.name}</p>

                <p className="mt-1 text-sm text-muted-foreground">{tower.location}</p>
              </div>

              <StatusBadge status={displayStatus} />
            </div>
          </div>
        </section>

        <section className="mt-8" aria-label="Recent telemetry">
          <SectionHeader
            description="The most recent measurements available for this tower."
            title="Recent telemetry"
          />

          <div className="mt-4 overflow-hidden rounded-md border border-border bg-card">
            {loading ? (
              <TableSkeleton rows={6} />
            ) : sortedReadings.length === 0 ? (
              <EmptyState
                message="No telemetry readings are currently available for this tower."
                title="No readings"
              />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Recorded</TableHead>
                    <TableHead className="text-right">Electricity</TableHead>
                    <TableHead className="text-right">Temperature</TableHead>
                    <TableHead className="text-right">Traffic</TableHead>
                  </TableRow>
                </TableHeader>

                <TableBody>
                  {sortedReadings.map((entry: Reading) => (
                    <TableRow key={entry.id}>
                      <TableCell className="text-muted-foreground">
                        {formatDateTime(entry.recordedAt)}
                      </TableCell>

                      <ThresholdCell
                        breached={entry.electricityConsumption > ELECTRICITY_THRESHOLD}
                        label={`${entry.electricityConsumption} kWh`}
                      />

                      <TableCell className="num text-right">{entry.temperature} °C</TableCell>

                      <TableCell className="num text-right">{entry.trafficLoad}%</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </div>
        </section>

        <section className="mt-8" aria-label="Tower operational analysis">
          <SectionHeader
            description="A plain-language explanation of what was detected, why it matters, and what the operations team should check next."
            title="Operational analysis"
          />

          <div className="mt-4">
            {analyzing && !view ? (
              <div className="space-y-4">
                <CardSkeleton lines={4} />
                <CardSkeleton lines={3} />
              </div>
            ) : view ? (
              <AnalysisView view={view} />
            ) : (
              <EmptyState
                message="Run an assessment to review the evidence, business impact, and recommended response."
                title="Analysis not available"
              />
            )}
          </div>
        </section>
      </div>
    </div>
  );
}

function ThresholdCell({ label, breached }: { label: string; breached: boolean }) {
  return (
    <TableCell className="num text-right">
      <span className="inline-flex items-center justify-end gap-1.5">
        {label}

        {breached ? (
          <span className="rounded-xs border border-warning/25 bg-warning-subtle px-1 text-[10px] font-medium tracking-wide text-warning uppercase">
            High
          </span>
        ) : null}
      </span>
    </TableCell>
  );
}

type AnalysisViewModel = {
  status: string;
  severity: string;
  confidence: number | undefined;
  headline: string;
  observationWindow: string;
  executiveSummary: string;
  evidence: string[];
  keyFindings: string[];
  possibleCauses: AnalysisCause[];
  operationalImpact: string;
  estimatedExcessEnergyCost: string;
  energyTariff: string;
  estimatedCarbonImpact: string;
  carbonEmissionsFactor: string;
  recommendedActions: string[];
  metadata: Record<string, string>;
  actualConsumptionKwh: number | undefined;
  baselineConsumptionKwh: number | undefined;
  deviationPercent: number | undefined;
};

function AnalysisView({ view }: { view: AnalysisViewModel }) {
  const excessKwh =
    view.actualConsumptionKwh != null && view.baselineConsumptionKwh != null
      ? view.actualConsumptionKwh - view.baselineConsumptionKwh
      : null;

  const deviation =
    view.deviationPercent != null
      ? `${view.deviationPercent >= 0 ? "+" : ""}${view.deviationPercent.toFixed(1)}%`
      : "—";

  const formattedExcessKwh = excessKwh == null ? null : formatKwh(excessKwh);
  const isBelowExpected = excessKwh != null && excessKwh < 0;
  const carbonImpact = isBelowExpected ? "0 kg CO₂e" : view.estimatedCarbonImpact;
  const carbonImpactDetail = isBelowExpected
    ? "No additional emissions below the expected level"
    : view.carbonEmissionsFactor;

  return (
    <div className="analysis-result space-y-6">
      <section
        aria-label="Assessment finding"
        className="analysis-summary overflow-hidden rounded-md border border-border bg-card"
      >
        <div className="border-l-4 border-brand px-5 py-5 sm:px-6">
          <div className="analysis-summary-top flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div className="min-w-0">
              <p className="text-[11px] font-semibold uppercase tracking-[0.08em] text-muted-foreground">
                What needs attention
              </p>

              <h3 className="mt-2 max-w-3xl text-xl font-semibold tracking-tight text-foreground sm:text-2xl">
                {view.headline}
              </h3>

              <p className="mt-2 max-w-3xl text-sm leading-6 text-muted-foreground">
                {view.executiveSummary}
              </p>
            </div>

            <div className="analysis-summary-status flex shrink-0 flex-wrap items-center gap-2">
              <span className="inline-flex items-center rounded-sm border border-danger/25 bg-danger-subtle px-2.5 py-1 text-xs font-semibold text-danger">
                {view.severity}
              </span>

              <span className="inline-flex items-center rounded-sm border border-border bg-muted px-2.5 py-1 text-xs font-medium text-foreground">
                {view.confidence == null ? "Not scored" : `${view.confidence}% confidence`}
              </span>
            </div>
          </div>

          <div className="analysis-kpi-grid mt-5 grid gap-4 border-t border-border pt-5 sm:grid-cols-3">
            <AnalysisMetric
              label="Actual electricity"
              value={
                view.actualConsumptionKwh != null
                  ? `${view.actualConsumptionKwh} kWh`
                  : "Not available"
              }
            />

            <AnalysisMetric
              label="Expected electricity"
              value={
                view.baselineConsumptionKwh != null
                  ? `${view.baselineConsumptionKwh} kWh`
                  : "Not available"
              }
            />

            <AnalysisMetric
              accent
              label="Above expected"
              value={formattedExcessKwh != null ? `${formattedExcessKwh} kWh` : deviation}
              detail={deviation === "—" ? undefined : deviation}
            />
          </div>

          <p className="mt-5 border-t border-border pt-4 text-xs leading-5 text-muted-foreground">
            Official metrics are calculated from approved telemetry and project configuration.
            The AI layer explains the result and recommends operational next steps.
          </p>
        </div>
      </section>

      <section aria-label="Business impact" className="analysis-section">
        <SectionHeader
          description="What this deviation could mean in money and emissions."
          title="Business impact"
        />

        <div className="mt-4 grid overflow-hidden rounded-md border border-border bg-card sm:grid-cols-3">
          <ImpactMetric
            accent
            label="Additional electricity"
            value={formattedExcessKwh != null ? `${formattedExcessKwh} kWh` : "—"}
            detail={excessKwh == null ? "No comparison available" : excessKwh >= 0 ? "Above the expected level" : "Below the expected level"}
          />

          <ImpactMetric
            label="Estimated additional cost"
            value={view.estimatedExcessEnergyCost}
            detail={view.energyTariff}
          />

          <ImpactMetric
            label="Estimated emissions"
            value={carbonImpact}
            detail={carbonImpactDetail}
          />
        </div>
      </section>

      <section
        aria-label="Plain language explanation"
        className="analysis-impact rounded-md border border-border bg-muted/30 px-5 py-5 sm:px-6"
      >
        <SectionHeader
          description="The measured deviation and the checks needed before taking action."
          title="Assessment basis"
        />

        <p className="mt-4 max-w-4xl text-sm leading-6 text-foreground">
          {excessKwh != null
            ? `Electricity use was ${formattedExcessKwh} kWh ${isBelowExpected ? "below" : "above"} the expected level during ${view.observationWindow.toLowerCase()}. The result indicates a sustained deviation, but it does not identify the cause on its own.`
            : view.executiveSummary}
        </p>

        <div className="mt-5 grid gap-4 border-t border-border pt-5 sm:grid-cols-3">
          <PlainLanguageSignal
            label="Observed"
            value={
              formattedExcessKwh != null
                ? `${formattedExcessKwh} kWh ${isBelowExpected ? "below" : "above"} expected`
                : "Higher than expected"
            }
          />

          <PlainLanguageSignal
            label="Likely driver (unconfirmed)"
            value={
              view.possibleCauses[0]?.hypothesis
                ? view.possibleCauses[0].hypothesis
                : "Further review is needed"
            }
          />

          <PlainLanguageSignal
            label="Verification status"
            value="Not confirmed — field check required"
          />
        </div>
      </section>

      <section aria-label="Evidence and context" className="analysis-section">
        <SectionHeader
          description="Measured values compared with the expected operating baseline."
          title="Evidence and baseline"
        />

        <div className="mt-4 overflow-hidden rounded-md border border-border bg-card">
          {view.evidence.slice(0, 6).map((item: string, index: number) => (
            <div
              className="grid gap-1 border-b border-border px-5 py-3.5 last:border-b-0 sm:grid-cols-[180px_minmax(0,1fr)] sm:items-center"
              key={`${item}-${index}`}
            >
              <span className="text-xs font-medium text-muted-foreground">
                {evidenceLabel(item, index)}
              </span>

              <span className="analysis-evidence-value text-sm font-medium text-foreground">
                {evidenceValue(item)}
              </span>
            </div>
          ))}
        </div>
      </section>

      <section aria-label="Possible contributors" className="analysis-section">
        <SectionHeader
          description="These are signals to check, not confirmed causes."
          title="What may be contributing"
        />

        <div className="mt-4 grid gap-3 md:grid-cols-2">
          {view.possibleCauses.length ? (
            view.possibleCauses.map((cause) => (
              <article
                className="rounded-md border border-border bg-card px-5 py-4"
                key={`${cause.rank}-${cause.hypothesis}`}
              >
                <div className="flex items-start gap-3">
                  <span className="inline-flex size-7 shrink-0 items-center justify-center rounded-sm border border-brand/25 bg-brand-subtle text-[10px] font-semibold text-brand-foreground">
                    {cause.rank}
                  </span>

                  <div className="min-w-0">
                    <h4 className="text-sm font-semibold text-foreground">{cause.hypothesis}</h4>

                    <p className="mt-1 text-sm leading-5 text-muted-foreground">
                      {cause.justification}
                    </p>
                  </div>
                </div>
              </article>
            ))
          ) : (
            <div className="rounded-md border border-border bg-card px-5 py-4 text-sm text-muted-foreground md:col-span-2">
              No possible contributors were returned by the assessment.
            </div>
          )}
        </div>
      </section>

      <section aria-label="Recommended response" className="analysis-action">
        <SectionHeader
          description="Practical next steps for the operations team."
          title="Recommended next steps"
        />

        <div className="mt-4 overflow-hidden rounded-md border border-border bg-card">
          {view.recommendedActions.map((action: string, index: number) => {
            const parsed = parseAction(action);

            return (
              <div
                className="grid gap-3 border-b border-border px-5 py-4 last:border-b-0 sm:grid-cols-[36px_minmax(0,1fr)_auto] sm:items-start"
                key={`${action}-${index}`}
              >
                <span className="inline-flex size-7 items-center justify-center rounded-sm bg-brand text-xs font-semibold text-white">
                  {index + 1}
                </span>

                <div className="min-w-0">
                  <p className="text-sm font-semibold text-foreground">{parsed.title}</p>

                  {parsed.detail ? (
                    <p className="mt-1 text-sm leading-5 text-muted-foreground">{parsed.detail}</p>
                  ) : null}
                </div>

                {parsed.owner ? (
                  <span className="analysis-action-owner text-xs font-medium text-muted-foreground sm:pt-1">
                    {parsed.owner}
                  </span>
                ) : null}
              </div>
            );
          })}
        </div>
      </section>

      <section aria-label="Assessment status" className="analysis-section rounded-md border border-border bg-card">
        <div className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between sm:px-6">
          <div>
            <p className="text-sm font-semibold text-foreground">Assessment status</p>

            <p className="mt-1 text-sm text-muted-foreground">
              The system identified an unusual energy pattern. The physical cause still needs to be
              confirmed by the operations team.
            </p>
          </div>

          <span className="inline-flex w-fit items-center gap-2 rounded-sm border border-brand/25 bg-brand-subtle px-3 py-1.5 text-xs font-semibold text-brand-foreground">
            <span aria-hidden="true" className="size-1.5 rounded-full bg-brand" />
            Review required
          </span>
        </div>
      </section>

      <details className="analysis-section rounded-md border border-border bg-card">
        <summary className="cursor-pointer list-none px-5 py-4 text-sm font-semibold text-foreground">
          Technical details
          <span className="ml-2 font-normal text-muted-foreground">
            Model and calculation information
          </span>
        </summary>

        <div className="border-t border-border px-5 py-4 sm:px-6">
          <dl className="grid gap-x-8 gap-y-4 sm:grid-cols-2">
            <TechnicalValue
              label="Expected consumption"
              value={`${view.baselineConsumptionKwh ?? "—"} kWh`}
            />

            <TechnicalValue
              label="Actual consumption"
              value={`${view.actualConsumptionKwh ?? "—"} kWh`}
            />

            <TechnicalValue
              label="Residual"
              value={formattedExcessKwh != null ? `${formattedExcessKwh} kWh` : "—"}
            />

            <TechnicalValue label="Deviation" value={deviation} />

            <TechnicalValue label="Assessment status" value={view.status} />

            <TechnicalValue label="Severity" value={view.severity} />

            <TechnicalValue
              label="Confidence"
              value={view.confidence == null ? "Not scored" : `${view.confidence}%`}
            />

            <TechnicalValue label="Observation window" value={view.observationWindow} />

            <TechnicalValue label="Energy tariff" value={view.energyTariff} />

            <TechnicalValue label="Emissions factor" value={view.carbonEmissionsFactor} />

            {Object.entries(view.metadata).map(([label, value]: [string, string]) => (
              <TechnicalValue key={label} label={label} value={value} />
            ))}
          </dl>
        </div>
      </details>
    </div>
  );
}

function AnalysisMetric({
  label,
  value,
  detail,
  accent = false,
}: {
  label: string;
  value: string;
  detail?: string | undefined;
  accent?: boolean;
}) {
  return (
    <div>
      <p className="text-[11px] font-semibold uppercase tracking-[0.07em] text-muted-foreground">
        {label}
      </p>

      <p
        className={cn(
          "mt-1 text-2xl font-semibold tracking-tight",
          accent ? "text-brand" : "text-foreground",
        )}
      >
        {value}
      </p>

      {detail ? <p className="mt-1 text-xs text-muted-foreground">{detail}</p> : null}
    </div>
  );
}

function ImpactMetric({
  label,
  value,
  detail,
  accent = false,
}: {
  label: string;
  value: string;
  detail: string;
  accent?: boolean;
}) {
  return (
    <div className="border-b border-border px-5 py-5 last:border-b-0 sm:border-b-0 sm:border-r sm:last:border-r-0">
      <p className="text-[11px] font-semibold uppercase tracking-[0.07em] text-muted-foreground">
        {label}
      </p>

      <p
        className={cn(
          "mt-2 text-2xl font-semibold tracking-tight",
          accent ? "text-brand" : "text-foreground",
        )}
      >
        {value}
      </p>

      <p className="mt-1 text-xs leading-5 text-muted-foreground">{detail}</p>
    </div>
  );
}

function PlainLanguageSignal({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-[10px] font-semibold uppercase tracking-[0.07em] text-muted-foreground">
        {label}
      </p>

      <p className="mt-1 text-sm font-medium leading-5 text-foreground">{value}</p>
    </div>
  );
}

function TechnicalValue({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs text-muted-foreground">{label}</dt>

      <dd className="mt-1 break-words text-sm font-medium text-foreground">{value}</dd>
    </div>
  );
}

function evidenceLabel(item: string, index: number): string {
  const lower = item.toLowerCase();

  if (lower.includes("actual consumption")) {
    return "Actual use";
  }

  if (lower.includes("expected baseline")) {
    return "Expected use";
  }

  if (lower.includes("observation window")) {
    return "Period reviewed";
  }

  if (lower.includes("estimated excess energy cost")) {
    return "Estimated cost";
  }

  if (lower.includes("estimated carbon")) {
    return "Carbon estimate";
  }

  if (lower.includes("excess")) {
    return "Difference";
  }

  return ["Observed signal", "Operating condition", "Assessment basis"][index] ?? "Evidence";
}

function evidenceValue(item: string): string {
  const separator = item.indexOf(":");

  return separator >= 0 ? item.slice(separator + 1).trim() : item;
}

function parseAction(action: string): {
  title: string;
  detail: string;
  owner: string;
} {
  const parts = action
    .split("·")
    .map((part: string) => part.trim())
    .filter(Boolean);

  if (parts.length === 1) {
    return {
      title: parts[0] ?? "",
      detail: "",
      owner: "",
    };
  }

  return {
    title: parts[0] ?? "",
    owner: parts[1] || "",
    detail: parts.slice(2).join(" · "),
  };
}

function formatKwh(value: number): string {
  return value.toFixed(2);
}

function buildView(analysis: Analysis | null): AnalysisViewModel | null {
  if (!analysis) {
    return null;
  }

  const causes = Array.isArray(analysis.possibleCauses)
    ? analysis.possibleCauses
    : splitCause(analysis.possibleCause);

  const executiveSummary =
    analysis.executiveSummary || analysis.reason || "No assessment summary returned.";

  const keyFindings = (analysis.keyFindings ?? []).filter(
    (item: string) => item && item !== executiveSummary && item !== analysis.reason,
  );

  return {
    status: analysis.status || "Assessment available",

    severity: analysis.severity || severityFromStatus(analysis.status),

    confidence: analysis.confidence,

    headline:
      analysis.actualConsumptionKwh != null && analysis.baselineConsumptionKwh != null
        ? `Electricity use is ${Math.abs(
            analysis.actualConsumptionKwh - analysis.baselineConsumptionKwh
          ).toFixed(2)} kWh ${analysis.actualConsumptionKwh >= analysis.baselineConsumptionKwh ? "above" : "below"} expected.`
        : analysis.headline || executiveSummary,

    observationWindow: analysis.observationWindow || "Observation window not supplied.",

    executiveSummary,

    evidence: list(analysis.evidence, ["No direct evidence returned."]),

    keyFindings,

    possibleCauses: causes,

    operationalImpact: analysis.operationalImpact || "No operational impact assessment.",

    estimatedExcessEnergyCost:
      analysis.estimatedExcessEnergyCost || analysis.estimatedFinancialLoss || "Not estimated",

    energyTariff: analysis.energyTariff || "JOD tariff not configured",

    estimatedCarbonImpact: (
      analysis.estimatedCarbonImpact || "No incremental impact estimated"
    ).replaceAll("CO2e", "CO₂e"),

    carbonEmissionsFactor:
      analysis.carbonEmissionsFactor || "Approved project factor not configured",

    recommendedActions: list(analysis.recommendedActions, splitText(analysis.recommendation, 4)),

    metadata: {
      Engine: analysis.metadata?.engine ?? "Anomaly detection service",

      Model: analysis.metadata?.model ?? "Unavailable",

      "Weather source": analysis.metadata?.weatherSource ?? "Unavailable",

      "Telemetry time": formatDateTime(analysis.metadata?.telemetryTimestampUtc),

      "Analyzed at": formatDateTime(analysis.metadata?.analyzedAtUtc),
    },

    actualConsumptionKwh: analysis.actualConsumptionKwh,

    baselineConsumptionKwh: analysis.baselineConsumptionKwh,

    deviationPercent: analysis.deviationPercent,
  };
}

function list(items: string[] | undefined, fallback: string[]): string[] {
  if (Array.isArray(items) && items.length) {
    return items.filter(Boolean);
  }

  return fallback.length ? fallback : ["No detail."];
}

function splitText(text: string | undefined, count: number): string[] {
  if (!text) {
    return ["No detail."];
  }

  return text
    .split(/[\n;.]+/)
    .map((part: string) => part.trim())
    .filter(Boolean)
    .slice(0, count);
}

function splitCause(text?: string): AnalysisCause[] {
  return splitText(text, 3).map((hypothesis: string, index: number) => ({
    rank: index + 1,
    hypothesis,
    confidence: Math.max(55 - index * 5, 40),
    justification: "Mapped from the available assessment detail.",
  }));
}

function severityFromStatus(status?: string): string {
  const tone = getTone(status);

  if (tone === "danger") {
    return "Critical";
  }

  if (tone === "warn") {
    return "Warning";
  }

  if (tone === "ok") {
    return "Operational";
  }

  return "Unknown";
}
