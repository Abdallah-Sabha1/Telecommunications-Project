export type Tower = {
  id: number;
  name: string;
  location: string;
  status: string;
  /** Optional site metadata. Supplied by the API when available. */
  region?: string;
  siteType?: string;
  latitude?: number;
  longitude?: number;
  /** Dataset alignment: operational grouping and site characteristics. */
  siteCode?: string;
  group?: string;
  gridReliabilityPct?: number;
  coolingType?: string;
};

/** Incident workflow state. Unknown/absent values are treated as "open". */
export type IncidentStatus = "open" | "acknowledged" | "resolved";

export type Reading = {
  id: number;
  towerId: number;
  recordedAt: string;
  electricityConsumption: number;
  baselineConsumptionKwh?: number;
  deviationPercent?: number;
  temperature: number;
  trafficLoad: number;
  gridStatus?: string;
};

export type Alert = {
  id: number;
  towerId: number;
  tower: string;
  location: string;
  severity: string;
  message: string;
  confidence: number;
  createdAt: string;
  classification?: string;
  observationCount?: number;
  firstObservedAt?: string;
  lastObservedAt?: string;
  evidenceSummary?: string;
  possibleCause?: string;
  estimatedFinancialLoss?: string;
  estimatedCarbonImpact?: string;
  recommendation?: string;
  status?: IncidentStatus;
  assignedTo?: string;
  acknowledgedAt?: string;
  resolvedAt?: string;
  /** Operations metadata. Supplied by the API when available. */
  alertType?: string;
  cluster?: string;
  affectedSystems?: string[];
  falsePositive?: boolean;
  analysedAt?: string;
  openedAt?: string;
  maintenanceAt?: string;
};

/** One entry in the incident lifecycle timeline shown in the details drawer. */
export type TimelineEvent = {
  key: string;
  label: string;
  detail: string;
  at?: string | undefined;
  done: boolean;
};

export type SiteEnergy = {
  towerId: number;
  towerName: string;
  location: string;
  electricityKwh: number;
  gridCostJod: number;
  totalCostJod: number;
  /** kWh delivered per JOD spent. Higher is better. */
  efficiencyIndex: number;
};

export type EnergyCostPoint = {
  label: string;
  gridCostJod: number;
};

export type EnergyAnalytics = {
  reportingPeriod: string;
  reportingDays: number;
  totalCostJod: number;
  gridCostJod: number;
  totalElectricityKwh: number;
  trend?: EnergyCostPoint[];
  sites?: SiteEnergy[];
};

export type CarbonContributor = {
  towerName: string;
  location: string;
  electricityKwh: number;
  totalKg: number | null;
};

export type CarbonTrendPoint = {
  label: string;
  electricityKwh: number;
  totalKg: number | null;
};

export type CarbonReport = {
  reportingPeriod: string;
  totalTowers: number;
  totalReadings?: number;
  totalCo2Kg: number | null;
  actualElectricityKwh: number;
  emissionFactorKgCo2ePerKwh?: number | null;
  carbonEvaluated: boolean;
  topContributors?: CarbonContributor[];
  monthlyTrend?: CarbonTrendPoint[];
};

export type AnalysisCause = {
  rank: number;
  hypothesis: string;
  confidence: number;
  justification: string;
};

export type Analysis = {
  status?: string;
  severity?: string;
  confidence?: number;
  actualConsumptionKwh?: number;
  baselineConsumptionKwh?: number;
  deviationPercent?: number;
  observationWindow?: string;
  estimatedExcessEnergyCost?: string;
  energyTariff?: string;
  carbonEmissionsFactor?: string;
  headline?: string;
  executiveSummary?: string;
  reason?: string;
  evidence?: string[];
  keyFindings?: string[];
  possibleCauses?: AnalysisCause[];
  possibleCause?: string;
  operationalImpact?: string;
  estimatedFinancialLoss?: string;
  estimatedCarbonImpact?: string;
  recommendedActions?: string[];
  recommendation?: string;
  metadata?: {
    engine?: string;
    model?: string;
    weatherSource?: string;
    telemetryTimestampUtc?: string;
    analyzedAtUtc?: string;
  };
};

export type SessionUser = {
  name: string;
  email: string;
};

export type DashboardSummary = {
  fleetHealth: number;
  totalTowers: number;
  healthyTowers: number;
  warningTowers: number;
  criticalTowers: number;
  unknownTowers?: number;
  carbonFootprintKg: number;
  operationalTrends: DashboardTrendPoint[];
};

export type DashboardTrendPoint = {
  label: string;
  electricityKwh: number;
  temperature: number;
  trafficLoad: number;
  openAlerts: number;
};
