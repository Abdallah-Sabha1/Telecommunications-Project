import { getTone } from "./status";
import type { Alert, IncidentStatus, TimelineEvent, Tower } from "./types";

/**
 * Derivations shared by the operations centre page, its table and its drawer.
 * Everything falls back gracefully so the same code works against the live
 * .NET API payload used by the incidents page.
 */

export function workflowStatus(alert: Alert): IncidentStatus {
  return alert.status ?? "open";
}

export function alertType(alert: Alert): string {
  return alert.classification?.trim() || alert.possibleCause?.trim() || "Unclassified anomaly";
}

export function displayClassification(alert: Alert): string {
  return alertType(alert)
    .replaceAll("_", " ")
    .replace(/\b\w/g, (letter) => letter.toUpperCase());
}

/** Groups all daily AI observations for a tower into one operational incident. */
export function groupIncidents(alerts: Alert[]): Alert[] {
  const groups = new Map<number, Alert>();

  for (const alert of alerts) {
    const current = groups.get(alert.towerId);
    const observedAt = alert.createdAt;

    if (!current) {
      groups.set(alert.towerId, {
        ...alert,
        observationCount: 1,
        firstObservedAt: observedAt,
        lastObservedAt: observedAt,
      });
      continue;
    }

    const latest = new Date(observedAt).getTime() >= new Date(current.createdAt).getTime();
    const primary = severityWeight(alert.severity) > severityWeight(current.severity)
      || (severityWeight(alert.severity) === severityWeight(current.severity) && latest)
      ? alert
      : current;
    const firstObservedAt = new Date(observedAt).getTime() < new Date(current.firstObservedAt ?? observedAt).getTime()
      ? observedAt
      : current.firstObservedAt ?? observedAt;
    const statuses = [workflowStatus(current), workflowStatus(alert)];
    const status: IncidentStatus = statuses.includes("open")
      ? "open"
      : statuses.includes("acknowledged")
        ? "acknowledged"
        : "resolved";

    const merged: Alert = {
      ...primary,
      status,
      observationCount: (current.observationCount ?? 1) + 1,
      firstObservedAt,
      lastObservedAt: new Date(observedAt).getTime() > new Date(current.lastObservedAt ?? observedAt).getTime()
        ? observedAt
        : current.lastObservedAt ?? observedAt,
    };
    const assignedTo = primary.assignedTo || current.assignedTo || alert.assignedTo;
    if (assignedTo) merged.assignedTo = assignedTo;
    else delete merged.assignedTo;
    groups.set(alert.towerId, merged);
  }

  return [...groups.values()].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  );
}

function severityWeight(severity: string): number {
  const tone = getTone(severity);
  return tone === "danger" ? 3 : tone === "warn" ? 2 : tone === "ok" ? 1 : 0;
}

export function alertCluster(alert: Alert, towers: Tower[]): string {
  if (alert.cluster) return alert.cluster;
  const tower = towers.find((entry) => entry.id === alert.towerId);
  return tower?.group ?? tower?.region ?? "Unassigned";
}

export function assignedEngineer(alert: Alert): string {
  return alert.assignedTo?.trim() || "Unassigned";
}

export function currentTowerStatus(tower: Tower, alerts: Alert[]): string {
  const towerAlerts = alerts.filter(
    (alert) => alert.towerId === tower.id && workflowStatus(alert) !== "resolved",
  );
  if (towerAlerts.some((alert) => getTone(alert.severity) === "danger")) return "Critical";
  if (towerAlerts.some((alert) => getTone(alert.severity) === "warn")) return "Warning";
  return "Healthy";
}

/** Demo field team used until the engineer directory is connected to the API. */
export const JORDANIAN_ENGINEERS = [
  "Ahmad Al-Khatib",
  "Omar Al-Rawashdeh",
  "Yazan Al-Hadidi",
  "Lina Al-Masri",
  "Mohammad Al-Tal",
];

