import { Check, CircleDot, Eye } from "lucide-react";

import type { IncidentStatus } from "@/lib/types";
import { cn } from "@/lib/utils";

const LABEL: Record<IncidentStatus, string> = {
  open: "Open",
  acknowledged: "Investigating",
  resolved: "Resolved",
};

const CLASS: Record<IncidentStatus, string> = {
  open: "bg-danger-subtle text-danger border-danger/20",
  acknowledged: "bg-warning-subtle text-warning border-warning/20",
  resolved: "bg-success-subtle text-success border-success/20",
};

const ICON: Record<IncidentStatus, typeof CircleDot> = {
  open: CircleDot,
  acknowledged: Eye,
  resolved: Check,
};

/** Workflow badge, shared by the operations table and the incident drawer. */
export function WorkflowBadge({
  status,
  className,
}: {
  status: IncidentStatus;
  className?: string;
}) {
  const Icon = ICON[status];
  return (
    <span
      className={cn(
        "inline-flex shrink-0 items-center gap-1.5 rounded-sm border px-2 py-0.5 text-xs font-medium",
        CLASS[status],
        className,
      )}
    >
      <Icon aria-hidden="true" className="size-3.5" />
      {LABEL[status]}
    </span>
  );
}
