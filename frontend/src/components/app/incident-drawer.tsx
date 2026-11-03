import { ArrowRight, Check, Download, ListChecks, UserPlus } from "lucide-react";
import { useState } from "react";

import { ChartSkeleton } from "@/components/app/skeletons";
import { StatCard } from "@/components/app/stat-card";
import { StatusBadge } from "@/components/app/status-badge";
import { TrendChart, type TrendPoint } from "@/components/app/trend-chart";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { formatDateTime, formatNumber, formatRelative } from "@/lib/format";
import { downloadIncidentPdf } from "@/lib/incident-pdf";
import {
  alertCluster,
  alertType,
  assignedEngineer,
  buildTimeline,
  JORDANIAN_ENGINEERS,
  relatedAlerts,
  workflowStatus,
} from "@/lib/incidents";
import type { Alert, IncidentStatus, Reading, Tower } from "@/lib/types";
import { cn } from "@/lib/utils";
import { WorkflowBadge } from "./workflow-badge";

/**
 * Right-side incident workspace. Every block reuses an existing primitive
 * (StatCard, StatusBadge, TrendChart, Button) so the drawer reads as part of
 * the same console rather than a bolt-on.
 */
export function IncidentDrawer({
  alert,
  actionMessage,
  alerts,
  towers,
  readings,
  readingsLoading,
  pending,
  onOpenChange,
  onTransition,
  onAssign,
  onSelectAlert,
  onOpenTower,
}: {
  alert: Alert | null;
  actionMessage?: string | null;
  alerts: Alert[];
  towers: Tower[];
  readings: Reading[];
  readingsLoading: boolean;
  pending: boolean;
  onOpenChange: (open: boolean) => void;
  onTransition: (alert: Alert, status: IncidentStatus) => void;
  onAssign: (alert: Alert, engineer: string) => void;
  onSelectAlert: (alert: Alert) => void;
  onOpenTower: (towerId: number) => void;
}) {
  const tower = alert ? towers.find((entry) => entry.id === alert.towerId) : undefined;
  const status = alert ? workflowStatus(alert) : "open";
  const latest = readings[readings.length - 1];

  const trend: TrendPoint[] = [...readings]
    .sort((a, b) => new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime())
    .slice(-24)
    .map((entry) => ({
      time: new Date(entry.recordedAt).toLocaleTimeString(undefined, {
        hour: "2-digit",
        minute: "2-digit",
      }),
      electricity: entry.electricityConsumption,
      temperature: entry.temperature,
      traffic: entry.trafficLoad,
    }));

  const related = alert ? relatedAlerts(alert, alerts, towers) : [];

  return (
    <Sheet onOpenChange={onOpenChange} open={Boolean(alert)}>
      <SheetContent className="w-full overflow-y-auto p-0 sm:max-w-[560px]" side="right">
        {alert ? (
          <>
            <SheetHeader className="border-b p-6">
              <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
                Incident #{alert.id} · {alertType(alert)}
              </p>
              <SheetTitle className="text-lg">{alert.tower}</SheetTitle>
              <SheetDescription>
                {alert.location} · {alertCluster(alert, towers)} cluster ·{" "}
                {tower?.siteType ?? "Site"} · raised {formatRelative(alert.createdAt)}
              </SheetDescription>
              <div className="flex flex-wrap items-center gap-2 pt-1">
                <StatusBadge status={alert.severity} />
                <WorkflowBadge status={status} />
                <span className="num text-xs text-muted-foreground">
                  {alert.observationCount ?? 1} AI observation{(alert.observationCount ?? 1) === 1 ? "" : "s"}
                </span>
              </div>
              <Button
                className="mt-3 w-full sm:w-auto"
                onClick={() =>
                  document.getElementById("incident-actions")?.scrollIntoView({ behavior: "smooth" })
                }
                type="button"
              >
                <ListChecks aria-hidden="true" className="size-4" />
                Review actions
                <ArrowRight aria-hidden="true" className="size-4" />
              </Button>
            </SheetHeader>

            <div className="space-y-8 p-6">
              <Block title="Alert summary">
                <p className="text-sm leading-relaxed text-foreground">{alert.message}</p>
                {alert.evidenceSummary ? (
                  <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                    {alert.evidenceSummary}
                  </p>
                ) : null}
                <dl className="mt-4 grid gap-3 sm:grid-cols-2">
                  <MetaRow label="Created" value={formatDateTime(alert.createdAt)} />
                  <MetaRow label="Assigned engineer" value={assignedEngineer(alert)} />
                  <MetaRow label="Cluster" value={alertCluster(alert, towers)} />
                  <MetaRow label="Site status" value={tower?.status ?? "Unknown"} />
                </dl>
              </Block>

              <AiIntelligenceCard alert={alert} />

              <Block title="Latest telemetry">
                {readingsLoading ? (
                  <div className="grid gap-3 sm:grid-cols-2">
                    <div className="h-20 animate-pulse rounded-md border bg-muted/40" />
                    <div className="h-20 animate-pulse rounded-md border bg-muted/40" />
                  </div>
                ) : latest ? (
                  <div className="grid gap-3 sm:grid-cols-2">
                    <StatCard
                      hint="kWh"
                      label="Electricity"
                      value={formatNumber(latest.electricityConsumption)}
                    />
                    <StatCard
                      hint="Celsius"
                      label="Temperature"
                      value={formatNumber(latest.temperature)}
                    />
                    <StatCard
                      hint="Percent of capacity"
                      label="Traffic load"
                      value={formatNumber(latest.trafficLoad)}
                    />
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">
                    No telemetry reported for this site yet.
                  </p>
                )}
              </Block>

              <Block description="Last 24 hours" title="Telemetry snapshot">
                {readingsLoading ? (
                  <div className="grid gap-3">
                    <ChartSkeleton />
                    <ChartSkeleton />
                  </div>
                ) : trend.length ? (
                  <div className="grid gap-3">
                    <TrendChart
                      color="var(--color-chart-1)"
                      data={trend}
                      dataKey="electricity"
                      title="Electricity"
                      unit="kWh"
                    />
                    <TrendChart
                      color="var(--color-chart-5)"
                      data={trend}
                      dataKey="temperature"
                      title="Temperature"
                      unit="°C"
                    />
                    <TrendChart
                      color="var(--color-chart-2)"
                      data={trend}
                      dataKey="traffic"
                      title="Traffic load"
                      unit="%"
                      variant="area"
                    />
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">
                    Telemetry history is unavailable for this site.
                  </p>
                )}
              </Block>

              <Block title="Timeline">
                <ol className="mt-1 space-y-0">
                  {buildTimeline(alert).map((event, index, list) => (
                    <li className="grid grid-cols-[16px_minmax(0,1fr)] gap-3" key={event.key}>
                      <div className="flex flex-col items-center">
                        <span
                          className={cn(
                            "mt-1 size-2.5 rounded-full border-2",
                            event.done ? "border-brand bg-brand" : "border-border bg-background",
                          )}
                        />
                        {index < list.length - 1 ? (
                          <span
                            className={cn("w-px flex-1", event.done ? "bg-brand/30" : "bg-border")}
                          />
                        ) : null}
                      </div>
                      <div className="pb-5">
                        <div className="flex flex-wrap items-baseline justify-between gap-2">
                          <p
                            className={cn(
                              "text-sm font-medium",
                              event.done ? "text-foreground" : "text-muted-foreground",
                            )}
                          >
                            {event.label}
                          </p>
                          <span className="num text-xs text-muted-foreground">
                            {event.at ? formatDateTime(event.at) : "Pending"}
                          </span>
                        </div>
                        <p className="mt-0.5 text-sm text-muted-foreground">{event.detail}</p>
                      </div>
                    </li>
                  ))}
                </ol>
              </Block>

              {related.length ? (
                <Block title="Related alerts">
                  <div className="space-y-2">
                    {related.map((entry) => (
                      <button
                        className="flex w-full items-center gap-2 rounded-md border bg-card p-3 text-left transition-colors hover:border-brand/40 hover:bg-accent/40"
                        key={entry.id}
                        onClick={() => onSelectAlert(entry)}
                        type="button"
                      >
                        <StatusBadge showLabel={false} status={entry.severity} />
                        <span className="min-w-0 flex-1 truncate text-sm text-foreground">
                          {entry.tower} · {alertType(entry)}
                        </span>
                        <span className="num shrink-0 text-xs text-muted-foreground">
                          {formatRelative(entry.createdAt)}
                        </span>
                      </button>
                    ))}
                  </div>
                </Block>
              ) : null}

              <Block id="incident-actions" title="Action centre" description="Assign, acknowledge, resolve, or export">
                {actionMessage ? (
                  <p
                    className="mb-3 rounded-md border border-success/30 bg-success-subtle px-3 py-2 text-sm text-success"
                    role="status"
                  >
                    {actionMessage}
                  </p>
                ) : null}
                <div className="grid gap-3 sm:grid-cols-2">
                  <AssignEngineer
                    currentName={alert.assignedTo ?? ""}
                    onAssign={(name) => onAssign(alert, name)}
                    pending={pending}
                  />
                  <Button
                    disabled={pending || status !== "open"}
                    onClick={() => onTransition(alert, "acknowledged")}
                    size="sm"
                    variant="outline"
                  >
                    <UserPlus aria-hidden="true" className="size-4" />
                    Acknowledge alert
                  </Button>
                  <Button
                    onClick={() => downloadIncidentPdf(alert, towers)}
                    size="sm"
                    variant="outline"
                  >
                    <Download aria-hidden="true" className="size-4" />
                    Export PDF
                  </Button>
                  <Button
                    disabled={pending || status === "resolved"}
                    onClick={() => onTransition(alert, "resolved")}
                    size="sm"
                  >
                    <Check aria-hidden="true" className="size-4" />
                    Close alert
                  </Button>
                </div>
                <Separator className="my-4" />
                <Button
                  className="w-full"
                  onClick={() => onOpenTower(alert.towerId)}
                  size="sm"
                  variant="ghost"
                >
                  Open tower workspace
                  <ArrowRight aria-hidden="true" className="size-4" />
                </Button>
              </Block>
            </div>
          </>
        ) : null}
      </SheetContent>
    </Sheet>
  );
}

