<div align="center">

<img src="project-logo.png" alt="Telecommunications Project platform logo" width="230" />

# Telecommunications Project

### See every tower. Understand every incident. Act on business impact.

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react&logoColor=white)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=flat-square&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-EF_Core-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Claude](https://img.shields.io/badge/AI-Claude-D97757?style=flat-square)](https://www.anthropic.com/claude)

**A full-stack telecom operations platform for network health, incident response, energy cost, and carbon performance.**

</div>

---

## Overview

A telecom tower can remain online while becoming more expensive, less efficient, and more likely to fail. Grid instability increases generator runtime, cooling problems raise cabinet temperature, and traffic shifts reduce capacity headroom. Telecommunications Project brings these signals into one operating workspace.

The platform connects tower telemetry, anomaly detection, incidents, evidence, ownership, energy intelligence, carbon reporting, and AI-assisted recommendations into one practical flow:

\`\`\`text
Telemetry -> Anomaly -> Incident -> Explanation -> Action
\`\`\`

The backend remains the source of truth for official severity, incident state, consumption, cost, and carbon calculations. AI explains the evidence and recommends action without changing those business metrics.

## The Business Problem

Telecom operations data is often split across alarms, readings, energy records, maintenance workflows, and sustainability reports. That fragmentation creates four expensive gaps:

| Business pressure | How the platform responds |
|---|---|
| **Slow incident triage** | Prioritises towers and places supporting telemetry beside each alert |
| **Uncontrolled energy spend** | Calculates electricity cost, efficiency, and site-level comparisons |
| **Reactive maintenance** | Connects abnormal readings with evidence, likely causes, and next actions |
| **Disconnected carbon reporting** | Calculates emissions from the same operational data used by the network team |

## The Product Experience

### 1. A Focused Operations Workspace

![Telecommunications Project operations workspace](docs/New%20Orange%20Project%20Screenshoots/Screenshot%202026-10-04%20155753.png)

The frontend provides a dedicated workspace for tower telemetry, incident detection, energy performance, AI results, and carbon reporting. The current repository includes both live API integration and an explicit demonstration mode.

### 2. Fleet Health at a Glance

![Telecommunications Project fleet health dashboard](docs/New%20Orange%20Project%20Screenshoots/Screenshot%202026-10-04%20155759.png)

The dashboard brings fleet status, warnings, critical towers, trends, open incidents, and operational priorities into one view.

### 3. Priority Incidents Carry Their Evidence

![Telecommunications Project incident workspace](docs/New%20Orange%20Project%20Screenshoots/Screenshot%202026-10-04%20155831.png)

Incidents preserve severity, tower and site context, evidence, likely causes, financial impact, carbon impact, ownership, workflow status, and recommended action.

### 4. Tower, Energy, and Carbon Intelligence

![Telecommunications Project tower and reporting experience](docs/New%20Orange%20Project%20Screenshoots/Screenshot%202026-10-04%20160022.png)

Operators can inspect individual tower readings, energy and cost trends, site comparisons, carbon totals, monthly emissions, and the highest-impact locations.

## From Alert to Resolution

The application is organised around an operational workflow rather than a collection of disconnected charts:

1. Import or receive tower, reading, energy-price, and validated AI-result data.
2. Classify tower health and identify abnormal telemetry.
3. Group findings into incidents that operators can review and manage.
4. Inspect readings, evidence, history, and AI-assisted explanations.
5. Assign ownership, track workflow status, and record the recommended action.
6. Measure energy, cost, and carbon impact through the same operational data.

The objective is not to replace engineering judgement. It is to give that judgement a faster, better-evidenced starting point.

## Financial and Sustainability Intelligence

### Energy Cost Becomes Operational Data

The energy experience reports electricity consumption, electricity spend, monthly trends, site comparisons, and measured efficiency. Calculations use imported monthly electricity prices when available.

### Carbon Reporting Uses the Same Operational Truth

The carbon experience reports CO2e totals, monthly emissions trends, highest-emitting locations, and downloadable reporting data using the configured project emissions factor.

Missing measurements remain unknown rather than being treated as confirmed operational states, and savings are presented as opportunities to measure against a real baseline—not guaranteed claims.

## Business Value

**For network operations leaders**

- See fleet health and the most urgent operational risk in one view
- Reduce time spent gathering evidence before triage
- Track incident ownership and resolution consistently
- Compare network condition with financial and carbon exposure

**For field and reliability engineers**

- Review tower readings and recent telemetry before dispatch
- Work from ranked causes, evidence, confidence, and recommended actions
- Recognise recurring site patterns and related incidents

**For finance and sustainability teams**

- Understand energy consumption and electricity spend by site
- Benchmark efficiency across the tower estate
- Track carbon impact from operational consumption data
- Identify locations contributing the greatest cost and carbon burden

## Key Capabilities

- Fleet health classification across healthy, warning, critical, and unknown states
- Tower-level electricity, temperature, network-load, and operational telemetry
- Incident review and workflow management with severity, status, ownership, and evidence
- AI-assisted explanations and recommendations with official metrics protected in the backend
- CSV and structured JSON import flows for project data
- Energy, cost, efficiency, and carbon calculations
- Carbon report generation and PDF-oriented reporting flows
- SQL Server persistence through Entity Framework Core
- Explicit demonstration mode when the live API is unavailable

## Technology

| | |
|---|---|
| **Frontend** | React 19 + TypeScript + Vite |
| **Routing and state** | TanStack Router + TanStack Query |
| **Interface** | Tailwind CSS 4 + Radix UI primitives |
| **Visualisation** | Recharts |
| **Backend** | ASP.NET Core 10 + REST endpoints |
| **Persistence** | Entity Framework Core 10 + SQL Server |
| **AI** | Claude integration for structured explanations and recommendations |
| **Reporting** | QuestPDF and jsPDF-oriented report flows |

## Application Structure

\`\`\`text
backend/
├── Aithos.Api             # ASP.NET Core API and controllers
├── Aithos.Application     # Use cases, DTOs, services, and feature logic
├── Aithos.Domain          # Domain entities and business rules
└── Aithos.Infrastructure  # EF Core persistence and external integrations

frontend/
├── src/pages/             # Dashboard, towers, incidents, energy, carbon, and login
├── src/components/        # Product-specific and reusable UI components
├── src/lib/               # API client, types, formatting, PDFs, and demo data
└── public/                # Aithos logo and frontend static assets

docs/                      # Architecture notes and product screenshots
\`\`\`

## Getting Started

### Backend

\`\`\`bash
cd backend
dotnet restore TelecommunicationsPlatform.slnx
dotnet run --project Aithos.Api
\`\`\`

Copy \`backend/Aithos.Api/appsettings.example.json\` to a local settings file and provide the required connection and integration settings.

### Frontend

\`\`\`bash
cd frontend
npm install
npm run dev
\`\`\`

Use the environment examples in \`frontend/\` for local, demo, and production-style configuration.

## Project Scope

This repository is the Clean Architecture version of Telecommunications Project. It contains the React frontend, ASP.NET Core backend, application/domain/infrastructure layers, EF Core persistence, configuration examples, project logo, architecture notes, and supplied product screenshots.

It replaces obsolete descriptions from the archived production-version repository with documentation that reflects the code and assets included here. Credentials, local configuration, datasets containing secrets, and build outputs are intentionally excluded.

---

<div align="center">

Built in Jordan 🇯🇴 by [**Abdallah Sabha**](https://github.com/Abdallah-Sabha1)

</div>
