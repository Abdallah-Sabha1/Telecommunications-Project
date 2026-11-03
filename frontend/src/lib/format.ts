const numberFormat = new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 });

export function formatNumber(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return "—";
  return numberFormat.format(value);
}

export function formatKg(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return "—";
  return `${numberFormat.format(value)} kg`;
}

export function formatDateTime(value?: string | null): string {
  if (!value) return "Unavailable";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Unavailable";
  return date.toLocaleString(undefined, {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function formatRelative(value?: string | null): string {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  const minutes = Math.round((Date.now() - date.getTime()) / 60000);
  if (minutes < 1) return "just now";
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.round(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.round(hours / 24)}d ago`;
}

const currencyFormat = new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 });
const rateFormat = new Intl.NumberFormat(undefined, { minimumFractionDigits: 3, maximumFractionDigits: 3 });

/** Jordanian dinar, rendered as a plain code + amount to stay locale-safe. */
export function formatJod(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return "—";
  return `JOD ${currencyFormat.format(value)}`;
}

export function formatJodPerKwh(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return "—";
  return `JOD ${rateFormat.format(value)}`;
}

export function formatPercent(value: number | null | undefined, fractionDigits = 0): string {
  if (value == null || Number.isNaN(value)) return "—";
  return `${value.toFixed(fractionDigits)}%`;
}
