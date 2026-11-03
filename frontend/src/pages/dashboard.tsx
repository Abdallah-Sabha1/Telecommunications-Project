import { useEffect, useMemo, useState } from "react";
import { ArrowRight, ArrowUpDown, Search } from "lucide-react";

import { ChartSkeleton, TableSkeleton } from "@/components/app/skeletons";
import { StatCard } from "@/components/app/stat-card";
import { StatusBadge } from "@/components/app/status-badge";
import { EmptyState, ErrorState, SectionHeader } from "@/components/app/states";
import { TrendChart, type TrendPoint } from "@/components/app/trend-chart";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import { fetchAlerts, fetchCarbonReport, fetchDashboard, fetchTowers } from "@/lib/api";
import { formatKg, formatNumber, formatRelative } from "@/lib/format";
import { currentTowerStatus, displayClassification, groupIncidents } from "@/lib/incidents";
import { getTone, severityWeight, type Tone } from "@/lib/status";
import type { Alert, CarbonReport, DashboardSummary, Tower } from "@/lib/types";

const ALERT_PAGE_SIZE = 4;

type SortKey = "name" | "location" | "status";

export function DashboardPage({
  alertsVersion,
  onSelectTower,
  onOpenIncidents,
  onOpenCarbon,
}: {
  alertsVersion: number;
  onSelectTower: (tower: Tower) => void;
  onOpenIncidents: () => void;
  onOpenCarbon: () => void;
}) {
  const [towers, setTowers] = useState<Tower[]>([]);
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [carbon, setCarbon] = useState<CarbonReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [carbonLoading, setCarbonLoading] = useState(true);
  const [error, setError] = useState("");
  const [reloadToken, setReloadToken] = useState(0);

  const [severityFilter, setSeverityFilter] = useState<"all" | Tone>("all");
  const [sortBy, setSortBy] = useState<"latest" | "severity">("latest");
  const [visibleAlerts, setVisibleAlerts] = useState(ALERT_PAGE_SIZE);

  const [query, setQuery] = useState("");
  const [sortKey, setSortKey] = useState<SortKey>("status");
  const [sortAsc, setSortAsc] = useState(true);

  useEffect(() => {
    let alive = true;
    setLoading(true);
    setCarbonLoading(true);
    setError("");

    Promise.all([fetchDashboard(), fetchTowers(), fetchAlerts()])
      .then(([summaryResult, towerResult, alertResult]) => {
        if (!alive) return;
        setSummary(summaryResult.data);
        setTowers(towerResult.data);
        setAlerts(groupIncidents(alertResult.data));
        setLoading(false);
      })
      .catch(() => {
        if (!alive) return;
        setError(
          "Unable to load fleet data. Check your connection to the Telecommunications Project API.",
        );
        setLoading(false);
      });

    fetchCarbonReport()
      .then((result) => {
        if (!alive) return;
        setCarbon(result.data);
      })
      .catch(() => undefined)
      .finally(() => {
        if (alive) setCarbonLoading(false);
      });

    return () => {
      alive = false;
    };
  }, [alertsVersion, reloadToken]);

  const effectiveStatus = (tower: Tower) => currentTowerStatus(tower, alerts);

  const health = {
    total: summary?.totalTowers ?? towers.length,
    healthy: summary?.healthyTowers ?? 0,
    warning: summary?.warningTowers ?? 0,
    critical: summary?.criticalTowers ?? 0,
    unknown:
      summary?.unknownTowers ??
      Math.max(
        (summary?.totalTowers ?? towers.length) -
          (summary?.healthyTowers ?? 0) -
          (summary?.warningTowers ?? 0) -
          (summary?.criticalTowers ?? 0),
        0,
      ),
    pct: (summary?.fleetHealth ?? 0).toFixed(1),
  };

  const carbonSites = carbon?.topContributors ?? [];
  const averageCo2PerSite = carbon?.totalCo2Kg != null && carbon.totalTowers > 0
    ? carbon.totalCo2Kg / carbon.totalTowers
    : null;
  const highestEmittingSite = carbonSites.length
    ? carbonSites.reduce((highest, current) =>
        (current.totalKg ?? 0) > (highest.totalKg ?? 0) ? current : highest,
      )
    : null;

  const filteredAlerts = useMemo(
    () =>
      [...alerts]
        .filter((alert) => severityFilter === "all" || getTone(alert.severity) === severityFilter)
        .sort((a, b) => {
          if (sortBy === "severity") return severityWeight(b.severity) - severityWeight(a.severity);
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        }),
    [alerts, severityFilter, sortBy],
  );

  useEffect(() => {
    setVisibleAlerts(ALERT_PAGE_SIZE);
  }, [severityFilter, sortBy]);

  const trendData: TrendPoint[] = (summary?.operationalTrends ?? []).map((entry) => ({
    time: entry.label,
    electricity: entry.electricityKwh,
    temperature: entry.temperature,
    traffic: entry.trafficLoad,
  }));

  const visibleTowers = useMemo(() => {
    const needle = query.trim().toLowerCase();
    const filtered = needle
      ? towers.filter(
          (tower) =>
            tower.name.toLowerCase().includes(needle) ||
            tower.location.toLowerCase().includes(needle),
        )
      : towers;

    return [...filtered].sort((a, b) => {
      const direction = sortAsc ? 1 : -1;
      if (sortKey === "status") {
        return (
          direction * (severityWeight(effectiveStatus(b)) - severityWeight(effectiveStatus(a)))
        );
      }
      return direction * a[sortKey].localeCompare(b[sortKey]);
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [towers, alerts, query, sortKey, sortAsc]);

  function toggleSort(key: SortKey) {
    if (key === sortKey) setSortAsc((value) => !value);
    else {
      setSortKey(key);
      setSortAsc(true);
    }
  }

  const headline = health.critical
    ? `${health.critical} tower${health.critical > 1 ? "s need" : " needs"} immediate attention`
    : health.warning
      ? `${health.warning} tower${health.warning > 1 ? "s" : ""} to monitor`
      : "Fleet is stable";

  return (
    <div className="mx-auto w-full max-w-[1200px] px-4 py-8 sm:px-6 lg:px-8">
      <header className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-4 sm:flex sm:items-end sm:justify-between">
        <div className="min-w-0">
          <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
            Network overview
          </p>
          <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground">{headline}</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {health.pct}% of {health.total} monitored towers are healthy right now.
          </p>
        </div>
        <div className="flex shrink-0 flex-col items-end gap-2">
          <Button onClick={onOpenIncidents}>
            Review incidents
            <ArrowRight aria-hidden="true" className="size-4" />
          </Button>
        </div>
      </header>

      {error ? (
        <ErrorState
          className="mt-6"
          message={error}
          onRetry={() => setReloadToken((token) => token + 1)}
        />
      ) : null}

      <section aria-label="Fleet summary" className="mt-6 grid gap-4 sm:grid-cols-3">
        <StatCard
          emphasis
          hint="No active alert"
          label="Healthy"
          loading={loading}
          value={formatNumber(health.healthy)}
        />
        <StatCard
          emphasis
          hint="Monitor and inspect soon"
          label="Warning"
          loading={loading}
          value={formatNumber(health.warning)}
        />
        <StatCard
          emphasis
          hint="Immediate attention"
          label="Critical"
          loading={loading}
          value={formatNumber(health.critical)}
        />
      </section>

      <section
        aria-label="Secondary metrics"
        className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
      >
        <StatCard
          hint="Monitored telecom assets"
          label="Total towers"
          loading={loading}
          value={formatNumber(health.total)}
        />
        <StatCard
          hint="Status not reported"
          label="Unknown status"
          loading={loading}
          value={formatNumber(health.unknown)}
        />
        <StatCard
          hint={carbon?.reportingPeriod ?? "Reporting period"}
          label="Carbon footprint"
          loading={carbonLoading}
          value={carbon ? formatKg(carbon.totalCo2Kg) : "—"}
        />
      </section>

      <section aria-label="Operational trends" className="mt-6">
        <SectionHeader
          description="Recent telemetry snapshots across the fleet."
          title="Operational trends"
        />
        <div className="mt-4 grid gap-4 lg:grid-cols-3">
          {loading ? (
            <>
              <ChartSkeleton />
              <ChartSkeleton />
              <ChartSkeleton />
              <ChartSkeleton />
            </>
          ) : (
            <>
              <TrendChart
                color="var(--color-chart-1)"
                data={trendData}
                dataKey="electricity"
                title="Electricity consumption"
                unit="kWh"
              />
              <TrendChart
                color="var(--color-chart-5)"
                data={trendData}
                dataKey="temperature"
                title="Cabinet temperature"
                unit="°C"
              />
              <TrendChart
                color="var(--color-chart-2)"
                data={trendData}
                dataKey="traffic"
                title="Traffic load"
                unit="%"
                variant="area"
              />
            </>
          )}
        </div>
      </section>

      <section aria-label="Incident feed" className="mt-6">
        <SectionHeader
          description={
            loading
              ? "Loading incidents…"
              : `${filteredAlerts.length} incident${filteredAlerts.length === 1 ? "" : "s"} match the current filter.`
          }
          title="Incidents"
          actions={
            <>
              <Button onClick={onOpenIncidents} size="sm" variant="outline">
                View all incidents
                <ArrowRight aria-hidden="true" className="size-4" />
              </Button>
              <Label className="sr-only" htmlFor="severity-filter">
                Filter by severity
              </Label>
              <Select
                onValueChange={(value) => setSeverityFilter(value as "all" | Tone)}
                value={severityFilter}
              >
                <SelectTrigger className="h-9 w-[150px]" id="severity-filter">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All severities</SelectItem>
                  <SelectItem value="danger">Critical</SelectItem>
                  <SelectItem value="warn">Warning</SelectItem>
                  <SelectItem value="ok">Normal</SelectItem>
                  <SelectItem value="unknown">Unknown</SelectItem>
                </SelectContent>
              </Select>
              <Label className="sr-only" htmlFor="alert-sort">
                Sort incidents
              </Label>
              <Select
                onValueChange={(value) => setSortBy(value as "latest" | "severity")}
                value={sortBy}
              >
                <SelectTrigger className="h-9 w-[150px]" id="alert-sort">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="latest">Latest first</SelectItem>
                  <SelectItem value="severity">By severity</SelectItem>
                </SelectContent>
              </Select>
            </>
          }
        />

        <div aria-live="polite" className="mt-4 space-y-3">
          {loading ? (
            <>
              <div className="h-28 animate-pulse rounded-md border bg-muted/40" />
              <div className="h-28 animate-pulse rounded-md border bg-muted/40" />
            </>
          ) : filteredAlerts.length === 0 ? (
            <EmptyState
              message="No incidents match this severity. Change the filter or wait for the next analysis."
              title="No incidents"
            />
          ) : (
            <>
              {filteredAlerts.slice(0, visibleAlerts).map((alert) => (
                <AlertRow
                  alert={alert}
                  key={alert.id}
                  onInvestigate={() => {
                    const tower = towers.find((entry) => entry.id === alert.towerId);
                    if (tower) onSelectTower({ ...tower, status: effectiveStatus(tower) });
                  }}
                />
              ))}
              {filteredAlerts.length > visibleAlerts ? (
                <Button
                  className="w-full"
                  onClick={() => setVisibleAlerts((count) => count + ALERT_PAGE_SIZE)}
                  variant="outline"
                >
                  Show {Math.min(ALERT_PAGE_SIZE, filteredAlerts.length - visibleAlerts)} more of{" "}
                  {filteredAlerts.length}
                </Button>
              ) : (
                <p className="text-xs text-muted-foreground">
                  Showing all {filteredAlerts.length} matching incidents.
                </p>
              )}
            </>
          )}
        </div>
      </section>

      {carbon ? (
        <section aria-label="Carbon summary" className="mt-6">
          <SectionHeader
            description={`${carbon.reportingPeriod}. Full breakdown in the carbon report.`}
            title="Carbon summary"
            actions={
              <Button onClick={onOpenCarbon} size="sm" variant="outline">
                Open report
                <ArrowRight aria-hidden="true" className="size-4" />
              </Button>
            }
          />
          <div className="mt-4 grid gap-4 sm:grid-cols-3">
            <StatCard
              hint="Total emissions"
              label="Total CO2"
              value={formatKg(carbon.totalCo2Kg)}
            />
            <StatCard
              hint="Average across reporting sites"
              label="Average CO2 per site"
              value={averageCo2PerSite != null ? formatKg(averageCo2PerSite) : "—"}
            />
            <StatCard
              hint={highestEmittingSite?.location ?? "Highest reported contributor"}
              label="Highest emitting site"
              value={highestEmittingSite ? formatKg(highestEmittingSite.totalKg) : "—"}
            />
          </div>
        </section>
      ) : null}

      <section aria-label="Tower directory" className="mt-6 pb-4">
        <SectionHeader
          description={
            loading
              ? "Loading assets…"
              : `${visibleTowers.length} of ${towers.length} assets shown.`
          }
          title="Tower directory"
          actions={
            <div className="relative">
              <Search
                aria-hidden="true"
                className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
              />
              <Label className="sr-only" htmlFor="tower-search">
                Search towers
              </Label>
              <Input
                className="h-9 w-[220px] pl-8"
                id="tower-search"
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search name or location"
                type="search"
                value={query}
              />
            </div>
          }
        />

        <div className="mt-4 overflow-hidden rounded-md border">
          {loading ? (
            <TableSkeleton />
          ) : visibleTowers.length === 0 ? (
            <EmptyState
              className="border-0"
              message="No towers match your search. Try a different name or location."
              title="No matching towers"
            />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <SortableHeader
                    active={sortKey === "name"}
                    ascending={sortAsc}
                    label="Tower"
                    onClick={() => toggleSort("name")}
                  />
                  <SortableHeader
                    active={sortKey === "location"}
                    ascending={sortAsc}
                    label="Location"
                    onClick={() => toggleSort("location")}
                  />
                  <SortableHeader
                    active={sortKey === "status"}
                    ascending={sortAsc}
                    label="Status"
                    onClick={() => toggleSort("status")}
                  />
                  <TableHead className="w-24 text-right">Action</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {visibleTowers.map((tower) => (
                  <TableRow key={tower.id}>
                    <TableCell className="font-medium text-foreground">{tower.name}</TableCell>
                    <TableCell className="text-muted-foreground">{tower.location}</TableCell>
                    <TableCell>
                      <StatusBadge status={effectiveStatus(tower)} />
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        onClick={() => onSelectTower({ ...tower, status: effectiveStatus(tower) })}
                        size="sm"
                        variant="ghost"
                      >
                        View
                        <ArrowRight aria-hidden="true" className="size-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </div>
      </section>
    </div>
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
      <button
        className="inline-flex items-center gap-1.5 text-xs font-medium tracking-wide text-muted-foreground uppercase hover:text-foreground"
        onClick={onClick}
        type="button"
      >
        {label}
        <ArrowUpDown aria-hidden="true" className="size-3.5" />
        <span className="sr-only">
          {active ? `sorted ${ascending ? "ascending" : "descending"}` : "not sorted"}
        </span>
      </button>
    </TableHead>
  );
}

function AlertRow({ alert, onInvestigate }: { alert: Alert; onInvestigate: () => void }) {
  const details = [
    alert.evidenceSummary && { label: "Evidence", value: alert.evidenceSummary },
    alert.possibleCause && { label: "Possible cause", value: alert.possibleCause },
    alert.estimatedFinancialLoss &&
      alert.estimatedFinancialLoss !== "Not estimated" && {
        label: "Financial impact",
        value: alert.estimatedFinancialLoss,
      },
    alert.estimatedCarbonImpact &&
      alert.estimatedCarbonImpact !== "Not estimated" && {
        label: "Carbon impact",
        value: alert.estimatedCarbonImpact,
      },
    alert.recommendation &&
      alert.recommendation !== "No action provided" && {
        label: "Recommended action",
        value: alert.recommendation,
      },
  ].filter(Boolean) as { label: string; value: string }[];

  return (
    <article className="rounded-md border bg-card p-4">
      <div className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-3 sm:flex sm:items-center sm:justify-between">
        <div className="flex min-w-0 items-center gap-2.5">
          <StatusBadge status={alert.severity} />
          <span className="truncate text-sm font-medium text-foreground">{alert.tower}</span>
          <span className="hidden truncate text-sm text-muted-foreground sm:inline">
            {alert.location}
          </span>
        </div>
        <span className="num shrink-0 text-xs text-muted-foreground">
          {formatRelative(alert.lastObservedAt ?? alert.createdAt)} · {displayClassification(alert)}
        </span>
      </div>

      <p className="mt-2 text-sm leading-relaxed text-muted-foreground">{alert.message}</p>

      <p className="mt-2 text-xs text-muted-foreground">
        {alert.observationCount ?? 1} AI observation{(alert.observationCount ?? 1) === 1 ? "" : "s"}
      </p>

      <div className="mt-3 flex justify-end">
        <Button onClick={onInvestigate} size="sm" variant="outline">
          Investigate tower
          <ArrowRight aria-hidden="true" className="size-4" />
        </Button>
      </div>

      {details.length > 0 ? (
        <dl className="mt-3 grid gap-3 border-t pt-3 sm:grid-cols-2">
          {details.map((detail) => (
            <div key={detail.label}>
              <dt className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
                {detail.label}
              </dt>
              <dd className="mt-0.5 text-sm text-foreground">{detail.value}</dd>
            </div>
          ))}
        </dl>
      ) : null}
    </article>
  );
}
