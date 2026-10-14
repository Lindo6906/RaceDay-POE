# RaceDay - Part 2: RESTful API

ASP.NET Core 8 Web API (C#) with Entity Framework Core (Code-First) on SQL Server, session-based
authentication, Swagger, and xUnit tests run by GitHub Actions.

> Copy the "Part 2" section below into the main README.md of the repository.

## Roles
| Role | Can do |
|------|--------|
| **Organiser** | Create / edit / delete their own events, define categories for them, view all enrolments for their events, capture finish times and positions. |
| **Participant** | Register, browse events and categories, enter an event by choosing a category, view / withdraw their own enrolments, view their own results. |

Both roles can register, log in, log out, and view / update their own profile.

## Setup and run
1. Install the .NET 8 SDK and SQL Server (LocalDB comes with Visual Studio) .
2. Open `Part2-API/RaceDay.sln` in Visual Studio (or use the CLI below).
3. Check the connection string in `RaceDay.Api/appsettings.json` (`DefaultConnection`).
4. Create the database with EF Core migrations (Code-First):
   ```bash
   cd Part2-API
   dotnet tool install --global dotnet-ef        # once
   dotnet ef migrations add InitialCreate --project RaceDay.Api
   dotnet ef database update --project RaceDay.Api
   ```
   (In Visual Studio: Package Manager Console -> `Add-Migration InitialCreate`, `Update-Database`.)
5. Run: `dotnet run --project RaceDay.Api` (or press F5). Swagger opens at `/swagger`.
6. Tests: `dotnet test RaceDay.sln` (uses the EF Core in-memory provider, no SQL Server needed).

### Using Swagger
Authentication is session (cookie) based, so no token is needed:
1. `POST /api/auth/register` - register as a Participant, or as an Organiser using `organiserCode`
   (configured in `appsettings.json` -> `Registration:OrganiserCode`, default `RACEDAY-ORG-2026`).
2. `POST /api/auth/login` - the browser stores the session cookie.
3. Call any other endpoint from the Swagger UI. `POST /api/auth/logout` ends the session.

## Design decisions
- **Role selection at registration:** the user chooses a role, but registering as an Organiser requires a secret
  code, otherwise anyone could make themselves an Organiser.
- **Passwords:** hashed with PBKDF2-HMAC-SHA256 (100 000 iterations, random 16-byte salt). Plain text is never stored.
- **Sessions:** `UserId` and `Role` are stored server-side in the ASP.NET Core session. A `[SessionAuthorize]`
  filter returns **401** when not logged in and **403** when the role is wrong.
- **Ownership:** an Organiser can only change / view enrolments and results of events they created.
- **Data rules:** a participant can enrol in an event once; the chosen category must belong to that event;
  one result per enrolment; finishing positions are unique within an event.

## Endpoints
| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| POST | /api/auth/register | Public | Create an account |
| POST | /api/auth/login | Public | Start a session |
| POST | /api/auth/logout | Public | End the session |
| GET | /api/profile | Any | View own profile |
| PUT | /api/profile | Any | Update own profile |
| GET | /api/events?type= | Any | List events (filter by Run/Walk/Cycle) |
| GET | /api/events/{id} | Any | Get one event |
| POST | /api/events | Organiser | Create event |
| PUT | /api/events/{id} | Organiser (owner) | Update event |
| DELETE | /api/events/{id} | Organiser (owner) | Delete event |
| GET | /api/events/{eventId}/categories | Any | List categories |
| POST | /api/events/{eventId}/categories | Organiser (owner) | Add category |
| PUT | /api/categories/{id} | Organiser (owner) | Update category |
| DELETE | /api/categories/{id} | Organiser (owner) | Delete category |
| POST | /api/events/{eventId}/enrolments | Participant | Enter an event with a category |
| GET | /api/enrolments/my | Participant | Own enrolments |
| DELETE | /api/enrolments/{id} | Participant (owner) | Withdraw |
| GET | /api/events/{eventId}/enrolments | Organiser (owner) | Enrolments for an event |
| GET | /api/enrolments | Organiser | Enrolments across own events |
| POST | /api/enrolments/{enrolmentId}/result | Organiser (owner) | Capture finish time and position |
| PUT | /api/results/{id} | Organiser (owner) | Correct a result |
| GET | /api/events/{eventId}/results | Organiser (owner) | Results for an event |
| GET | /api/results/my | Participant | Own results |

## Differences from the Part 1 plan
The database follows the **Part 1 ERD** (`PROG_Part1.drawio.png`). Deviations, with reasons:

| Area | Part 1 | Part 2 | Reason |
|------|--------|--------|--------|
| Events | No distance or event type | Added `DistanceKm` and `EventType` (Run / Walk / Cycle) | Required by the Part 2 brief |
| Results | Status column | Added to the API (`Pending`, `Completed`, `Disqualified`) | Kept from the ERD / SQL |
| SQL script vs ERD | SQL script uses `FirstName`/`LastName`, a `Role` text column, global `Categories` + `EventCategories` junction, `MaxParticipants`, `IsActive` | API follows the ERD: `FullName`, `Roles` table, `Categories` belong to one event, `Enrolments` link to `CategoryID` | The ERD and SQL script disagreed; the ERD is the design document. **Update or explain the SQL script in the README.** |
| Enrolment rule | Unique (ParticipantID, CategoryID) | Also enforced in code: one enrolment per event | Business rule: you cannot enter two categories of the same race |
| Passwords | Sample data has placeholder hashes | Real PBKDF2 hashes | Seed users from the Part 1 script cannot log in; register through the API |
| Endpoints | 29 endpoints in `API Endpoint Plan.md.pdf` | 23 endpoints (table above) | _Compare with your plan and list every route that differs._ |

## CI/CD
Workflow: `.github/workflows/part2-ci.yml` (restore, build, test on every push).

![CI green build](docs/ci-green-build.png)

## Video
[Part 2 walkthrough (unlisted YouTube)](PASTE-LINK-HERE)
