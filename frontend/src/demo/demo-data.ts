import type {
  Alert,
  Analysis,
  CarbonReport,
  DashboardSummary,
  EnergyAnalytics,
  Reading,
  Tower,
} from "@/lib/types";

/**
 * Presentation-only data for the executive walkthrough.
 * Keep this file separate so it can be removed when the real APIs are ready.
 */
export const DEMO_MODE = import.meta.env.VITE_DEMO_MODE !== "false";
export const demoTowers: Tower[] = [
  { id: 1, name: "HOT-01 Downtown", location: "Amman", status: "Operational", group: "Amman" },
  { id: 2, name: "HOT-02 Sweileh", location: "Amman", status: "Operational", group: "Amman" },
  { id: 3, name: "HOT-03 Industrial", location: "Zarqa", status: "Critical", group: "North" },
  { id: 4, name: "HOT-04 University", location: "Irbid", status: "Operational", group: "North" },
  { id: 5, name: "HOT-05 Airport", location: "Amman", status: "Operational", group: "Amman" },
  { id: 6, name: "HOT-06 Aqaba", location: "Aqaba", status: "Warning", group: "South" },
  { id: 7, name: "HOT-07 Madaba", location: "Madaba", status: "Operational", group: "Central" },
  { id: 8, name: "HOT-08 Salt", location: "Salt", status: "Warning", group: "Central" },
];

export const demoAlerts: Alert[] = [
  {
    id: 801,
    towerId: 3,
    tower: "HOT-03 Industrial",
    location: "Zarqa",
    severity: "critical",
    message: "High electricity consumption",
    confidence: 91,
    createdAt: "2026-09-25T20:45:00Z",
    evidenceSummary: "Consumption stayed above the expected baseline for nine hours.",
    possibleCause: "High traffic load and increased cooling demand",
    estimatedFinancialLoss: "JOD 8.45",
    estimatedCarbonImpact: "27.3 kg CO2",
    recommendation: "Inspect cooling equipment and review the tower traffic load.",
    status: "open",
    alertType: "Energy anomaly",
    cluster: "North",
    affectedSystems: ["Electricity meter", "Cooling system"],
    analysedAt: "2026-09-25T21:00:00Z",
    openedAt: "2026-09-25T20:45:00Z",
  },
  {
    id: 803,
    towerId: 8,
    tower: "HOT-08 Salt",
    location: "Salt",
    severity: "warning",
    message: "Traffic load above the normal range",
    confidence: 87,
    createdAt: "2026-09-25T18:15:00Z",
    evidenceSummary: "Traffic load has remained above the site baseline since the afternoon.",
    possibleCause: "Temporary demand increase",
    estimatedFinancialLoss: "JOD 3.10",
    estimatedCarbonImpact: "10.2 kg CO2",
    recommendation: "Monitor the site and compare the next telemetry window.",
    status: "acknowledged",
    assignedTo: "Operations queue",
    alertType: "Traffic anomaly",
    cluster: "Central",
    affectedSystems: ["Traffic telemetry"],
    analysedAt: "2026-09-25T18:30:00Z",
  },
  {
    id: 806,
    towerId: 6,
    tower: "HOT-06 Aqaba",
    location: "Aqaba",
    severity: "warning",
    message: "Temperature approaching the site threshold",
    confidence: 82,
    createdAt: "2026-09-25T16:20:00Z",
    evidenceSummary: "Cabinet temperature is trending upward with a higher cooling demand.",
    possibleCause: "High ambient temperature",
    estimatedFinancialLoss: "JOD 1.80",
    estimatedCarbonImpact: "6.4 kg CO2",
    recommendation: "Schedule a cooling inspection during the next field visit.",
    status: "open",
    alertType: "Temperature anomaly",
    cluster: "South",
    affectedSystems: ["Cabinet temperature"],
    analysedAt: "2026-09-25T16:35:00Z",
  },
];

const readingValues = [182, 188, 179, 191, 196, 203, 198, 211, 206, 219, 224, 245];

export function createDemoReadings(towerId?: number): Reading[] {
  const towers = towerId == null ? demoTowers : demoTowers.filter((tower) => tower.id === towerId);
  return towers.flatMap((tower) =>
    readingValues.map((value, index) => {
      const isCriticalTower = tower.id === 3;
      const increase = isCriticalTower ? index * 3 : tower.id * 2;
      const electricityConsumption = isCriticalTower ? value + increase : value - 18 + increase;
      const baselineConsumptionKwh = isCriticalTower
        ? 180
        : Math.round(electricityConsumption * 0.98);
      return {
        id: tower.id * 100 + index + 1,
        towerId: tower.id,
        recordedAt: new Date(Date.UTC(2026, 8, 14 + index, 20, 45)).toISOString(),
        electricityConsumption,
        baselineConsumptionKwh,
        deviationPercent: Math.round(
          ((electricityConsumption - baselineConsumptionKwh) / baselineConsumptionKwh) * 100,
        ),
        temperature: 28 + (tower.id % 3) + index * 0.4,
        trafficLoad: Math.min(96, 58 + tower.id * 2 + index * 1.5),
        gridStatus: index === 10 && tower.id === 8 ? "OFF" : "ON",
      };
    }),
  );
}

