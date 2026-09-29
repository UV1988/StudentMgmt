# Walkthrough: one request, start to finish

This traces exactly what happens when someone adds a student, and separately when
someone deletes one. If you can narrate these two flows in your own words, you can
answer most panel questions about this project.

## Adding a student

1. **UI (`StudentForm.tsx`)** — user types into the Name, Date of birth, and Phone
   fields. On submit, `validateStudent()` (`validation.ts`) checks the values
   client-side. If anything fails, we stop right there and show the error under the
   field — no network call yet.

2. **API call (`api.ts` → `createStudent`)** — a `fetch` `POST` to `/api/students`
   with a JSON body `{ name, dateOfBirth, phone }`. In dev, Vite's proxy
   (`vite.config.ts`) forwards `/api/*` to the .NET API on port 5080, so the browser
   never needs to know the API's real address.

3. **Controller (`StudentsController.Create`)** — ASP.NET Core model-binds the JSON
   into a `CreateStudentRequest` record. The controller runs
   `CreateStudentRequestValidator` (FluentValidation) explicitly. If invalid, it
   returns `400` with a `ValidationProblemDetails` body shaped like
   `{ "errors": { "Name": ["..."] } }` — this is why the client can map server errors
   back onto the right form field (`mapServerErrors` in `validation.ts`).

4. **Service (`StudentService.CreateAsync`)** — builds a `Student` entity: generates a
   new `Guid`, trims the strings, stamps `CreatedAtUtc` from the injected
   `TimeProvider` (not `DateTime.Now`, so it's testable). Logs, then calls the
   repository.

5. **Repository (`StudentRepository.AddAsync`)** — the only place that talks to EF
   Core directly. Adds the entity to the `DbContext` and calls `SaveChangesAsync`,
   which issues the `INSERT` against the SQLite file.

6. **Response** — the controller returns `201 Created` with a `Location` header
   pointing at `GET /api/students/{id}`, and the created student in the body.

7. **Back in the UI** — TanStack Query's `useAddStudent` mutation succeeds, which
   invalidates the `students` query. React Query automatically refetches the list, so
   the new row appears without a manual page reload or manual state update.

## Deleting a student

1. **UI (`StudentList.tsx`)** — clicking "Delete" doesn't delete immediately; it
   switches that row into a "confirm" state (`confirmingId`). Clicking the confirm
   button calls `useDeleteStudent().mutate(id)`.
2. **API call** — `DELETE /api/students/{id}`.
3. **Controller → Service → Repository** — `StudentRepository.DeleteAsync` uses EF
   Core's `ExecuteDeleteAsync`, which issues a `DELETE` directly in SQL without
   loading the row into memory first. Returns whether a row was actually deleted.
4. **Response** — `204 No Content` if it existed, `404` if it didn't (e.g. someone
   else already deleted it).
5. **UI** — on success, the `students` query is invalidated and the list refetches.

## Why this shape?

Each layer has exactly one job:
- **Controller** — HTTP concerns only (status codes, routing, binding).
- **Validator** — is this input well-formed?
- **Service** — the business logic ("what does creating a student mean?").
- **Repository** — the only layer that knows about EF Core / SQL.

That separation is why `StudentServiceTests.cs` can test the service's logic with a
`FakeStudentRepository` (no database at all), while `StudentsApiTests.cs` separately
tests the real thing end-to-end through an actual (throwaway) SQLite file.
