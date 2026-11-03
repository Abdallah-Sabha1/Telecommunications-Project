import type { ReactNode } from "react";

import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

/**
 * One label/value primitive replaces the four near-identical card components
 * (Metric, KpiCard, TelCard, MetaRow) the audit found duplicated.
 */
export function StatCard({
  label,
  value,
  hint,
  emphasis = false,
  loading = false,
  trailing,
  className,
}: {
  label: string;
  value: ReactNode;
  hint?: ReactNode;
  emphasis?: boolean;
  loading?: boolean;
  trailing?: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("rounded-md border bg-card p-4", emphasis && "shadow-xs", className)}>
      <div className="flex items-start justify-between gap-2">
        <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{label}</p>
        {trailing}
      </div>
      {loading ? (
        <Skeleton className="mt-2 h-7 w-20" />
      ) : (
        <p
          className={cn(
            "num mt-2 text-[1.85rem] font-semibold leading-none tracking-[-0.03em] text-foreground sm:text-[2rem]",
            emphasis && "tracking-[-0.035em]",
          )}
        >
          {value}
        </p>
      )}
      {hint ? <p className="mt-1 text-xs text-muted-foreground">{hint}</p> : null}
    </div>
  );
}
