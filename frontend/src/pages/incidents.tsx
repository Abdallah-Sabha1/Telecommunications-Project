import { useEffect, useMemo, useState } from "react";
import { ArrowRight, Check, Eye, Search } from "lucide-react";

import { IncidentDrawer } from "@/components/app/incident-drawer";
import { TableSkeleton } from "@/components/app/skeletons";
import { StatCard } from "@/components/app/stat-card";
import { StatusBadge } from "@/components/app/status-badge";
import { EmptyState, ErrorState, SectionHeader } from "@/components/app/states";
import { WorkflowBadge } from "@/components/app/workflow-badge";
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
import { assignIncident, fetchAlerts, fetchReadings, updateIncidentStatus } from "@/lib/api";
import { formatNumber, formatRelative } from "@/lib/format";
import {
  alertCluster,
  alertType,
  assignedEngineer,
  currentTowerStatus,
  DATE_RANGE_LABEL,
  JORDANIAN_ENGINEERS,
  displayClassification,
  groupIncidents,
  summarise,
  uniqueValues,
  withinRange,
  workflowStatus,
  type DateRangeKey,
} from "@/lib/incidents";
import { getTone, severityWeight, type Tone } from "@/lib/status";
import type { Alert, IncidentStatus, Reading, Tower } from "@/lib/types";
import { cn } from "@/lib/utils";

const ANY = "all";

/**
 * Operations centre. The original alerts queue, extended into the console the
 * O&M engineers run their day from: executive KPIs, enterprise filters, a
 * dense table and a full incident workspace drawer.
 */
