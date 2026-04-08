# SimTrainer - Medical Simulation Training Platform

A full-stack medical simulation and training analytics platform that provides real-time CPR quality feedback, simulated patient monitoring with scenario control, and data-driven training analytics.

[![CI](../../actions/workflows/ci.yml/badge.svg)](../../actions/workflows/ci.yml)

**Live Demo:** [simtrainer-web.ashypebble-e8f32780.norwayeast.azurecontainerapps.io](https://simtrainer-web.ashypebble-e8f32780.norwayeast.azurecontainerapps.io) *(Azure Container Apps, Norway East region -- available during Norwegian business hours)*

**Job Listing:** [Laerdal Medical - Spill av knapp](https://www.finn.no/job/ad/457153565)

## Skills Demonstrated

| Feature | Job Requirement | Skills Shown |
|---|---|---|
| **Real-time CPR Scoring** (RQI) | Software connecting devices, learning flows, and analytics | SignalR WebSocket streaming, domain-driven scoring engine, AHA guideline implementation |
| **Patient Monitor** (Simulated Instruments) | Digital components interacting with physical devices, real-time feedback | Real-time data generation, smooth scenario transitions, waveform visualization |
| **Analytics Dashboard** (Sim to Improve) | AI-driven simulation analytics, quality improvement | Statistical analysis, trend detection, rule-based insight generation |
| **REST API** | REST APIs, databases | ASP.NET Core controllers, Entity Framework Core, SQL Server |
| **React Frontend** | React, TypeScript | React 18, TypeScript, Recharts, responsive design |
| **CI/CD Pipeline** | Azure cloud services, CI/CD | GitHub Actions, Docker multi-stage builds, automated testing |
| **Clean Architecture** | Scalable platform development | Domain/service/infrastructure layers, dependency injection, testability |

## Architecture

```
React SPA (TypeScript)          .NET 8 Web API
+--------------------+          +--------------------+
| CPR Training       |<-------->| REST Controllers   |
| Patient Monitor    | SignalR  | SignalR Hubs       |
| Analytics Dashboard| WebSocket| Domain Services    |
| Learner Management |          | Device Simulator   |
+--------------------+          +--------+-----------+
                                         |
                                         | Entity Framework Core
                                         |
                                +--------v-----------+
                                |   SQL Server 2022  |
                                +--------------------+
```

**Data flows:**
1. **CPR Training:** Device simulator generates compressions at ~2Hz -> scored against AHA guidelines in real time -> streamed to React via SignalR -> persisted for analytics
2. **Patient Monitor:** Instructor selects scenario -> vitals transition smoothly over 8 seconds -> streamed at 1Hz -> waveforms rendered in browser
3. **Analytics:** Session history queried via REST -> statistics computed server-side -> trend analysis and improvement suggestions returned

## Quick Start

```bash
docker compose up
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

The platform starts with:
- **SQL Server** on port 1433 (auto-migrated on first startup)
- **API** on port 5000 (Swagger UI at [http://localhost:5000/swagger](http://localhost:5000/swagger))
- **Frontend** on port 3000

### Getting Started
1. Go to **Learners** tab and create a learner
2. Go to **CPR Training** tab, select the learner, and click "Start Session"
3. Watch real-time compression feedback and quality scores
4. Stop the session and review the summary
5. Go to **Patient Monitor** tab to see simulated vital signs
6. Try different scenarios (Tachycardia, Cardiac Arrest, etc.)
7. Check the **Analytics** tab for performance trends and suggestions

## Running Tests

### Backend
```bash
cd src/SimTrainer.Tests
dotnet test
```

### Frontend
```bash
cd src/frontend
npm install
npm test
```

## Tech Stack

| Technology | Usage | Rationale |
|---|---|---|
| **C# / .NET 8** | Backend API and domain logic | Primary language in job listing |
| **ASP.NET Core + SignalR** | REST API + real-time WebSocket | Native .NET real-time framework for device-to-UI streaming |
| **Entity Framework Core** | Database ORM with code-first migrations | Standard .NET data access |
| **SQL Server 2022** | Relational database | Production-grade, runs in Docker |
| **React 18 + TypeScript** | Single-page application | Required by job listing |
| **Vite** | Frontend build tooling | Fast HMR, modern bundling |
| **Recharts** | Charts and waveform visualization | Lightweight React charting |
| **xUnit + Moq** | Backend testing | .NET testing standard |
| **React Testing Library + Vitest** | Frontend testing | React testing standard |
| **Docker Compose** | One-command deployment | Everything runs with `docker compose up` |
| **GitHub Actions** | CI/CD pipeline | Automated build, test, and Docker validation |

## Project Structure

```
src/
  SimTrainer.Api/           .NET 8 backend
    Controllers/            REST API endpoints
    Hubs/                   SignalR real-time hubs (CPR, Monitor)
    Domain/                 Entities, enums, scoring engine
    Services/               Business logic layer
    Simulation/             Device simulator and vital sign generation
    Infrastructure/         EF Core DB context and migrations
  SimTrainer.Tests/         Backend test suite (unit + integration)
  frontend/                 React TypeScript SPA
    src/pages/              Feature-based page components
    src/api/                REST client and SignalR connections
    src/types/              TypeScript type definitions
```
