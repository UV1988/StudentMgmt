# Cheat sheet: "why did you..."

One line each. Read this a couple of times before the interview — you don't need to
memorize it, just recognize the reasoning as your own.

| Choice | Why |
|---|---|
| SQLite | Zero setup for whoever reviews it — no server to install, `docker compose up` just works. Swappable for SQL Server/Postgres since it's behind EF Core. |
| `Database.EnsureCreated()` not migrations | Fine for a schema that won't evolve after submission. Real project → EF migrations from day one. |
| Repository interface (`IStudentRepository`) | Lets the service be unit-tested without a real database, and gives a seam to swap storage later. |
| Service layer between controller and repository | Keeps controllers thin (HTTP only); business rules (trimming, timestamps, logging) live in one testable place. |
| DTOs separate from the `Student` entity | The API contract shouldn't change just because the database schema does. |
| FluentValidation, server-side | Single source of truth for validity. The client mirrors the same rules only for instant feedback — the server response is what's trusted. |
| `TimeProvider` instead of `DateTime.Now` | Makes "date of birth can't be in the future" deterministic in tests (`FixedTimeProvider`). |
| `ExecuteDeleteAsync` for delete | One SQL `DELETE`, no need to load the row into memory first. |
| Global exception handler | Anything unhandled gets logged server-side and returns a generic message — no stack traces leak to the client. |
| Two-step delete confirm in the UI | Prevents an accidental click from removing a record; no server change needed for this. |
| React plain state + TanStack Query, no Redux | The app has one list and one form — a global state library would be overhead with nothing to manage. |
| Vite proxy for `/api` in dev | No hard-coded API URL in the frontend code, and no CORS friction locally. |
| No auth | Out of scope for the brief; noted explicitly as a "what's next" item. |
| Docker + docker-compose | One command to run the whole thing; also how you'd deploy it as containers. |
| Unit tests + integration tests | Unit tests check logic in isolation (fast, no DB); integration tests check the real HTTP + SQL path end-to-end. |

## If you go blank

It's fine to say: "I used AI to scaffold this quickly, then reviewed it — here's what I
remember about why it's built this way..." and talk through the layers (controller →
service → repository) even if you can't recall one specific detail. That's an honest,
reasonable answer to the brief, which explicitly invited AI use.