export const demoDashboard: DashboardSummary = {
  fleetHealth: 62.5,
  totalTowers: 8,
  healthyTowers: 5,
  warningTowers: 2,
  criticalTowers: 1,
  unknownTowers: 0,
  carbonFootprintKg: 48366,
  operationalTrends: readingValues.map((value, index) => ({
    label: `Sep ${index + 14}`,
    electricityKwh: value + (index === readingValues.length - 1 ? 22 : 0),
    temperature: 29 + index * 0.3,
    trafficLoad: 64 + index * 1.8,
    openAlerts: index > 8 ? 1 : 0,
  })),
};

export const demoEnergy: EnergyAnalytics = {
  reportingPeriod: "01 Sep 2026 – 25 Sep 2026",
  reportingDays: 25,
  totalCostJod: 18450,
  gridCostJod: 18450,
  totalElectricityKwh: 146820,
  trend: [
    { label: "14 Sep", gridCostJod: 1040 },
    { label: "16 Sep", gridCostJod: 1190 },
    { label: "18 Sep", gridCostJod: 1275 },
    { label: "20 Sep", gridCostJod: 1330 },
    { label: "22 Sep", gridCostJod: 1460 },
    { label: "25 Sep", gridCostJod: 1585 },
  ],
  sites: demoTowers.map((tower, index) => ({
    towerId: tower.id,
    towerName: tower.name,
    location: tower.location,
    electricityKwh: 14500 + index * 950 + (tower.id === 3 ? 5200 : 0),
    gridCostJod: 2300 + index * 130 + (tower.id === 3 ? 980 : 0),
    totalCostJod: 2300 + index * 130 + (tower.id === 3 ? 980 : 0),
    efficiencyIndex: tower.id === 3 ? 6.8 : 7.7 + index * 0.1,
  })),
};

export const demoCarbon: CarbonReport = {
  reportingPeriod: "01 Jan 2026 – 25 Sep 2026",
  totalTowers: 8,
  totalCo2Kg: 48366,
  actualElectricityKwh: 123950,
  emissionFactorKgCo2ePerKwh: 0.4,
  carbonEvaluated: true,
  topContributors: demoTowers
    .slice()
    .reverse()
    .map((tower, index) => ({
      towerName: tower.name,
      location: tower.location,
      electricityKwh: 8200 / 0.4 - index * 620 / 0.4,
      totalKg: 8200 - index * 620,
    })),
  monthlyTrend: [
    { label: "Mar", electricityKwh: 9200 / 0.4, totalKg: 9200 },
    { label: "Apr", electricityKwh: 10100 / 0.4, totalKg: 10100 },
    { label: "May", electricityKwh: 11200 / 0.4, totalKg: 11200 },
    { label: "Jun", electricityKwh: 10900 / 0.4, totalKg: 10900 },
    { label: "Jul", electricityKwh: 11966 / 0.4, totalKg: 11966 },
  ],
};

export const demoAnalysis: Analysis = {
  status: "Anomaly detected",
  severity: "Critical",
  confidence: 91,
  actualConsumptionKwh: 245,
  baselineConsumptionKwh: 180,
  deviationPercent: 36,
  observationWindow: "Last nine hours, ending 25 Sep 2026 at 23:45",
  estimatedExcessEnergyCost: "JOD 8.45",
  energyTariff: "JOD 0.13 per excess kWh",
  carbonEmissionsFactor: "0.42 kg CO2 per excess kWh",
  headline: "HOT-03 consumed 65 kWh above baseline (+36%).",
  executiveSummary:
    "The excess is sustained across the observation window and needs an operational check.",
  reason:
    "Traffic and temperature increased during the same period; neither signal confirms a cooling fault.",
  evidence: [
    "Actual consumption: 245 kWh",
    "Expected baseline: 180 kWh",
    "Excess: 65 kWh (+36%)",
    "Observation window: 9 hours ending 25 Sep 2026, 23:45",
    "Estimated excess energy cost: JOD 8.45 at JOD 0.13 per kWh",
    "Estimated carbon: 27.3 kg CO2 at 0.42 kg per excess kWh",
  ],
  keyFindings: ["The site is the highest energy contributor in the current reporting period."],
  possibleCauses: [
    {
      rank: 1,
      hypothesis: "High traffic load",
      confidence: 91,
      justification:
        "Traffic rose during the same period; verify against comparable load periods before treating it as the cause.",
    },
    {
      rank: 2,
      hypothesis: "Increased cooling demand",
      confidence: 84,
      justification:
        "Temperature and electricity use rose together; this is a signal to check, not proof of a cooling fault.",
    },
  ],
  operationalImpact:
    "Confirm the excess load before dispatching maintenance. Compare cooling runtime and intake temperature with similar traffic periods.",
  estimatedFinancialLoss: "JOD 8.45",
  estimatedCarbonImpact: "27.3 kg CO2",
  recommendedActions: [
    "Field operations · within 24 hours · compare cooling runtime and intake temperature against similar load periods.",
    "Operations control · next telemetry window · confirm whether the deviation persists after traffic normalises.",
  ],
  metadata: {
    engine: "Anomaly detection service",
    model: "Energy anomaly model v1",
    weatherSource: "Site telemetry",
    telemetryTimestampUtc: "2026-09-25T20:45:00Z",
    analyzedAtUtc: "2026-09-25T21:00:00Z",
  },
};
