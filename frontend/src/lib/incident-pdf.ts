import { jsPDF } from "jspdf";

import { formatDateTime } from "./format";
import { alertCluster, alertType, buildTimeline, workflowStatus } from "./incidents";
import type { Alert, Tower } from "./types";

const BRAND: [number, number, number] = [255, 121, 0];
const INK: [number, number, number] = [23, 23, 23];
const MUTED: [number, number, number] = [110, 110, 110];

/**
 * Single-incident export. Mirrors the carbon report styling so both documents
 * look like they come from the same operations platform.
 */
export function downloadIncidentPdf(alert: Alert, towers: Tower[]) {
  const doc = new jsPDF({ unit: "pt", format: "a4" });
  const margin = 48;
  const width = doc.internal.pageSize.getWidth();
  let y = margin;

  const ensureSpace = (needed: number) => {
    if (y + needed > doc.internal.pageSize.getHeight() - margin) {
      doc.addPage();
      y = margin;
    }
  };

  doc.setFillColor(...BRAND);
  doc.rect(0, 0, width, 6, "F");

  doc.setTextColor(...INK);
  doc.setFont("helvetica", "bold");
  doc.setFontSize(18);
  y += 24;
  doc.text("Telecommunications Project Operations Centre", margin, y);

  doc.setFont("helvetica", "normal");
  doc.setFontSize(10);
  doc.setTextColor(...MUTED);
  y += 16;
  doc.text(`Incident report · generated ${formatDateTime(new Date().toISOString())}`, margin, y);

  const rows: [string, string][] = [
    ["Incident", `#${alert.id} · ${alertType(alert)}`],
    ["Tower", alert.tower],
    ["Cluster", alertCluster(alert, towers)],
    ["Location", alert.location],
    ["Severity", alert.severity],
    ["Status", workflowStatus(alert)],
    ["AI classification", alertType(alert)],
    ["AI observations", String(alert.observationCount ?? 1)],
    ["Assigned engineer", alert.assignedTo ?? "Unassigned"],
    ["Created", formatDateTime(alert.createdAt)],
  ];

  y += 26;
  rows.forEach(([label, value]) => {
    ensureSpace(18);
    doc.setTextColor(...MUTED);
    doc.setFont("helvetica", "normal");
    doc.text(label, margin, y);
    doc.setTextColor(...INK);
    doc.setFont("helvetica", "bold");
    doc.text(String(value), margin + 150, y);
    y += 18;
  });

  const section = (title: string, body?: string) => {
    if (!body) return;
    ensureSpace(52);
    y += 14;
    doc.setTextColor(...BRAND);
    doc.setFont("helvetica", "bold");
    doc.setFontSize(11);
    doc.text(title.toUpperCase(), margin, y);
    y += 14;
    doc.setTextColor(...INK);
    doc.setFont("helvetica", "normal");
    doc.setFontSize(10);
    const lines = doc.splitTextToSize(body, width - margin * 2) as string[];
    lines.forEach((line) => {
      ensureSpace(14);
      doc.text(line, margin, y);
      y += 14;
    });
  };

  section("What happened", alert.message);
  section("Evidence", alert.evidenceSummary);
  section("Likely cause", alert.possibleCause);
  section("Affected systems", alert.affectedSystems?.join(", "));
  section("Business impact", alert.estimatedFinancialLoss);
  section("Carbon impact", alert.estimatedCarbonImpact);
  section("Recommended action", alert.recommendation);

  y += 14;
  ensureSpace(40);
  doc.setTextColor(...BRAND);
  doc.setFont("helvetica", "bold");
  doc.setFontSize(11);
  doc.text("TIMELINE", margin, y);
  y += 14;
  doc.setFontSize(10);
  buildTimeline(alert).forEach((event) => {
    ensureSpace(14);
    doc.setTextColor(...INK);
    doc.setFont("helvetica", "bold");
    doc.text(event.label, margin, y);
    doc.setTextColor(...MUTED);
    doc.setFont("helvetica", "normal");
    doc.text(event.at ? formatDateTime(event.at) : "Pending", margin + 150, y);
    y += 14;
  });

  doc.save(`aithos-incident-${alert.id}.pdf`);
}
