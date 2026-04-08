# PRD: Medical Simulation Training Platform (SimTrainer)

## 1. Project Overview

**SimTrainer** is a medical simulation and training analytics platform that demonstrates the core engineering challenges across all three of Laerdal Medical's hiring teams. It provides:

- **Real-time CPR quality measurement and feedback** (RQI team) -- a simulated manikin sends compression data via WebSocket; the system scores each compression against AHA guidelines (rate 100-120/min, depth 5-6cm, full recoil) and tracks learner performance over time.
- **Simulation session analytics with AI-driven insights** (Sim to Improve team) -- after a training session completes, the platform analyzes workflow patterns, identifies bottlenecks, and suggests quality improvements using statistical analysis.
- **Simulated instrument control and real-time device feedback** (Simulated Instruments team) -- a virtual patient monitor displays vitals (heart rate, SpO2, blood pressure, respiratory rate) that respond in real time to trainee actions, simulating how physical devices behave during medical training.

The platform is one unified application: a .NET backend serves a React frontend, with SignalR providing real-time communication and SQL Server storing all training data. A device simulator generates realistic physiological data so the system works without any physical hardware.

### Why This Project

Laerdal's mission is helping save lives through better training. This project demonstrates exactly the kind of software Laerdal builds: connecting devices to learning flows, providing real-time feedback, and using data to improve healthcare outcomes. It naturally exercises the full tech stack (C#/.NET, React, Azure-compatible patterns, REST APIs, databases, real-time systems) while solving a realistic domain problem.

## 2. Technical Architecture

```
+------------------+     WebSocket/SignalR      +-------------------+
|                  | <========================> |                   |
|   React SPA      |     REST API (HTTP)        |   .NET 8 API     |
|   (Frontend)     | <========================> |   (Backend)       |
|                  |                            |                   |
|  - CPR Trainer   |                            |  - REST Controllers|
|  - Patient       |                            |  - SignalR Hubs   |
|    Monitor       |                            |  - Domain Services|
|  - Analytics     |                            |  - Background     |
|    Dashboard     |                            |    Services       |
|                  |                            |                   |
+------------------+                            +--------+----------+
                                                         |
                                                         |  EF Core
                                                         |
                                                +--------v----------+
                                                |                   |
                                                |   SQL Server      |
                                                |   (Database)      |
                                                |                   |
                                                +-------------------+

Internal to Backend:
+-------------------+
| Device Simulator  |  (Background service generating realistic
| Service           |   physiological data - heart rate, SpO2, etc.)
+-------------------+
```

### Key Components

1. **.NET 8 Web API** -- The core backend. Hosts REST endpoints for CRUD operations, SignalR hubs for real-time data, and background services for device simulation. Uses clean architecture with domain/application/infrastructure layers.

2. **SignalR Hubs** -- Two real-time channels:
   - `CprHub` -- streams compression events from the simulator and returns instant quality scores to the frontend.
   - `MonitorHub` -- streams vital signs (HR, SpO2, BP, RR) to the patient monitor UI, with the ability to trigger scenarios (e.g., cardiac arrest, respiratory failure).

3. **Device Simulator Service** -- A hosted background service that generates physiological data using sine waves with noise, realistic drift, and configurable patient scenarios. This replaces the physical manikin/devices for demo purposes.

4. **React SPA** -- Three main views: CPR Training (real-time compression feedback), Patient Monitor (vital signs display with scenario controls), and Analytics Dashboard (session history, performance trends, improvement suggestions).

5. **SQL Server** -- Stores training sessions, compression events, vital sign snapshots, learner profiles, and analytics results. Accessed via Entity Framework Core.

### Data Flow

1. **CPR Training Flow**: Device simulator generates compression events -> SignalR pushes to backend -> backend scores against AHA guidelines in real time -> score + feedback pushed to React via SignalR -> compression data persisted to database for analytics.

2. **Patient Monitor Flow**: Scenario selected in React UI -> REST call triggers scenario on backend -> Device Simulator adjusts vital generation parameters -> SignalR streams vitals to React at 1Hz -> React renders real-time waveforms and numeric displays.

3. **Analytics Flow**: React requests session data via REST -> Backend queries database for session history -> Computes statistics (avg compression quality, response times, improvement trends) -> Returns structured analytics with improvement suggestions.

