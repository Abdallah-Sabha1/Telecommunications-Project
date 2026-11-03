import { useEffect, useState } from "react";

import { ArrowRight, ArrowUpDown, Zap } from "lucide-react";

import { ChartSkeleton, TableSkeleton } from "@/components/app/skeletons";

import { StatCard } from "@/components/app/stat-card";

import { EmptyState, ErrorState, SectionHeader } from "@/components/app/states";

import { AXIS, TOOLTIP_STYLE } from "@/components/app/chart-styles";

import { Button } from "@/components/ui/button";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import { fetchEnergyAnalytics } from "@/lib/api";

import { formatJod, formatJodPerKwh, formatNumber } from "@/lib/format";

import type { EnergyAnalytics, SiteEnergy } from "@/lib/types";

type SortKey = "totalCostJod" | "electricityKwh" | "efficiencyIndex";

/**

 * Energy and cost analytics. Answers where the money goes, how much of it is

 * electricity cost and which sites are least efficient per dinar spent.

 */

export function EnergyAnalyticsPage({ onOpenCarbon }: { onOpenCarbon: () => void }) {
  const [report, setReport] = useState<EnergyAnalytics | null>(null);

  const [loading, setLoading] = useState(true);

  const [error, setError] = useState("");

  const [reloadToken, setReloadToken] = useState(0);

  const [sortKey, setSortKey] = useState<SortKey>("totalCostJod");

  const [sortAsc, setSortAsc] = useState(false);

  useEffect(() => {
    let alive = true;

    setLoading(true);

    setError("");

    fetchEnergyAnalytics()
      .then((result) => {
        if (!alive) return;

        setReport(result.data);
      })

      .catch(() => {
        if (alive) {
          setError(
            "Unable to load energy analytics. Check your connection to the Telecommunications Project API.",
          );
        }
      })

      .finally(() => {
        if (alive) setLoading(false);
      });

    return () => {
      alive = false;
    };
  }, [reloadToken]);

  const sites = report?.sites ?? [];

  const costPerSite = report && sites.length ? report.totalCostJod / sites.length : null;

  const averageDailyConsumption = report && report.reportingDays > 0
    ? report.totalElectricityKwh / report.reportingDays
    : null;

  const averageElectricityRate = report && report.totalElectricityKwh > 0
    ? report.totalCostJod / report.totalElectricityKwh
    : null;

  const direction = sortAsc ? 1 : -1;

  const sortedSites = [...sites].sort((a, b) => direction * (a[sortKey] - b[sortKey]));

  function toggleSort(key: SortKey) {
    if (key === sortKey) setSortAsc((value) => !value);
    else {
      setSortKey(key);

      setSortAsc(false);
    }
  }

  return (
    <div className="mx-auto w-full max-w-[1200px] px-4 py-8 sm:px-6 lg:px-8">
      {" "}
      <header className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-4 sm:flex sm:items-end sm:justify-between">
        {" "}
        <div className="min-w-0">
          {" "}
          <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
            Energy and cost{" "}
          </p>{" "}
          <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground">
            Consumption and spend analytics{" "}
          </h1>{" "}
          <p className="mt-1 text-sm text-muted-foreground">
            {report?.reportingPeriod ?? "Reporting period"}. Electricity spend across the
            fleet.{" "}
          </p>{" "}
        </div>{" "}
        <Button onClick={onOpenCarbon} type="button" variant="default">
          Carbon report
          <ArrowRight aria-hidden="true" className="size-4" />
        </Button>{" "}
      </header>{" "}
      {error ? (
        <ErrorState
          className="mt-6"

          message={error}

          onRetry={() => setReloadToken((token) => token + 1)}
        />
      ) : null}
      <section aria-label="Energy and cost summary" className="mt-6 grid gap-4 sm:grid-cols-3">
        <StatCard
          emphasis
          hint="Purchased electricity"
          label="Electricity cost"
          loading={loading}
          trailing={<Zap aria-hidden="true" className="size-4 text-muted-foreground" />}
          value={formatJod(report?.gridCostJod)}
        />

        <StatCard
          emphasis
          hint="Metered consumption"
          label="Electricity consumption"
          loading={loading}
          value={report ? `${formatNumber(report.totalElectricityKwh)} kWh` : "—"}
        />

        <StatCard
          emphasis
          hint="Average across sites"
          label="Average site spend"
          loading={loading}
          value={formatJod(costPerSite)}
        />

        <StatCard
          hint="Fleet average"
          label="Average daily consumption"
          loading={loading}
          value={
            averageDailyConsumption !== null
              ? `${formatNumber(averageDailyConsumption)} kWh/day`
              : "—"
          }
        />

        <StatCard
          hint="Actual electricity cost per kWh"
          label="Average electricity rate"
          loading={loading}
          value={averageElectricityRate !== null ? `${formatJodPerKwh(averageElectricityRate)}/kWh` : "—"}
        />

        <StatCard
          hint="Electricity efficiency"
          label="Average efficiency"
          loading={loading}
          value={
            sites.length
              ? `${(
                  sites.reduce((sum, site) => sum + site.efficiencyIndex, 0) / sites.length
                ).toFixed(1)} kWh/JOD`
              : "—"
          }
        />
      </section>{" "}
      <section aria-label="Spend trend" className="mt-6">
        {" "}
        <SectionHeader
          description="Electricity spend over the reporting period."

          title="Spend trend"
        />{" "}
        <div className="mt-4">
          {" "}
          {loading ? (
            <ChartSkeleton />
          ) : !report?.trend?.length ? (
            <EmptyState
              message="No trend data has been reported for this period yet."

              title="No trend available"
            />
          ) : (
            <figure className="rounded-md border bg-card p-4">
              {" "}
              <figcaption className="flex items-baseline justify-between gap-2">
                {" "}
                <span className="text-sm font-medium text-foreground">Electricity spend</span>
                <span className="text-xs text-muted-foreground">JOD</span>{" "}
              </figcaption>{" "}
              <div className="mt-3">
                {" "}
                <ResponsiveContainer height={240} width="100%">
                  <BarChart
                    barCategoryGap="22%"

                    barGap={8}

                    data={report.trend}

                    margin={{ top: 4, right: 4, bottom: 0, left: 0 }}
                  >
                    <CartesianGrid
                      stroke="var(--color-border)"

                      strokeDasharray="3 3"

                      vertical={false}
                    />
                    <XAxis
                      axisLine={false}

                      dataKey="label"

                      minTickGap={24}

                      tick={AXIS}

                      tickLine={false}
                    />
                    <YAxis axisLine={false} tick={AXIS} tickLine={false} width={44} />
                    <Tooltip
                      contentStyle={TOOLTIP_STYLE}

                      cursor={{ fill: "var(--color-muted)", opacity: 0.4 }}
                    />
                    <Legend iconSize={8} wrapperStyle={{ fontSize: 12 }} />
                    <Bar
                      dataKey="gridCostJod"

                      fill="var(--color-chart-1)"

                      maxBarSize={120}

                      name="Grid"

                      radius={[5, 5, 0, 0]}

                      stackId="cost"
                    />
                  </BarChart>
                </ResponsiveContainer>
              </div>{" "}
            </figure>
          )}{" "}
        </div>{" "}
      </section>{" "}
      <section aria-label="Site benchmarking" className="mt-6 pb-4">
        {" "}
        <SectionHeader
          description={
            loading
              ? "Loading sites…"
              : `${sortedSites.length} sites ranked. Efficiency is kWh delivered per dinar spent.`
          }

          title="Site benchmarking"
        />{" "}
        <div className="mt-4 overflow-hidden rounded-md border">
          {" "}
          {loading ? (
            <TableSkeleton columns={4} />
          ) : sortedSites.length === 0 ? (
            <EmptyState
              className="border-0"

              message="No per-site energy data has been reported for this period."

              title="No site data"
            />
          ) : (
            <Table>
              {" "}
              <TableHeader>
                {" "}
                <TableRow>
                  <TableHead>Site</TableHead>{" "}
                  <SortableHeader
                    active={sortKey === "electricityKwh"}

                    ascending={sortAsc}

                    label="Electricity"

                    onClick={() => toggleSort("electricityKwh")}
                  />{" "}
                  <SortableHeader
                    active={sortKey === "totalCostJod"}

                    ascending={sortAsc}

                    label="Total spend"

                    onClick={() => toggleSort("totalCostJod")}
                  />{" "}
                  <SortableHeader
                    active={sortKey === "efficiencyIndex"}

                    ascending={sortAsc}

                    label="Efficiency"

                    onClick={() => toggleSort("efficiencyIndex")}
                  />{" "}
                </TableRow>{" "}
              </TableHeader>{" "}
              <TableBody>
                {" "}
                {sortedSites.map((site) => (
                  <SiteRow key={site.towerId} site={site} />
                ))}{" "}
              </TableBody>{" "}
            </Table>
          )}{" "}
        </div>{" "}
      </section>{" "}
    </div>
  );
}

