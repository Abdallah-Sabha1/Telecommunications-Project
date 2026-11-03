# Telecommunications Project

<p align="center">
  <img src="frontend/public/aithos-logo.png" alt="Aithos logo" width="180" />
</p>

<p align="center"><strong>Aithos — Telecommunications Tower Operations Platform</strong></p>

Aithos turns telecommunications-tower telemetry into a practical operating workflow for monitoring tower health, incidents, energy cost, carbon impact, and AI-assisted recommendations.

```text
Telemetry -> Anomaly -> Incident -> Explanation -> Action
```

## What this version contains

- A Clean Architecture ASP.NET Core backend with EF Core and feature-oriented application services.
- A React/Vite frontend for the operations dashboard, incidents, tower details, energy, analysis, AI results, and carbon reporting.
- CSV and structured JSON import flows for tower data, readings, electricity prices, and validated AI findings.
- Operational calculations for energy, cost, carbon impact, severity, and incident prioritization.
- Claude integration for explanations and recommendations while the backend remains the source of truth for official metrics.
- Product documentation and screenshots under `docs/`.

## Product areas

### Overview

Fleet health, critical towers, warnings, trends, open incidents, and operational priorities in one view.

### Incidents

Reviewable operational incidents with severity, tower and site context, evidence, likely causes, financial and carbon impact, ownership, status, and recommended action.

### Tower details

Recent readings, status, electricity, temperature, network load, evidence, and analysis for one tower.

### Energy and cost

Electricity consumption, spend, monthly trends, site comparison, and measured efficiency.

### Carbon report

CO2e totals, monthly emissions trends, highest-emitting sites, and downloadable reporting data using the project’s configured emissions factor.

## Architecture

```text
backend/
├── Aithos.Api             ASP.NET Core API and controllers
├── Aithos.Application     Use cases, DTOs, services, and feature logic
├── Aithos.Domain          Domain entities and business rules
└── Aithos.Infrastructure  EF Core persistence and external integrations

frontend/                  React + Vite operations interface
docs/                      Architecture notes and product screenshots
```

The backend owns official operational metrics. AI-generated text is used to explain and recommend; it does not overwrite severity, incident state, consumption, cost, carbon, or other business calculations.

## Data principles

- Imported monthly electricity prices are used when available for financial calculations.
- Carbon calculations use the configured project emissions factor.
- Missing measurements remain unknown rather than being treated as confirmed operational states.
- Reports are scoped to the selected reporting year and available source data.
- Savings and operational impact are opportunities to measure against a real baseline, not guaranteed claims.

## Getting started

### Backend

```bash
cd backend
dotnet restore TelecommunicationsPlatform.slnx
dotnet run --project Aithos.Api
```

Copy `backend/Aithos.Api/appsettings.example.json` to a local settings file and provide the required connection and integration settings.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Use the environment examples in `frontend/` for local, demo, and production-style configuration.

## Screenshots and documentation

The current product screenshots are available in [`docs/New Orange Project Screenshoots`](docs/New%20Orange%20Project%20Screenshoots/) and the enhanced screenshot set is available in [`docs/Enhanced Screenshots`](docs/Enhanced%20Screenshots/). Architecture notes are in [`backend/ARCHITECTURE.md`](backend/ARCHITECTURE.md).

## Project status

This repository is the Clean Architecture version of Telecommunications Project. It replaces obsolete descriptions from the archived production-version repository with documentation that reflects the code and assets included here.
