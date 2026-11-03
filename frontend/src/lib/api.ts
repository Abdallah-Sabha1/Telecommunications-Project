import type {
  Alert,
  Analysis,
  CarbonReport,
  DashboardSummary,
  EnergyAnalytics,
  IncidentStatus,
  Reading,
  Tower,
} from "./types";
import {
  createDemoReadings,
  DEMO_MODE,
  demoAlerts,
  demoAnalysis,
  demoCarbon,
  demoDashboard,
  demoEnergy,
  demoTowers,
} from "@/demo/demo-data";

/** API access layer. Demo mode uses the same response shapes as the backend. */

const BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "http://localhost:5000/api";

const TIMEOUT_MS = 30000;
export type Result<T> = {
  data: T;
};

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);
  try {
    const response = await fetch(`${BASE_URL}${path}`, {
      ...init,
      signal: controller.signal,
      headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
    });
    if (!response.ok) throw new Error(`Request failed with status ${response.status}`);
    return (await response.json()) as T;
  } finally {
    clearTimeout(timer);
  }
}

export function fetchTowers(): Promise<Result<Tower[]>> {
  if (DEMO_MODE) return Promise.resolve({ data: demoTowers });
  return request<Tower[]>("/towers").then((data) => ({ data }));
}

export function fetchDashboard(): Promise<Result<DashboardSummary>> {
  if (DEMO_MODE) return Promise.resolve({ data: demoDashboard });
  return request<DashboardSummary>("/dashboard").then((data) => ({ data }));
}

export function fetchReadings(towerId?: number): Promise<Result<Reading[]>> {
  if (DEMO_MODE) return Promise.resolve({ data: createDemoReadings(towerId) });
  const path = towerId == null ? "/readings" : `/readings?towerId=${towerId}`;
  return request<Reading[]>(path).then((data) => ({ data }));
}

export function fetchAlerts(): Promise<Result<Alert[]>> {
  if (DEMO_MODE) return Promise.resolve({ data: [...demoAlerts] });
  return request<Alert[]>("/alerts").then((data) => ({ data }));
}

export function fetchCarbonReport(): Promise<Result<CarbonReport>> {
  if (DEMO_MODE) return Promise.resolve({ data: demoCarbon });
  return request<CarbonReport>("/carbon/report").then((data) => ({ data }));
}

export function carbonReportPdfUrl(): string {
  return `${BASE_URL}/carbon/report/pdf`;
}

export function runAnalysis(towerId: number): Promise<Result<Analysis>> {
  if (DEMO_MODE) return Promise.resolve({ data: demoAnalysis });
  return request<Analysis>(`/analysis/${towerId}`, { method: "POST" }).then((data) => ({ data }));
}

export function fetchEnergyAnalytics(): Promise<Result<EnergyAnalytics>> {
  if (DEMO_MODE) return Promise.resolve({ data: demoEnergy });
  return request<EnergyAnalytics>("/energy/analytics").then((data) => ({ data }));
}

/**
 * Incident workflow transition. The backend stores the new status.
 */
export function updateIncidentStatus(alert: Alert, status: IncidentStatus): Promise<Result<Alert>> {
  if (DEMO_MODE) {
    const current = demoAlerts.find((entry) => entry.id === alert.id);
    if (current) {
      current.status = status;
      if (status === "acknowledged") {
        current.acknowledgedAt = new Date().toISOString();
        delete current.resolvedAt;
      } else if (status === "resolved") {
        current.resolvedAt = new Date().toISOString();
      } else {
        delete current.acknowledgedAt;
        delete current.resolvedAt;
      }
    }
    return Promise.resolve({ data: { ...alert, status } });
  }
  return request<Alert>(`/alerts/tower/${alert.towerId}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status }),
  }).then((data) => ({ data }));
}

/** Assign an incident to a field engineer. */
export function assignIncident(alert: Alert, engineer: string): Promise<Result<Alert>> {
  if (DEMO_MODE) {
    const current = demoAlerts.find((entry) => entry.id === alert.id);
    if (current) current.assignedTo = engineer;
    return Promise.resolve({ data: { ...alert, assignedTo: engineer } });
  }
  return request<Alert>(`/alerts/tower/${alert.towerId}/assignee`, {
    method: "PATCH",
    body: JSON.stringify({ assignedTo: engineer }),
  }).then((data) => ({ data }));
}