function SiteRow({ site }: { site: SiteEnergy }) {
  return (
    <TableRow>
      {" "}
      <TableCell>
        <span className="block text-sm font-medium text-foreground">{site.towerName}</span>
        <span className="block text-xs text-muted-foreground">{site.location}</span>{" "}
      </TableCell>{" "}
      <TableCell className="num text-sm text-muted-foreground">
        {formatNumber(site.electricityKwh)} kWh{" "}
      </TableCell>{" "}
      <TableCell className="num text-sm font-medium text-foreground">
        {formatJod(site.totalCostJod)}{" "}
      </TableCell>{" "}
      <TableCell className="num text-sm text-muted-foreground">
        {site.efficiencyIndex.toFixed(1)} kWh/JOD{" "}
      </TableCell>{" "}
    </TableRow>
  );
}

function SortableHeader({
  label,

  active,

  ascending,

  onClick,
}: {
  label: string;

  active: boolean;

  ascending: boolean;

  onClick: () => void;
}) {
  return (
    <TableHead aria-sort={active ? (ascending ? "ascending" : "descending") : "none"}>
      {" "}
      <button
        className="inline-flex items-center gap-1.5 text-xs font-medium tracking-wide text-muted-foreground uppercase hover:text-foreground"

        onClick={onClick}

        type="button"
      >
        {label}
        <ArrowUpDown aria-hidden="true" className="size-3.5" />{" "}
        <span className="sr-only">
          {active ? `sorted ${ascending ? "ascending" : "descending"}` : "not sorted"}{" "}
        </span>{" "}
      </button>{" "}
    </TableHead>
  );
}
