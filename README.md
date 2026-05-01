# Aihrly ATS API

An ASP.NET Core Web API for the Aihrly Applicant Tracking System — the team-side pipeline that recruiters and hiring managers use to manage job applications through a hiring workflow.

## Tech Stack

- **Runtime:** .NET 10 (ASP.NET Core)
- **Database:** PostgreSQL via EF Core 10 + Npgsql
- **Testing:** xUnit, SQLite in-memory for integration tests
- **Configuration:** DotNetEnv (`.env` files)
- **API Documentation:** OpenAPI / Swagger

## Quick Start

### Prerequisites

- .NET 10 SDK
- PostgreSQL (running locally or via Docker)

### 1. Set up PostgreSQL

If you're using Docker:

```bash
docker run --name postgres -e POSTGRES_HOST_AUTH_METHOD=trust -p 5432:5432 -d postgres:18
docker exec postgres createdb -U postgres aihrly
```

### 2. Configure environment

Create a `.env` file at the project root:

```
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=aihrly
POSTGRES_USER=postgres
```

### 3. Run migrations

```bash
dotnet ef database update
```

This creates all tables, indexes, foreign keys, and seeds 3 team members.

### 4. Run the API

```bash
dotnet run
```

The API starts at `https://localhost:5001` (or the port shown in output). OpenAPI docs are available at `/openapi/v1.json` in development mode.

## Running Tests

```bash
dotnet test Tests/Aihrly.Tests.csproj
```

All 17 tests run against a SQLite in-memory database — no external database needed:

- **14 unit tests** for stage transition validation (8 valid + 6 invalid transitions)
- **3 integration tests**:
  - Create application → add note → verify author name resolution
  - Submit score twice → verify second value wins
  - Duplicate application (same email + same job) → returns 409

## API Endpoints

### Jobs

| Method | Path | Description | Auth Required |
|--------|------|-------------|---------------|
| POST | `/api/jobs` | Create a job | No |
| GET | `/api/jobs` | List jobs (filter: `?status=open`, paginate: `?page=1&pageSize=20`) | No |
| GET | `/api/jobs/{id}` | Get a single job | No |

### Applications

| Method | Path | Description | Auth Required |
|--------|------|-------------|---------------|
| POST | `/api/jobs/{jobId}/applications` | Submit an application (name, email, optional cover letter) | No |
| GET | `/api/jobs/{jobId}/applications` | List applications for a job (filter: `?stage=screening`) | No |
| GET | `/api/applications/{id}` | Full applicant profile (scores, notes with authors, stage history) | No |
| PATCH | `/api/applications/{id}/stage` | Move application to a new stage | `X-Team-Member-Id` |

### Notes

| Method | Path | Description | Auth Required |
|--------|------|-------------|---------------|
| POST | `/api/applications/{id}/notes` | Add a note (type, description) | `X-Team-Member-Id` |
| GET | `/api/applications/{id}/notes` | List notes (newest first, with author names) | No |

### Scores

| Method | Path | Description | Auth Required |
|--------|------|-------------|---------------|
| PUT | `/api/applications/{id}/scores/culture-fit` | Update culture fit score (1-5) | `X-Team-Member-Id` |
| PUT | `/api/applications/{id}/scores/interview` | Update interview score (1-5) | `X-Team-Member-Id` |
| PUT | `/api/applications/{id}/scores/assessment` | Update assessment score (1-5) | `X-Team-Member-Id` |

## Team Member Seed Data

The database is seeded with 3 team members at startup:

| ID | Name | Email | Role |
|----|------|-------|------|
| `11111111-1111-1111-1111-111111111111` | Sarah Chen | sarah@aihrly.com | recruiter |
| `22222222-2222-2222-2222-222222222222` | Marcus Johnson | marcus@aihrly.com | hiring_manager |
| `33333333-3333-3333-3333-333333333333` | Priya Patel | priya@aihrly.com | recruiter |

Use these IDs in the `X-Team-Member-Id` header for mutating requests. Example:

```bash
curl -X PATCH http://localhost:5000/api/applications/{id}/stage \
  -H "Content-Type: application/json" \
  -H "X-Team-Member-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"targetStage": "screening", "reason": "Looks promising"}'
```

## Pipeline Stages

Valid stage transitions:

```
applied    → screening, rejected
screening  → interview, rejected
interview  → offer, rejected
offer      → hired, rejected
hired      → (terminal)
rejected   → (terminal)
```

Invalid transitions return `400` with a descriptive error message.

## Project Structure

```
Aihrly/
├── Controllers/          # API endpoints
├── Data/                 # DbContext and migrations
├── Dtos/                 # Request/response DTOs
├── Filters/              # Action filters (X-Team-Member-Id validation)
├── Models/               # Entity classes
├── Services/             # Business logic (stage transition rules)
├── Tests/                # Unit and integration tests
└── Program.cs            # Application entry point and DI setup
```

## Assumptions

- No real authentication — `X-Team-Member-Id` header is trusted for actor identification
- Jobs always start with status `open` — no endpoint to close a job was required
- Score values (1-5) are stored as integers on the Application entity — no separate scoring entities with history
- The duplicate application constraint is enforced at the database level via a unique index on `(JobId, CandidateEmail)`
- Error responses use ASP.NET Core's built-in `ProblemDetails` format (`application/problem+json`)

## What I'd Improve with More Time

- **Score history tracking:** A separate `ApplicationScore` table with one row per update would provide full audit history of who scored what and when.
- **Input validation with FluentValidation:** Moving validation logic out of controllers into dedicated validator classes for cleaner separation and easier testing.
- **AutoMapper for DTO mapping:** Reduce boilerplate in controller mapping code, especially for `ApplicationProfileDto` which maps 15+ fields.
- **Docker Compose setup:** A `docker-compose.yml` that brings up PostgreSQL + Redis + the API for a one-command local setup.
- **GitHub Actions CI:** Automated test runs on every push.
- **Job closure endpoint:** Ability to close a job (`PUT /api/jobs/{id}` to set status to `closed`).