function AssignEngineer({
  currentName,
  pending,
  onAssign,
}: {
  currentName: string;
  pending: boolean;
  onAssign: (name: string) => void;
}) {
  const [name, setName] = useState(currentName === "Unassigned" ? "" : currentName);

  return (
    <div className="flex gap-2 sm:col-span-2">
      <Label className="sr-only" htmlFor="assign-engineer">
        Assign Jordanian field engineer
      </Label>
      <Select onValueChange={setName} value={name}>
        <SelectTrigger className="flex-1" id="assign-engineer">
          <SelectValue placeholder="Select field engineer" />
        </SelectTrigger>

        <SelectContent>
          {JORDANIAN_ENGINEERS.map((engineer) => (
            <SelectItem key={engineer} value={engineer}>
              {engineer}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Button
        disabled={pending || !name}
        onClick={() => onAssign(name)}
        size="sm"
        type="button"
        variant="outline"
      >
        Assign
      </Button>
    </div>
  );
}

function AiIntelligenceCard({ alert }: { alert: Alert }) {
  const actions = (alert.recommendation ?? "")
    .split(/(?<=\.)\s+/)
    .map((entry) => entry.trim())
    .filter(Boolean);

  return (
    <section className="rounded-md border border-brand/25 bg-brand-subtle/40 p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
        AI operational briefing
        </p>
        <span className="num rounded-sm border border-brand/30 bg-card px-2 py-0.5 text-xs font-medium text-brand">
          {alert.observationCount ?? 1} observations
        </span>
      </div>

      <h3 className="mt-2 text-sm font-semibold text-foreground">
        {alert.possibleCause ?? "No dominant root cause identified."}
      </h3>

      <p className="mt-1 text-xs leading-5 text-muted-foreground">
        AI-generated interpretation of the validated incident data. Official cost and carbon values
        come from the backend calculations.
      </p>

      {alert.affectedSystems?.length ? (
        <div className="mt-3 flex flex-wrap gap-1.5">
          {alert.affectedSystems.map((system) => (
            <span
              className="rounded-sm border bg-card px-2 py-0.5 text-xs text-muted-foreground"
              key={system}
            >
              {system}
            </span>
          ))}
        </div>
      ) : null}

      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        <MetaRow label="Business impact" value={alert.estimatedFinancialLoss ?? "Not estimated"} />
        <MetaRow label="Carbon impact" value={alert.estimatedCarbonImpact ?? "Not estimated"} />
      </dl>

      {actions.length ? (
        <div className="mt-4">
          <p className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
            Recommended actions
          </p>
          <ul className="mt-1.5 space-y-1.5">
            {actions.map((action) => (
              <li className="flex gap-2 text-sm text-foreground" key={action}>
                <Check aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-brand" />
                {action}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  );
}

function Block({
  id,
  title,
  description,
  children,
}: {
  id?: string;
  title: string;
  description?: string;
  children: React.ReactNode;
}) {
  return (
    <section id={id}>
      <div className="flex items-baseline justify-between gap-2">
        <h3 className="text-sm font-semibold tracking-tight text-foreground">{title}</h3>
        {description ? <span className="text-xs text-muted-foreground">{description}</span> : null}
      </div>
      <div className="mt-3">{children}</div>
    </section>
  );
}

function MetaRow({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs font-medium tracking-wide text-muted-foreground uppercase">{label}</dt>
      <dd className="mt-0.5 text-sm text-foreground">{value}</dd>
    </div>
  );
}