export function IncidentsPage({
  towers,
  onSelectTower,
  onOpenEnergy,
}: {
  towers: Tower[];
  onSelectTower: (tower: Tower) => void;
  onOpenEnergy: () => void;
}) {
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reloadToken, setReloadToken] = useState(0);
  const [pendingId, setPendingId] = useState<number | null>(null);
  const [actionMessage, setActionMessage] = useState<string | null>(null);

  const [query, setQuery] = useState("");
  const [severityFilter, setSeverityFilter] = useState<string>(ANY);
  const [clusterFilter, setClusterFilter] = useState<string>(ANY);
  const [towerFilter, setTowerFilter] = useState<string>(ANY);
  const [statusFilter, setStatusFilter] = useState<string>(ANY);
  const [typeFilter, setTypeFilter] = useState<string>(ANY);
  const [engineerFilter, setEngineerFilter] = useState<string>(ANY);
  const [rangeFilter, setRangeFilter] = useState<DateRangeKey>("all");

  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [readings, setReadings] = useState<Reading[]>([]);
  const [readingsLoading, setReadingsLoading] = useState(false);

  useEffect(() => {
    let alive = true;
    setLoading(true);
    setError("");

    fetchAlerts()
      .then((result) => {
        if (!alive) return;
        setAlerts(
          groupIncidents(result.data.map((alert) => ({ ...alert, status: workflowStatus(alert) }))),
        );
      })
      .catch(() => {
        if (alive)
          setError(
            "Unable to load incidents. Check your connection to the Telecommunications Project API.",
          );
      })
      .finally(() => {
        if (alive) setLoading(false);
      });

    return () => {
      alive = false;
    };
  }, [reloadToken]);

  const selected = alerts.find((alert) => alert.id === selectedId) ?? null;
  const selectedTowerId = selected?.towerId;

  useEffect(() => {
    if (selectedTowerId == null) return;
    let alive = true;
    setReadingsLoading(true);
    fetchReadings(selectedTowerId)
      .then((result) => {
        if (alive) setReadings(result.data);
      })
      .catch(() => {
        if (alive) setReadings([]);
      })
      .finally(() => {
        if (alive) setReadingsLoading(false);
      });
    return () => {
      alive = false;
    };
  }, [selectedTowerId]);

  const summary = useMemo(() => summarise(alerts), [alerts]);
  const resolvedCount = useMemo(
    () => alerts.filter((alert) => workflowStatus(alert) === "resolved").length,
    [alerts],
  );
  const unassignedCount = useMemo(
    () => alerts.filter((alert) => !alert.assignedTo?.trim()).length,
    [alerts],
  );

  const clusters = useMemo(
    () => uniqueValues(alerts.map((alert) => alertCluster(alert, towers))),
    [alerts, towers],
  );
  const types = useMemo(() => uniqueValues(alerts.map(alertType)), [alerts]);
  const towerNames = useMemo(() => uniqueValues(alerts.map((alert) => alert.tower)), [alerts]);
  const engineers = useMemo(
    () => uniqueValues([...JORDANIAN_ENGINEERS, ...alerts.map(assignedEngineer)]),
    [alerts],
  );

  const filtered = useMemo(() => {
    const needle = query.trim().toLowerCase();
    return alerts
      .filter((a) => severityFilter === ANY || getTone(a.severity) === (severityFilter as Tone))
      .filter((a) => clusterFilter === ANY || alertCluster(a, towers) === clusterFilter)
      .filter((a) => towerFilter === ANY || a.tower === towerFilter)
      .filter((a) => statusFilter === ANY || workflowStatus(a) === statusFilter)
      .filter((a) => typeFilter === ANY || alertType(a) === typeFilter)
      .filter((a) => engineerFilter === ANY || assignedEngineer(a) === engineerFilter)
      .filter((a) => withinRange(a, rangeFilter))
      .filter(
        (a) =>
          !needle ||
          a.tower.toLowerCase().includes(needle) ||
          a.location.toLowerCase().includes(needle) ||
          a.message.toLowerCase().includes(needle) ||
          alertType(a).toLowerCase().includes(needle),
      )
      .sort((a, b) => {
        const weight = severityWeight(b.severity) - severityWeight(a.severity);
        if (weight !== 0) return weight;
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      });
  }, [
    alerts,
    query,
    severityFilter,
    clusterFilter,
    towerFilter,
    statusFilter,
    typeFilter,
    engineerFilter,
    rangeFilter,
    towers,
  ]);

  const filtersActive =
    query.trim() !== "" ||
    [severityFilter, clusterFilter, towerFilter, statusFilter, typeFilter, engineerFilter].some(
      (value) => value !== ANY,
    ) ||
    rangeFilter !== "all";

  function resetFilters() {
    setQuery("");
    setSeverityFilter(ANY);
    setClusterFilter(ANY);
    setTowerFilter(ANY);
    setStatusFilter(ANY);
    setTypeFilter(ANY);
    setEngineerFilter(ANY);
    setRangeFilter("all");
  }

  async function transition(alert: Alert, status: IncidentStatus) {
    setPendingId(alert.id);
    setActionMessage(null);
    try {
      const result = await updateIncidentStatus(alert, status);
      setAlerts((current) =>
        current.map((entry) => (entry.id === alert.id ? { ...entry, ...result.data } : entry)),
      );
      setActionMessage(
        status === "resolved"
          ? "Incident resolved successfully."
          : "Incident acknowledged successfully.",
      );
    } catch {
      setActionMessage("Unable to save this incident action. No changes were applied.");
    } finally {
      setPendingId(null);
    }
  }

  async function assign(alert: Alert, engineer: string) {
    setPendingId(alert.id);
    setActionMessage(null);
    try {
      const result = await assignIncident(alert, engineer);
      setAlerts((current) =>
        current.map((entry) =>
          entry.id === alert.id ? { ...entry, ...result.data, assignedTo: engineer } : entry,
        ),
      );
      setActionMessage("Engineer assigned successfully.");
    } catch {
      setActionMessage("Unable to assign the engineer. No changes were applied.");
    } finally {
      setPendingId(null);
    }
  }

  return (
    <div className="mx-auto w-full max-w-[1200px] px-4 py-8 sm:px-6 lg:px-8">
      <header className="grid grid-cols-[minmax(0,1fr)_auto] items-start gap-4 sm:flex sm:items-end sm:justify-between">
        <div className="min-w-0">
          <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
            Operations centre
          </p>
          <h1 className="mt-1 text-2xl font-semibold tracking-tight text-foreground">
            {summary.critical
              ? `${summary.critical} critical incident${summary.critical > 1 ? "s" : ""} awaiting action`
              : "No critical incidents outstanding"}
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Monitor, investigate and close operational alerts across the network.
          </p>
        </div>
        <Button onClick={onOpenEnergy}>
          Energy report
          <ArrowRight aria-hidden="true" className="size-4" />
        </Button>
      </header>

      {error ? (
        <ErrorState
          className="mt-6"
          message={error}
          onRetry={() => setReloadToken((token) => token + 1)}
        />
      ) : null}

      <section
        aria-label="Operations summary"
        className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6"
      >
        <StatCard
          emphasis
          hint="Not yet reviewed"
          label="Open"
          loading={loading}
          value={String(summary.open)}
        />
        <StatCard
          emphasis
          hint="Immediate attention"
          label="Critical"
          loading={loading}
          value={String(summary.critical)}
        />
        <StatCard
          hint="Assigned for investigation"
          label="Investigating"
          loading={loading}
          value={String(summary.investigating)}
        />
        <StatCard
          hint="Closed incidents"
          label="Resolved"
          loading={loading}
          value={String(resolvedCount)}
        />
        <StatCard
          hint="Waiting for an engineer"
          label="Unassigned"
          loading={loading}
          value={String(unassignedCount)}
        />
        <StatCard
          hint="Marked false by field review"
          label="False positives"
          loading={loading}
          value={String(alerts.filter((alert) => alert.falsePositive).length)}
        />
      </section>

      <section aria-label="Alert queue" className="mt-6 pb-4">
        <SectionHeader
          description={
            loading
              ? "Loading incidents…"
              : `${filtered.length} of ${alerts.length} active operational incidents match the current filters.`
          }
          title="Alerts"
          actions={
            filtersActive ? (
              <Button onClick={resetFilters} size="sm" variant="ghost">
                Clear filters
              </Button>
            ) : null
          }
        />

        <div className="mt-4 rounded-md border bg-card p-3">
          <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
            <div className="relative sm:col-span-2 lg:col-span-1">
              <Search
                aria-hidden="true"
                className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
              />
              <Label className="sr-only" htmlFor="ops-search">
                Search alerts
              </Label>
              <Input
                className="h-9 w-full pl-8"
                id="ops-search"
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search tower, type or text"
                type="search"
                value={query}
              />
            </div>

            <FilterSelect
              id="ops-severity"
              label="Severity"
              onChange={setSeverityFilter}
              options={[
                { value: "danger", label: "Critical" },
                { value: "warn", label: "Warning" },
                { value: "ok", label: "Normal" },
                { value: "unknown", label: "Unknown" },
              ]}
              placeholder="All severities"
              value={severityFilter}
            />
            <FilterSelect
              id="ops-status"
              label="Status"
              onChange={setStatusFilter}
              options={[
                { value: "open", label: "Open" },
                { value: "acknowledged", label: "Investigating" },
                { value: "resolved", label: "Resolved" },
              ]}
              placeholder="All statuses"
              value={statusFilter}
            />
            <FilterSelect
              id="ops-type"
              label="Alert type"
              onChange={setTypeFilter}
              options={types.map((value) => ({ value, label: value }))}
              placeholder="All alert types"
              value={typeFilter}
            />
            <FilterSelect
              id="ops-cluster"
              label="Cluster"
              onChange={setClusterFilter}
              options={clusters.map((value) => ({ value, label: value }))}
              placeholder="All clusters"
              value={clusterFilter}
            />
            <FilterSelect
              id="ops-tower"
              label="Tower"
              onChange={setTowerFilter}
              options={towerNames.map((value) => ({ value, label: value }))}
              placeholder="All towers"
              value={towerFilter}
            />
            <FilterSelect
              id="ops-engineer"
              label="Assigned engineer"
              onChange={setEngineerFilter}
              options={engineers.map((value) => ({ value, label: value }))}
              placeholder="All engineers"
              value={engineerFilter}
            />
            <FilterSelect
              id="ops-range"
              label="Date range"
              onChange={(value) => setRangeFilter(value as DateRangeKey)}
              options={(["24h", "7d", "30d"] as const).map((value) => ({
                value,
                label: DATE_RANGE_LABEL[value],
              }))}
              placeholder={DATE_RANGE_LABEL.all}
              value={rangeFilter}
            />
          </div>
        </div>

        <div aria-live="polite" className="mt-4 overflow-hidden rounded-md border">
          {loading ? (
            <TableSkeleton columns={6} rows={6} />
          ) : filtered.length === 0 ? (
            <EmptyState
              className="border-0"
              message={
                filtersActive
                  ? "No alerts match these filters. Clear a filter or widen the date range."
                  : "No active incidents have been recorded for the selected period."
              }
              title={filtersActive ? "No search results" : "No alerts"}
            />
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[132px]">Severity</TableHead>
                    <TableHead>Tower</TableHead>
                    <TableHead className="hidden md:table-cell">Cluster</TableHead>
                    <TableHead className="hidden lg:table-cell">Classification</TableHead>
                    <TableHead className="hidden sm:table-cell">Last observed</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="hidden lg:table-cell">AI observations</TableHead>
                    <TableHead className="hidden xl:table-cell">Engineer</TableHead>
                    <TableHead className="w-[190px] text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filtered.map((alert) => {
                    const status = workflowStatus(alert);
                    const tone = getTone(alert.severity);
                    return (
                      <TableRow
                        className="cursor-pointer transition-colors hover:bg-accent/40"
                        key={alert.id}
                        onClick={() => setSelectedId(alert.id)}
                      >
                        <TableCell>
                          <span className="flex items-center gap-2">
                            <span
                              aria-hidden="true"
                              className={cn(
                                "h-6 w-1 rounded-full",
                                tone === "danger"
                                  ? "bg-danger"
                                  : tone === "warn"
                                    ? "bg-warning"
                                    : tone === "ok"
                                      ? "bg-success"
                                      : "bg-border",
                              )}
                            />
                            <StatusBadge status={alert.severity} />
                          </span>
                        </TableCell>
                        <TableCell className="font-medium text-foreground">
                          {alert.tower}
                          <span className="block text-xs font-normal text-muted-foreground">
                            {alert.location}
                          </span>
                        </TableCell>
                        <TableCell className="hidden text-muted-foreground md:table-cell">
                          {alertCluster(alert, towers)}
                        </TableCell>
                        <TableCell className="hidden text-muted-foreground lg:table-cell">
                          {displayClassification(alert)}
                        </TableCell>
                        <TableCell className="num hidden text-muted-foreground sm:table-cell">
                          {formatRelative(alert.lastObservedAt ?? alert.createdAt)}
                        </TableCell>
                        <TableCell>
                          <WorkflowBadge status={status} />
                        </TableCell>
                        <TableCell className="num hidden lg:table-cell">
                          {alert.observationCount ?? 1}
                        </TableCell>
                        <TableCell className="hidden text-muted-foreground xl:table-cell">
                          {assignedEngineer(alert)}
                        </TableCell>
                        <TableCell
                          className="text-right"
                          onClick={(event) => event.stopPropagation()}
                        >
                          <div className="flex justify-end gap-1">
                            <Button
                              disabled={pendingId === alert.id || status !== "open"}
                              onClick={() => transition(alert, "acknowledged")}
                              size="sm"
                              variant="outline"
                            >
                              <Eye aria-hidden="true" className="size-4" />
                              <span className="sr-only sm:not-sr-only">Investigate</span>
                            </Button>
                            <Button
                              disabled={pendingId === alert.id || status === "resolved"}
                              onClick={() => transition(alert, "resolved")}
                              size="sm"
                              variant="outline"
                            >
                              <Check aria-hidden="true" className="size-4" />
                              <span className="sr-only">Resolve</span>
                            </Button>
                            <Button
                              onClick={() => setSelectedId(alert.id)}
                              size="sm"
                              variant="ghost"
                            >
                              <span className="sr-only">Open incident detail</span>
                              <ArrowRight aria-hidden="true" className="size-4" />
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          )}
        </div>
      </section>

      <IncidentDrawer
        alert={selected}
        actionMessage={actionMessage}
        alerts={alerts}
        onAssign={assign}
        onOpenChange={(open) => {
          if (!open) setSelectedId(null);
        }}
        onOpenTower={(towerId) => {
          const tower = towers.find((entry) => entry.id === towerId);
          if (tower) {
            setSelectedId(null);
            onSelectTower({ ...tower, status: currentTowerStatus(tower, alerts) });
          }
        }}
        onSelectAlert={(entry) => setSelectedId(entry.id)}
        onTransition={transition}
        pending={pendingId === selected?.id}
        readings={readings}
        readingsLoading={readingsLoading}
        towers={towers}
      />
    </div>
  );
}

function FilterSelect({
  id,
  label,
  value,
  placeholder,
  options,
  onChange,
}: {
  id: string;
  label: string;
  value: string;
  placeholder: string;
  options: { value: string; label: string }[];
  onChange: (value: string) => void;
}) {
  return (
    <div>
      <Label className="sr-only" htmlFor={id}>
        {label}
      </Label>
      <Select onValueChange={onChange} value={value}>
        <SelectTrigger className="h-9 w-full" id={id}>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ANY}>{placeholder}</SelectItem>
          {options.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
