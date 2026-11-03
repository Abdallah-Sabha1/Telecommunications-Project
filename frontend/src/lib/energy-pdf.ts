import { jsPDF } from "jspdf";

import type { EnergyAnalytics } from "./types";

const BRAND: [number, number, number] = [255, 121, 0];
const INK: [number, number, number] = [23, 23, 23];
const MUTED: [number, number, number] = [100, 100, 100];

/** Creates a compact PDF from the energy report already loaded by the page. */
export function downloadEnergyPdf(report: EnergyAnalytics) {
  const doc = new jsPDF({ unit: "pt", format: "a4" });
  const margin = 48;
  const width = doc.internal.pageSize.getWidth();
  let y = margin + 24;

  doc.setFillColor(...BRAND);
  doc.rect(0, 0, width, 6, "F");
  doc.setTextColor(...INK);
  doc.setFont("helvetica", "bold");
  doc.setFontSize(18);
  doc.text("Telecommunications Project", margin, y);

  doc.setFont("helvetica", "normal");
  doc.setFontSize(10);
  doc.setTextColor(...MUTED);
  y += 17;
  doc.text(`Energy and cost report · ${report.reportingPeriod}`, margin, y);

  y += 34;
  doc.setTextColor(...BRAND);
  doc.setFont("helvetica", "bold");
  doc.setFontSize(11);
  doc.text("SUMMARY", margin, y);
  y += 20;
  doc.setTextColor(...INK);
  doc.setFontSize(12);
  doc.text(`Total electricity spend: ${report.totalCostJod.toLocaleString()} JOD`, margin, y);
  y += 17;
  doc.text(`Grid electricity cost: ${report.gridCostJod.toLocaleString()} JOD`, margin, y);
  y += 17;
  doc.text(
    `Electricity consumption: ${report.totalElectricityKwh.toLocaleString()} kWh`,
    margin,
    y,
  );

  y += 34;
  doc.setTextColor(...BRAND);
  doc.setFontSize(11);
  doc.text("SPEND TREND", margin, y);
  y += 18;
  doc.setTextColor(...INK);
  doc.setFont("helvetica", "normal");
  doc.setFontSize(10);
  for (const point of report.trend ?? []) {
    doc.text(point.label, margin, y);
    doc.text(`${point.gridCostJod.toLocaleString()} JOD`, margin + 100, y);
    y += 16;
  }

  y += 18;
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...BRAND);
  doc.text("SITE BENCHMARKING", margin, y);
  y += 18;
  doc.setFont("helvetica", "normal");
  doc.setTextColor(...INK);
  for (const site of report.sites ?? []) {
    doc.text(site.towerName, margin, y);
    doc.text(`${site.totalCostJod.toLocaleString()} JOD`, margin + 220, y);
    doc.text(`${site.efficiencyIndex.toFixed(1)} kWh/JOD`, margin + 320, y);
    y += 16;
  }

  doc.setFontSize(9);
  doc.setTextColor(...MUTED);
  doc.text(
    "Prepared from the operational energy dataset.",
    margin,
    doc.internal.pageSize.getHeight() - margin,
  );
  doc.save("telecommunications-energy-report.pdf");
}