## 3. Tech Stack

| Technology | Usage | Rationale |
|---|---|---|
| **C# / .NET 8** | Backend API, domain logic, background services | Primary backend language in job listing |
| **ASP.NET Core** | REST API + SignalR real-time | Standard .NET web framework, SignalR for real-time |
| **Entity Framework Core** | Database ORM | Standard .NET data access, code-first migrations |
| **SQL Server 2022** | Relational database | Production-grade DB, runs in Docker on Linux |
| **React 18 + TypeScript** | Frontend SPA | Required by job listing |
| **Vite** | Frontend build tool | Fast, modern React tooling |
| **SignalR** | WebSocket real-time communication | .NET-native real-time library, used for device-to-UI streaming |
| **Recharts** | Charts and analytics visualization | Lightweight React charting library |
| **xUnit + Moq** | Backend unit/integration testing | .NET testing standard |
| **React Testing Library** | Frontend testing | React testing standard |
| **Docker + Docker Compose** | Local development and deployment | Everything runs with `docker compose up` |
| **GitHub Actions** | CI/CD pipeline | Industry standard, demonstrates CI/CD practices |

## 4. Features & Acceptance Criteria

### Feature 1: Real-Time CPR Quality Scoring (RQI)

A trainee starts a CPR session and receives instant feedback on each chest compression.

**Acceptance Criteria:**
- Trainee clicks "Start Session" and the simulated manikin begins generating compression events
- Each compression is scored in real time against AHA guidelines:
  - Rate: 100-120 compressions/min (optimal), displayed as current rate
  - Depth: 5-6 cm (optimal), color-coded green/yellow/red
  - Recoil: full chest recoil required, boolean pass/fail
- A running quality score (0-100%) updates after each compression
- Visual feedback bar shows depth and rate in real time with < 100ms latency from event generation to UI update
- Session summary shows total compressions, average quality, and areas for improvement
- Session data is persisted to the database

### Feature 2: Simulated Patient Monitor (Simulated Instruments)

A virtual patient monitor displays vital signs that respond to scenario changes, simulating a real bedside monitor.

**Acceptance Criteria:**
- Monitor displays four vital signs with numeric values and waveforms: Heart Rate, SpO2, Blood Pressure, Respiratory Rate
- Vitals update at 1Hz with realistic physiological values and minor random variation
- Instructor can select from predefined scenarios: Normal, Tachycardia, Bradycardia, Hypoxia, Cardiac Arrest
- When a scenario is activated, vitals transition smoothly over 5-10 seconds (not instant jumps)
- Cardiac Arrest scenario triggers alarms (visual: flashing red, audio: optional browser beep)
- Waveform rendering uses a scrolling line chart that resembles a real patient monitor
- Monitor can be opened in a separate browser tab for projection during training

### Feature 3: Training Session Analytics Dashboard (Sim to Improve)

An analytics view that helps training coordinators understand performance trends and identify improvement areas.

**Acceptance Criteria:**
- Dashboard shows a list of all completed CPR training sessions with date, duration, and overall score
- Clicking a session shows detailed metrics: compression-by-compression quality, rate over time, depth distribution histogram
- Trend chart shows quality scores across sessions for a given learner, demonstrating improvement over time
- Summary statistics: average quality score, best session, areas consistently below threshold
- Improvement suggestions are generated based on the data (rule-based): e.g., "Compression depth is consistently below 5cm -- focus on pushing harder" or "Rate tends to drift above 120/min after 60 seconds -- practice pacing"
- Data can be filtered by date range and learner

### Feature 4: Learner Management

Basic learner profiles to associate sessions with individuals.

**Acceptance Criteria:**
- Create a learner with name and role (e.g., "Nurse", "Medical Student", "Paramedic")
- List all learners with their session count and average score
- View a learner's session history
- REST API supports full CRUD for learners

### Feature 5: Scenario Management for Simulated Instruments

Instructors can define and trigger patient scenarios during training.

**Acceptance Criteria:**
- System includes 5 predefined scenarios with physiologically accurate vital ranges:
  - Normal: HR 60-100, SpO2 95-100, BP 120/80, RR 12-20
  - Tachycardia: HR 130-160, SpO2 92-96, BP 100/65, RR 18-24
  - Bradycardia: HR 35-50, SpO2 94-98, BP 90/60, RR 10-14
  - Hypoxia: HR 110-130, SpO2 75-88, BP 130/85, RR 24-32
  - Cardiac Arrest: HR 0, SpO2 0, BP 0/0, RR 0 (flatline)
