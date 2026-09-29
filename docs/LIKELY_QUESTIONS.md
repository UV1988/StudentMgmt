# Likely panel questions, with answers you can say in your own words

## About the architecture

**Q: Walk me through what happens when I click "Add student."**
A: See `WALKTHROUGH.md` — briefly: client-side validation, then a POST to the API,
which validates again, builds the entity in the service layer, saves it via the
repository, and returns 201. The UI then refetches the list.

**Q: Why did you separate Controller / Service / Repository instead of putting
everything in the controller?**
A: Each layer has one job. It makes the service testable without a database (see
`StudentServiceTests.cs`), and keeps the controller focused on HTTP concerns only.

**Q: Why DTOs instead of just returning the EF entity directly?**
A: So the API's shape doesn't change every time the database schema does, and so we
control exactly what's exposed (e.g. not `CreatedAtUtc` if we didn't want to expose
it — though here we do expose the fields that matter).

**Q: Why SQLite? Would you use it in production?**
A: For this exercise it means zero setup — no server to install, works the same on any
machine. For a production system with concurrent writers, I'd move to SQL Server or
Postgres, which is a one-line change since it's behind EF Core.

## About validation and errors

**Q: What happens if I submit an invalid phone number?**
A: Client-side, `validateStudent()` catches it before any network call. If it somehow
reached the server, FluentValidation would reject it and the controller returns 400
with `{ "errors": { "Phone": [...] } }`, which the client maps back onto the phone
field.

**Q: Why validate on both client and server? Isn't that duplicated logic?**
A: The server validation is authoritative — nothing is trusted just because the client
allowed it. The client copy exists only for instant feedback so the user doesn't wait
on a round trip to learn a field is wrong.

**Q: What happens on an unexpected server error, like the database being locked?**
A: The `GlobalExceptionHandler` catches anything unhandled, logs the real exception
server-side, and returns a generic 500 message to the client — no internals or stack
traces are exposed.

## About testing

**Q: What's the difference between your two test projects/files?**
A: `StudentServiceTests.cs` is a unit test — it uses `FakeStudentRepository`, an
in-memory stand-in, so it tests the service's logic in isolation, fast, no database.
`StudentsApiTests.cs` is an integration test — it boots the real API via
`WebApplicationFactory<Program>` against a real (temporary) SQLite file, so it
exercises the full HTTP + validation + database path.

**Q: How would you test the React side more thoroughly?**
A: Right now only the validation logic has tests (`validation.test.ts`). I'd add
component tests with React Testing Library for the form and list — e.g. "shows an
error when name is blank," "shows the new student after a successful add."

## About extending it

**Q: How would you add an "edit student" feature?**
A: Add a `PUT /api/students/{id}` endpoint with its own DTO/validator, a
`UpdateAsync` on the repository and service, and an edit form (or make the add form
double as an edit form when a student is selected) in React.

**Q: How would you add pagination?**
A: On the API, add `page`/`pageSize` query params to `GET /api/students`, use
`.Skip().Take()` in the repository, and return a total count alongside the page. On
the client, add page controls and pass the params through `useStudents`.

**Q: How would you secure this?**
A: Put the API behind whatever identity provider the organization uses (e.g. an OIDC
provider), validate a bearer token in the API with `AddAuthentication`/
`AddAuthorization`, and have the React app handle sign-in and attach the token to
requests.

**Q: How would you scale this to a lot of students?**
A: Move off SQLite to a server database, add pagination and search/filtering on the
list endpoint (server-side, not loading everything into the browser), and add an
index on whatever field is searched most (name is already indexed).

## About your process

**Q: How did you use AI on this, and what did you check yourself?**
A: I used it to scaffold the boilerplate quickly — project structure, CRUD endpoints,
tests, Docker/CI setup. I read through everything, and the layering and validation
choices are things I can explain and would defend, not defaults I left unexamined.

**Q: What would you do differently with more time?**
A: The "What I'd add next" list in the README — EF migrations instead of
`EnsureCreated`, an edit endpoint, pagination, auth, and soft delete.
