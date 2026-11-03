import { AlertTriangle, CircleCheck, CircleHelp, OctagonAlert } from "lucide-react";

import { cn } from "@/lib/utils";
import { getTone, toneLabel, type Tone } from "@/lib/status";

const TONE_CLASS: Record<Tone, string> = {
  ok: "bg-success-subtle text-success border-success/20",
  warn: "bg-warning-subtle text-warning border-warning/20",
  danger: "bg-danger-subtle text-danger border-danger/20",
  unknown: "bg-neutral-subtle text-muted-foreground border-border",
};

const TONE_ICON: Record<Tone, typeof CircleCheck> = {
  ok: CircleCheck,
  warn: AlertTriangle,
  danger: OctagonAlert,
  unknown: CircleHelp,
};

/**
 * Status is conveyed by icon + text, never by colour alone (WCAG 1.4.1).
 */
export function StatusBadge({
  status,
  className,
  showLabel = true,
}: {
  status?: string | null;
  className?: string;
  showLabel?: boolean;
}) {
  const tone = getTone(status);
  const Icon = TONE_ICON[tone];
  const label = status?.trim() || toneLabel(tone);

  return (
    <span
      className={cn(
        "inline-flex shrink-0 items-center gap-1.5 rounded-sm border px-2 py-0.5 text-xs font-medium capitalize",
        TONE_CLASS[tone],
        className,
      )}
    >
      <Icon aria-hidden="true" className="size-3.5" />
      {showLabel ? label : <span className="sr-only">{label}</span>}
    </span>
  );
}