- Active scenario is visible in the UI with clear labeling
- Switching scenarios transitions vitals smoothly (gradual change, not a step function)
- Scenario changes are logged with timestamps for post-session review

### Feature 6: CI/CD Pipeline

Automated build, test, and quality checks.

**Acceptance Criteria:**
- GitHub Actions workflow triggers on push and pull request
- Backend: restore, build, run unit tests, run integration tests
- Frontend: install, lint, build, run tests
- Docker image builds successfully
- Pipeline completes in under 5 minutes
- Status badges in README

## 5. Data Models

### Entity Relationship

```
Learner 1──* TrainingSession 1──* CompressionEvent
                    |
                    1──* VitalSnapshot
                    |
                    1──* ScenarioChange
```

### Entities

#### Learner
| Field | Type | Description |
|---|---|---|
| Id | Guid (PK) | Unique identifier |
| Name | string | Full name |
| Role | string | Professional role (Nurse, Paramedic, etc.) |
| CreatedAt | DateTime | Registration timestamp |

#### TrainingSession
| Field | Type | Description |
|---|---|---|
| Id | Guid (PK) | Unique identifier |
| LearnerId | Guid (FK) | Associated learner |
| SessionType | enum | `CprTraining` or `PatientMonitoring` |
| StartedAt | DateTime | Session start |
| EndedAt | DateTime? | Session end (null if active) |
| OverallScore | decimal? | Computed quality score (0-100) for CPR sessions |

#### CompressionEvent
| Field | Type | Description |
|---|---|---|
| Id | long (PK) | Auto-increment |
| SessionId | Guid (FK) | Parent session |
| Timestamp | DateTime | When compression occurred |
| DepthCm | decimal | Compression depth in centimeters |
| RateBpm | int | Instantaneous rate (based on interval from previous) |
| FullRecoil | bool | Whether full chest recoil was achieved |
| QualityScore | decimal | Individual compression quality (0-100) |

#### VitalSnapshot
| Field | Type | Description |
|---|---|---|
| Id | long (PK) | Auto-increment |
| SessionId | Guid (FK) | Parent session |
| Timestamp | DateTime | Snapshot time |
| HeartRate | int | Beats per minute |
| SpO2 | int | Oxygen saturation percentage |
| SystolicBp | int | Systolic blood pressure mmHg |
| DiastolicBp | int | Diastolic blood pressure mmHg |
| RespiratoryRate | int | Breaths per minute |

#### ScenarioChange
| Field | Type | Description |
|---|---|---|
| Id | long (PK) | Auto-increment |
| SessionId | Guid (FK) | Parent session |
| Timestamp | DateTime | When scenario was activated |
| ScenarioName | string | Name of the scenario |
| PreviousScenario | string? | Name of the previous scenario |

## 6. API Design

Base URL: `/api`

### Learners

| Method | Endpoint | Description |
|---|---|---|
| GET | `/learners` | List all learners |
| GET | `/learners/{id}` | Get learner with session summary |
| POST | `/learners` | Create learner |
| PUT | `/learners/{id}` | Update learner |
| DELETE | `/learners/{id}` | Delete learner |

**POST /learners**
```json
// Request
{ "name": "Anna Olsen", "role": "Nurse" }
// Response 201
{ "id": "guid", "name": "Anna Olsen", "role": "Nurse", "createdAt": "2026-04-07T10:00:00Z" }
```

### Training Sessions

| Method | Endpoint | Description |
|---|---|---|
| GET | `/sessions` | List sessions (supports `?learnerId=`, `?type=`, `?from=`, `?to=`) |
| GET | `/sessions/{id}` | Get session with all events |
| POST | `/sessions` | Start a new session |
| PUT | `/sessions/{id}/end` | End a session and compute final score |
| GET | `/sessions/{id}/compressions` | Get compression events for a session |
| GET | `/sessions/{id}/vitals` | Get vital snapshots for a session |
| GET | `/sessions/{id}/scenarios` | Get scenario changes for a session |