/** Distinct, sorted option values for the filter selects. */
export function uniqueValues(values: string[]): string[] {
  return [...new Set(values.filter(Boolean))].sort((a, b) => a.localeCompare(b));
}

export type DateRangeKey = "all" | "24h" | "7d" | "30d";

export const DATE_RANGE_LABEL: Record<DateRangeKey, string> = {
  all: "Any date",
  "24h": "Last 24 hours",
  "7d": "Last 7 days",
  "30d": "Last 30 days",
};

const RANGE_HOURS: Record<Exclude<DateRangeKey, "all">, number> = {
  "24h": 24,
  "7d": 24 * 7,
  "30d": 24 * 30,
};

export function withinRange(alert: Alert, range: DateRangeKey): boolean {
  if (range === "all") return true;
  const created = new Date(alert.createdAt).getTime();
  if (Number.isNaN(created)) return true;
  return Date.now() - created <= RANGE_HOURS[range] * 3600_000;
}

export type OpsSummary = {
  open: number;
  critical: number;
  investigating: number;
  resolvedToday: number;
  avgResolutionHours: number | null;
  falseRate: number | null;
};

export function summarise(alerts: Alert[]): OpsSummary {
  let open = 0;
  let critical = 0;
  let investigating = 0;
  let resolvedToday = 0;
  let resolutionTotal = 0;
  let resolutionCount = 0;
  let falsePositives = 0;

  const startOfDay = new Date();
  startOfDay.setHours(0, 0, 0, 0);

  alerts.forEach((alert) => {
    const status = workflowStatus(alert);
    if (status === "open") open += 1;
    if (status === "acknowledged") investigating += 1;
    if (getTone(alert.severity) === "danger" && status !== "resolved") critical += 1;
    if (alert.falsePositive) falsePositives += 1;

    if (status === "resolved" && alert.resolvedAt) {
      const resolved = new Date(alert.resolvedAt).getTime();
      const created = new Date(alert.createdAt).getTime();
      if (resolved >= startOfDay.getTime()) resolvedToday += 1;
      if (!Number.isNaN(resolved) && !Number.isNaN(created) && resolved > created) {
        resolutionTotal += (resolved - created) / 3600_000;
        resolutionCount += 1;
      }
    }
  });

  return {
    open,
    critical,
    investigating,
    resolvedToday,
    avgResolutionHours: resolutionCount ? resolutionTotal / resolutionCount : null,
    falseRate: alerts.length ? (falsePositives / alerts.length) * 100 : null,
  };
}

/** Lifecycle timeline. Steps without a timestamp render as not yet reached. */
export function buildTimeline(alert: Alert): TimelineEvent[] {
  const status = workflowStatus(alert);
  return [
    {
      key: "created",
      label: "Alert created",
      detail: `${alertType(alert)} detected on ${alert.tower}.`,
      at: alert.createdAt,
      done: true,
    },
    {
      key: "acknowledged",
      label: "Acknowledged",
      detail:
        alert.assignedTo != null && alert.assignedTo !== ""
          ? `Owned by ${alert.assignedTo}.`
          : "Waiting for an engineer.",
      at: alert.acknowledgedAt,
      done: status !== "open",
    },
    {
      key: "resolved",
      label: "Resolved",
      detail:
        status === "resolved" ? "Incident closed and baseline restored." : "Not resolved yet.",
      at: alert.resolvedAt,
      done: status === "resolved",
    },
  ];
}

export function relatedAlerts(alert: Alert, alerts: Alert[], towers: Tower[]): Alert[] {
  const cluster = alertCluster(alert, towers);
  const type = alertType(alert);
  return alerts
    .filter((entry) => entry.id !== alert.id)
    .filter(
      (entry) =>
        entry.towerId === alert.towerId ||
        alertType(entry) === type ||
        alertCluster(entry, towers) === cluster,
    )
    .slice(0, 4);
}
