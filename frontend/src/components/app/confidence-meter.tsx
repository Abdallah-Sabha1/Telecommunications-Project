import { cn } from "@/lib/utils";

/**
 * Model confidence. Rendered ONLY when the backend supplies a real score —
 * pass `value = undefined` and the component tells the truth instead of
 * inventing a number.
 */
export function ConfidenceMeter({
  value,
  source,
  className,
  compact = false,
}: {
  /** 0-100, as reported by the backend. Undefined when not scored. */
  value?: number | null;
  /** Optional provenance label, e.g. the model or rule that produced it. */
  source?: string;
  className?: string;
  compact?: boolean;
}) {
  if (value == null || Number.isNaN(value)) {
    return <span className={cn("text-xs text-muted-foreground", className)}>Not scored</span>;
  }

  const pct = Math.max(0, Math.min(100, value));

  return (
    <span className={cn("inline-flex items-center gap-2", className)}>
      <span
        aria-hidden="true"
        className={cn(
          "h-1.5 overflow-hidden rounded-xs bg-neutral-subtle",
          compact ? "w-10" : "w-16",
        )}
      >
        <span className="block h-full rounded-xs bg-brand" style={{ width: `${pct}%` }} />
      </span>
      <span className="num text-xs font-medium text-foreground">
        {Math.round(pct)}%
        <span className="sr-only"> model confidence{source ? ` from ${source}` : ""}</span>
      </span>
      {!compact && source ? <span className="text-xs text-muted-foreground">{source}</span> : null}
    </span>
  );
}