**POST /sessions**
```json
// Request
{ "learnerId": "guid", "sessionType": "CprTraining" }
// Response 201
{ "id": "guid", "learnerId": "guid", "sessionType": "CprTraining", "startedAt": "2026-04-07T10:05:00Z" }
```

### Analytics

| Method | Endpoint | Description |
|---|---|---|
| GET | `/analytics/learner/{id}` | Learner performance summary and trends |
| GET | `/analytics/session/{id}` | Detailed session analytics with improvement suggestions |

**GET /analytics/learner/{id}**
```json
{
  "learnerId": "guid",
  "totalSessions": 12,
  "averageScore": 78.5,
  "bestScore": 95.0,
  "recentTrend": "improving",
  "sessionScores": [
    { "sessionId": "guid", "date": "2026-04-01", "score": 72.0 },
    { "sessionId": "guid", "date": "2026-04-03", "score": 81.0 }
  ],
  "suggestions": [
    "Compression depth is consistently below 5cm -- focus on pushing harder",
    "Rate stability has improved significantly over the last 5 sessions"
  ]
}
```

### Scenarios (Patient Monitor)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/scenarios` | List available scenarios with vital ranges |
| POST | `/scenarios/activate` | Activate a scenario for current monitor session |

**POST /scenarios/activate**
```json
// Request
{ "sessionId": "guid", "scenarioName": "Tachycardia" }
// Response 200
{ "activated": true, "scenarioName": "Tachycardia", "transitionDurationMs": 8000 }
```

### SignalR Hubs

**`/hubs/cpr`** -- CPR Training Hub
- Server -> Client: `ReceiveCompression(CompressionEvent)` -- real-time compression with score
- Server -> Client: `SessionScore(decimal overallScore)` -- updated running score
- Client -> Server: `StartSession(Guid sessionId)` -- begin receiving events
- Client -> Server: `StopSession(Guid sessionId)` -- stop session

**`/hubs/monitor`** -- Patient Monitor Hub
- Server -> Client: `ReceiveVitals(VitalSnapshot)` -- vital signs at 1Hz
- Server -> Client: `ScenarioChanged(string scenarioName)` -- notification of scenario change
- Server -> Client: `AlarmTriggered(string alarmType)` -- critical alarm
- Client -> Server: `StartMonitoring(Guid sessionId)` -- begin receiving vitals
- Client -> Server: `StopMonitoring(Guid sessionId)` -- stop monitoring

## 7. Testing Strategy

### Backend Unit Tests (xUnit + Moq)

**Domain logic (highest priority):**
- CPR quality scoring: test edge cases for rate, depth, recoil scoring
- Verify correct quality score computation for compressions at boundary values (e.g., exactly 100 bpm, exactly 5cm depth)
- Analytics computation: verify trend calculation, suggestion generation logic
- Scenario transition: verify vital value interpolation between scenarios

**Service layer:**
- Session management: start, end, score computation
- Learner CRUD operations
- Analytics aggregation queries

**Target: Cover all domain logic and service methods with unit tests.**

### Backend Integration Tests

- API endpoint tests using `WebApplicationFactory<Program>` with an in-memory database
- Test full request/response cycle for all REST endpoints
- Verify EF Core query correctness against actual SQL (using Testcontainers or in-memory provider)

### Frontend Tests (React Testing Library + Vitest)

- Component rendering: verify each view renders correctly with mock data
- User interactions: start/stop session buttons, scenario selection
- SignalR mock: verify UI updates when receiving real-time events
- Analytics charts: verify data is correctly mapped to chart components

### CI Pipeline

- All tests run on every push via GitHub Actions
- Backend and frontend tests run in parallel jobs
- Docker build as final validation step

## 8. Infrastructure & Deployment

### Docker Compose (Local Development)

Three services:

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "CHANGE_ME"
    ports:
      - "1433:1433"

  api:
    build: ./src/SimTrainer.Api
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=SimTrainer;User=sa;Password=CHANGE_ME;TrustServerCertificate=true"
    ports:
      - "5000:8080"
    depends_on:
      db:
        condition: service_healthy

  frontend:
    build: ./src/frontend
    ports:
      - "3000:80"
    depends_on:
      - api
