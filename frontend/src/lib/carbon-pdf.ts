import { jsPDF } from "jspdf";

import type { CarbonReport } from "./types";

const BRAND: [number, number, number] = [255, 121, 0];
const INK: [number, number, number] = [23, 23, 23];
const MUTED: [number, number, number] = [100, 100, 100];

/** Creates the same report the user sees, without depending on a second tab or API URL. */
export function downloadCarbonPdf(report: CarbonReport) {
  const doc = new jsPDF({ unit: "pt", format: "a4" });
  const margin = 48;
  const width = doc.internal.pageSize.getWidth();
  let y = margin;

  doc.setFillColor(...BRAND);
  doc.rect(0, 0, width, 6, "F");
  doc.setTextColor(...INK);
  doc.setFont("helvetica", "bold");
  doc.setFontSize(18);
  y += 24;
  doc.text("Telecommunications Project", margin, y);

  doc.setFont("helvetica", "normal");
  doc.setFontSize(10);
  doc.setTextColor(...MUTED);
  y += 17;
  doc.text(`Carbon report · ${report.reportingPeriod}`, margin, y);

  y += 34;
  doc.setTextColor(...BRAND);
  doc.setFont("helvetica", "bold");
  doc.setFontSize(11);
  doc.text("CARBON STATUS", margin, y);
  y += 22;
  doc.setTextColor(...INK);
  doc.setFontSize(24);
  doc.text(report.totalCo2Kg == null ? "Not evaluated" : `${Math.round(report.totalCo2Kg).toLocaleString()} kg CO2e`, margin, y);

  y += 34;
  doc.setFontSize(11);
  doc.setTextColor(...BRAND);
  doc.text("EMISSIONS TREND", margin, y);
  y += 18;
  doc.setFont("helvetica", "normal");
  doc.setFontSize(10);
  doc.setTextColor(...INK);
  for (const point of report.monthlyTrend ?? []) {
    doc.text(point.label, margin, y);
    doc.text(point.totalKg == null ? `${point.electricityKwh.toLocaleString()} kWh` : `${Math.round(point.totalKg).toLocaleString()} kg CO2e`, margin + 100, y);
    y += 16;
  }

  y += 18;
  doc.setFont("helvetica", "bold");
  doc.setTextColor(...BRAND);
  doc.text("TOP CONTRIBUTORS", margin, y);
  y += 18;
  doc.setFont("helvetica", "normal");
  doc.setTextColor(...INK);
  for (const contributor of report.topContributors ?? []) {
    doc.text(`${contributor.towerName} · ${contributor.location}`, margin, y);
    doc.text(contributor.totalKg == null ? `${contributor.electricityKwh.toLocaleString()} kWh` : `${Math.round(contributor.totalKg).toLocaleString()} kg`, margin + 300, y);
    y += 16;
  }

  doc.setFontSize(9);
  doc.setTextColor(...MUTED);
  doc.text(
    "Prepared from the operational carbon dataset.",
    margin,
    doc.internal.pageSize.getHeight() - margin,
  );
  doc.save("telecommunications-carbon-report.pdf");
}
