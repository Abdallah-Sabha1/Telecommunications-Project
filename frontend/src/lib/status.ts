/**
 * Single source of truth for status -> tone mapping.
 *
 * Audit fix: the previous implementation was duplicated across two pages and
 * defaulted every unrecognised value to "danger", so an unknown status raised a
 * false critical alarm. Unknown now maps to its own neutral tone.
 */
export type Tone = "ok" | "warn" | "danger" | "unknown";

const TONE_BY_STATUS: Record<string, Tone> = {
  normal: "ok",
  operational: "ok",
  healthy: "ok",
  ok: "ok",
  low: "ok",
  warning: "warn",
  medium: "warn",
  degraded: "warn",
  critical: "danger",
  high: "danger",
  severe: "danger",
  offline: "danger",
};

export function getTone(status?: string | null): Tone {
  if (!status) return "unknown";
  return TONE_BY_STATUS[status.trim().toLowerCase()] ?? "unknown";
}

/** Sort weight: critical first, unknown last (never treated as an incident). */
export function severityWeight(status?: string | null): number {
  const tone = getTone(status);
  if (tone === "danger") return 4;
  if (tone === "warn") return 3;
  if (tone === "ok") return 2;
  return 1;
}

export function toneLabel(tone: Tone): string {
  if (tone === "ok") return "Normal";
  if (tone === "warn") return "Warning";
  if (tone === "danger") return "Critical";
  return "Unknown";
}