```

The API container runs EF Core migrations on startup automatically, so no manual database setup is needed.

### GitHub Actions CI/CD

```
.github/workflows/ci.yml
  - Job 1: Backend (restore -> build -> test)
  - Job 2: Frontend (install -> lint -> build -> test)
  - Job 3: Docker (build all images, verify compose starts)
```

## 9. Project Structure

```
SimTrainer/
|-- docker-compose.yml
|-- README.md
|-- docs/
|   |-- PRD.md
|-- .github/
|   |-- workflows/
|       |-- ci.yml
|-- src/
|   |-- SimTrainer.Api/
|   |   |-- Dockerfile
|   |   |-- Program.cs
|   |   |-- SimTrainer.Api.csproj
|   |   |-- Controllers/
|   |   |   |-- LearnersController.cs
|   |   |   |-- SessionsController.cs
|   |   |   |-- AnalyticsController.cs
|   |   |   |-- ScenariosController.cs
|   |   |-- Hubs/
|   |   |   |-- CprHub.cs
|   |   |   |-- MonitorHub.cs
|   |   |-- Domain/
|   |   |   |-- Entities/
|   |   |   |   |-- Learner.cs
|   |   |   |   |-- TrainingSession.cs
|   |   |   |   |-- CompressionEvent.cs
|   |   |   |   |-- VitalSnapshot.cs
|   |   |   |   |-- ScenarioChange.cs
|   |   |   |-- Enums/
|   |   |   |   |-- SessionType.cs
|   |   |   |-- Scoring/
|   |   |       |-- CprQualityScorer.cs
|   |   |       |-- AnalyticsEngine.cs
|   |   |-- Services/
|   |   |   |-- SessionService.cs
|   |   |   |-- LearnerService.cs
|   |   |   |-- DeviceSimulatorService.cs
|   |   |   |-- ScenarioService.cs
|   |   |-- Infrastructure/
|   |   |   |-- SimTrainerDbContext.cs
|   |   |   |-- Migrations/
|   |   |-- Simulation/
|   |       |-- CprSimulator.cs
|   |       |-- VitalSignsGenerator.cs
|   |       |-- PatientScenario.cs
|   |-- SimTrainer.Tests/
|   |   |-- SimTrainer.Tests.csproj
|   |   |-- Domain/
|   |   |   |-- CprQualityScorerTests.cs
|   |   |   |-- AnalyticsEngineTests.cs
|   |   |-- Services/
|   |   |   |-- SessionServiceTests.cs
|   |   |   |-- LearnerServiceTests.cs
|   |   |-- Integration/
|   |       |-- LearnersApiTests.cs
|   |       |-- SessionsApiTests.cs
|   |       |-- AnalyticsApiTests.cs
|   |-- frontend/
|       |-- Dockerfile
|       |-- package.json
|       |-- tsconfig.json
|       |-- vite.config.ts
|       |-- src/
|           |-- main.tsx
|           |-- App.tsx
|           |-- api/
|           |   |-- client.ts          (REST API client)
|           |   |-- signalr.ts         (SignalR connection manager)
|           |-- components/
|           |   |-- Layout.tsx
|           |   |-- Navigation.tsx
|           |-- pages/
|           |   |-- CprTraining/
|           |   |   |-- CprTrainingPage.tsx
|           |   |   |-- CompressionFeedback.tsx
|           |   |   |-- QualityScoreBar.tsx
|           |   |   |-- SessionSummary.tsx
|           |   |-- PatientMonitor/
|           |   |   |-- PatientMonitorPage.tsx
|           |   |   |-- VitalDisplay.tsx
|           |   |   |-- Waveform.tsx
|           |   |   |-- ScenarioSelector.tsx
|           |   |   |-- AlarmOverlay.tsx
|           |   |-- Analytics/
|           |   |   |-- AnalyticsDashboard.tsx
|           |   |   |-- SessionList.tsx
|           |   |   |-- PerformanceTrend.tsx
|           |   |   |-- CompressionDetail.tsx
|           |   |   |-- Suggestions.tsx
|           |   |-- Learners/
|           |       |-- LearnersPage.tsx
|           |       |-- LearnerForm.tsx
|           |-- types/
|           |   |-- index.ts
|           |-- __tests__/
|               |-- CprTrainingPage.test.tsx
|               |-- PatientMonitorPage.test.tsx
|               |-- AnalyticsDashboard.test.tsx
```
