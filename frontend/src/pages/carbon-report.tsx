import { useCallback, useEffect, useState } from "react";
import { ChevronLeft, Download } from "lucide-react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

import { CardSkeleton } from "@/components/app/skeletons";
import { StatCard } from "@/components/app/stat-card";
import { EmptyState, ErrorState, SectionHeader } from "@/components/app/states";
import { AXIS, TOOLTIP_STYLE } from "@/components/app/chart-styles";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { fetchCarbonReport } from "@/lib/api";
import { downloadCarbonPdf } from "@/lib/carbon-pdf";
import { formatKg } from "@/lib/format";
import type { CarbonReport } from "@/lib/types";

export function CarbonReportPage({ onBack }: { onBack: () => void }) {
  const [report, setReport] = useState<CarbonReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(() => {
    let alive = true;
    setLoading(true);
    setError("");
    fetchCarbonReport()
      .then((result) => {
        if (alive) setReport(result.data);
      })
      .catch(() => {
        if (alive) setError("Unable to load carbon data from the Telecommunications Project API.");
      })
      .finally(() => {
        if (alive) setLoading(false);
      });
    return () => {
      alive = false;
    };
  }, []);

  useEffect(() => load(), [load]);

  const totalTrend = (report?.monthlyTrend ?? []).map((point) => ({
    label: point.label,
    totalKg: point.totalKg,
  }));
  const contributors = report?.topContributors ?? [];
  const averageCo2PerSite = report?.totalCo2Kg != null && report.totalTowers > 0
    ? report.totalCo2Kg / report.totalTowers
    : null;
  const highestEmittingSite = contributors.length
    ? contributors.reduce((highest, current) =>
        (current.totalKg ?? 0) > (highest.totalKg ?? 0) ? current : highest,
      )
    : null;

  return (
    <div className="mx-auto w-full max-w-[1200px] px-4 py-8 sm:px-6 lg:px-8">
      <nav aria-label="Breadcrumb">
        <ol className="flex items-center gap-1.5 text-sm text-muted-foreground">
          <li>
            <button
              className="inline-flex items-center gap-1 rounded-xs hover:text-foreground"
              onClick={onBack}
              type="button"
            >
              <ChevronLeft aria-hidden="true" className="size-4" />
              Overview
            </button>
          </li>
          <li aria-hidden="true">/</li>
          <li aria-current="page" className="font-medium text-foreground">
            Carbon report
          </li>
        </ol>
      </nav>

      <header className="mt-4 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight text-foreground">Carbon report</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {loading ? "Loading reporting period…" : (report?.reportingPeriod ?? "No data")} · electricity-based carbon reporting.
          </p>
        </div>
        <Button
          disabled={!report || loading}
          onClick={() => {
            if (report) downloadCarbonPdf(report);
          }}
          type="button"
          variant="default"
        >
          <Download aria-hidden="true" className="size-4" />
          Download PDF report
        </Button>
      </header>

      {error ? <ErrorState className="mt-6" message={error} onRetry={load} /> : null}

      <section aria-label="Carbon summary" className="mt-6 grid gap-4 sm:grid-cols-3">
        <StatCard
          emphasis
          hint={report?.carbonEvaluated ? "Approved emissions factor applied" : "Pending approved emissions factor"}
          label="Total CO2"
          loading={loading}
          value={report?.totalCo2Kg != null ? formatKg(report.totalCo2Kg) : "Not evaluated"}
        />
        <StatCard
          hint="Average across reporting sites"
          label="Average CO2 per site"
          loading={loading}
          value={averageCo2PerSite != null ? formatKg(averageCo2PerSite) : "—"}
        />
        <StatCard
          hint={highestEmittingSite?.location ?? "Highest reported contributor"}
          label="Highest emitting site"
          loading={loading}
          value={highestEmittingSite?.totalKg != null ? formatKg(highestEmittingSite.totalKg) : "Not evaluated"}
        />
      </section>

      <section aria-label="Emissions trend" className="mt-6">
        <SectionHeader
          description="Electricity consumption by reporting interval; CO2e appears after factor approval."
          title="Emissions trend"
        />
        <div className="mt-4 rounded-md border bg-card p-4">
          {loading ? (
            <Skeleton className="h-[260px] w-full" />
          ) : totalTrend.length ? (
            <ResponsiveContainer height={260} width="100%">
              <BarChart
                barCategoryGap="22%"
                barGap={8}
                data={totalTrend}
                margin={{ top: 4, right: 4, bottom: 0, left: 0 }}
              >
                <CartesianGrid
                  stroke="var(--color-border)"
                  strokeDasharray="3 3"
                  vertical={false}
                />
                <XAxis axisLine={false} dataKey="label" tick={AXIS} tickLine={false} />
                <YAxis axisLine={false} tick={AXIS} tickLine={false} width={48} />
                <Tooltip contentStyle={TOOLTIP_STYLE} cursor={{ fill: "var(--color-muted)" }} />
                <Bar
                  dataKey="totalKg"
                  fill="var(--color-chart-1)"
                  maxBarSize={120}
                  name="Total CO2"
                  radius={[5, 5, 0, 0]}
                />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <EmptyState
              className="border-0"
              message="Trend data will appear once more reporting intervals are available."
              title="No trend data"
            />
          )}
        </div>
      </section>

      <section aria-label="Top contributors" className="mt-6 pb-4">
        <SectionHeader
          description="Sites contributing the largest share of fleet emissions."
          title="Top contributors"
        />
        <div className="mt-4 overflow-hidden rounded-md border">
          {loading ? (
            <div className="p-4">
              <CardSkeleton lines={4} />
            </div>
          ) : report?.topContributors?.length ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Tower</TableHead>
                  <TableHead>Location</TableHead>
                  <TableHead className="text-right">CO2</TableHead>
                  <TableHead className="text-right">Share</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {report.topContributors.map((contributor) => (
                  <TableRow key={contributor.towerName}>
                    <TableCell className="font-medium text-foreground">
                      {contributor.towerName}
                    </TableCell>
                    <TableCell className="text-muted-foreground">{contributor.location}</TableCell>
                    <TableCell className="num text-right">
                      {contributor.totalKg != null ? formatKg(contributor.totalKg) : "Not evaluated"}
                    </TableCell>
                    <TableCell className="num text-right text-muted-foreground">
                      {report.totalCo2Kg != null && contributor.totalKg != null
                        ? `${((contributor.totalKg / report.totalCo2Kg) * 100).toFixed(1)}%`
                        : "—"}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <EmptyState
              className="border-0"
              message="Contributor breakdown will appear once emissions are attributed to sites."
              title="No contributors"
            />
          )}
        </div>
      </section>
    </div>
  );
}
