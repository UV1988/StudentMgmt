# Student Registry

A small full-stack app to add, list, and delete students (name, date of birth, phone).

- **Backend:** ASP.NET Core 8 Web API, EF Core, SQLite
- **Frontend:** React 18 + TypeScript, Vite, TanStack Query

## Running it

### Option A — Docker (no local tooling needed)

```bash
docker compose up --build
```

- Web UI: http://localhost:5173
- API + Swagger: http://localhost:5080/swagger

### Option B — Run locally

**Backend**
```bash
cd backend/src/StudentManager.Api
dotnet run
```
API runs at `http://localhost:5080`, Swagger at `/swagger`, health check at `/health`.

**Frontend** (separate terminal)
```bash
cd frontend
npm install
npm run dev
```
UI runs at `http://localhost:5173` and proxies `/api/*` to the backend (see `vite.config.ts`).

> **Note:** this repo was put together in an environment without internet access to run
> `npm install` or `dotnet build`, so dependencies could not be restored or the build
> verified end-to-end before committing. Everything was written and reviewed carefully,
> but please run both `dotnet build` / `dotnet test` and `npm install` / `npm run build`
> / `npm test` locally before you rely on this, and commit the generated
> `frontend/package-lock.json`.

### Running tests

```bash
# backend: unit tests (validator, service) + integration tests (real API, throwaway SQLite file)
cd backend && dotnet test

# frontend: validation logic
cd frontend && npm run test
```

## API

| Method | Route              | Purpose                        | Success | Failure          |
|--------|---------------------|---------------------------------|---------|-------------------|
| GET    | `/api/students`     | List students, ordered by name  | 200     | —                 |
| GET    | `/api/students/{id}`| Get one student                 | 200     | 404               |
| POST   | `/api/students`     | Create a student                | 201     | 400 (validation)  |
| DELETE | `/api/students/{id}`| Delete a student                | 204     | 404               |

Validation errors come back as an RFC 9457 `ValidationProblemDetails` body, e.g.:

```json
{ "errors": { "Name": ["Enter the student's name."] } }
```

## Project layout

```
backend/
  src/StudentManager.Api/
    Domain/        Student entity
    Data/          AppDbContext, IStudentRepository (EF Core)
    Dtos/          request/response shapes, kept separate from the entity
    Validation/     FluentValidation rules
    Services/       business logic, talks to the repository via its interface
    Controllers/    thin HTTP layer — validates, calls the service, maps to status codes
    Infrastructure/ GlobalExceptionHandler (catches anything unhandled)
  tests/StudentManager.Tests/
    CreateStudentRequestValidatorTests.cs  — validator rules
    StudentServiceTests.cs                 — service logic, against a fake repository
    StudentsApiTests.cs                    — real API + real SQLite, via WebApplicationFactory
frontend/
  src/
    api.ts          fetch wrappers + ApiError
    hooks.ts         TanStack Query hooks (useStudents, useAddStudent, useDeleteStudent)
    validation.ts     client-side rules, mirrors the server's
    components/       StudentForm, StudentList
```

## Design decisions (and what I'd change for a bigger system)

- **SQLite, not SQL Server/Postgres.** Zero setup for a reviewer — `docker compose up`
  and it works, no connection string or server to install. It sits behind EF Core, so
  swapping to SQL Server or Postgres is a one-line change in `Program.cs`
  (`UseSqlite` → `UseSqlServer`/`UseNpgsql`) plus the connection string.
- **`Database.EnsureCreated()`, not EF migrations.** Fine for a project this size where
  the schema won't change after submission. For a real project I'd use
  `dotnet ef migrations` from day one, so schema changes are versioned and reviewable.
- **Repository + service layers**, even though the logic is simple today. It keeps
  controllers thin, makes the service testable without a real database (see
  `StudentServiceTests.cs`, which uses `FakeStudentRepository`), and gives a natural
  seam if the rules grow (e.g. duplicate-student checks, soft delete).
- **DTOs separate from the EF entity.** The API's shape shouldn't be forced to match
  the database's shape — it protects the contract from schema churn.
- **Validation in two places, one source of truth.** FluentValidation on the server is
  authoritative and always runs. The same rules are mirrored in `validation.ts` purely
  for instant feedback; the server response is still what the UI trusts.
- **`TimeProvider` instead of `DateTime.Now`.** Makes "date of birth can't be in the
  future" deterministic and testable (see `FixedTimeProvider` in the tests).
- **No auth.** Out of scope for the brief. In a real system this would sit behind
  whatever the organization's identity provider is, with the API validating a bearer
  token and the UI handling sign-in.
- **Hard delete, no confirmation on the server.** The UI has a two-step confirm; a
  production version might soft-delete so records can be recovered.
- **No pagination.** Fine for a small list; I'd add it before this saw thousands of rows.

## What I'd add next

- EF Core migrations instead of `EnsureCreated`
- Edit/update endpoint
- Pagination and search on the list endpoint
- Authentication/authorization
- Soft delete with an "undo" in the UI
- Structured logging sink (e.g. Seq/Application Insights) instead of console only

## How AI was used

AI was used to scaffold the boilerplate (project structure, CRUD endpoints, DTOs,
tests, Docker/CI setup) quickly. All of it was reviewed, and the design decisions above
reflect deliberate choices, not defaults left unexamined.
