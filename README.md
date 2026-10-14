# RaceDay - Event Management System

## Overview
RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community. Event Organisers can create and manage events, categories, and participant results. Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day.

## System Features

### For Organisers
- **Event Management**: Create, edit, and delete running/cycling events
- **Category Management**: Define race categories (5km, 10km, 21km, 42km, etc.)
- **Participant Tracking**: View all enrolments for their events
- **Results Capture**: Record finish times and positions for participants

### For Participants
- **User Registration**: Create an account with role selection
- **Event Discovery**: Browse and search upcoming events
- **Event Registration**: Enrol in events by selecting a category
- **Performance History**: View personal results across all events

### Technical Specifications
- **Database**: SQL Server with normalized schema
- **API**: RESTful endpoints with role-based access control
- **Architecture**: Multi-tier (Database → API → Client)

## Part 1 - System Planning and Database

This repository contains the planning and database design for the RaceDay system.

---

## Folder Structure
```
RaceDay-POE/
├── docs/
│   ├── ERD.png
│   ├── API-Endpoint-Plan.md
│   └── RaceDay_Database.sql
├── README.md
└── CI-CD-Green-Build.png
```

---

## Documentation

### ERD Design Decisions

#### Users Table
Designed with a single `Users` table containing a `Role` field (Organiser/Participant) rather than separate tables. This simplifies authentication and reduces duplication while maintaining clear role separation.

#### EventCategories Junction
Uses a junction table to support many-to-many relationships between Events and Categories. This allows an event to offer multiple categories and a category to appear in multiple events.

#### Enrolments & Results Separation
Enrolments track participation registration, while Results store performance data. This separation allows:
- Participants to enrol before results are available
- Results to be updated without affecting enrolment status
- Historical tracking of both registration and performance
- 
**File:** `/docs/ERD.png`

---

### API Endpoint Categories

| Category | Description | Endpoints |
|----------|-------------|-----------|
| Authentication | User registration and login | 2 endpoints |
| User Management | Profile management | 3 endpoints |
| Event Management | CRUD operations | 5 endpoints |
| Category Management | CRUD operations | 5 endpoints |
| Event Categories | Junction management | 2 endpoints |
| Enrolments | Registration management | 6 endpoints |
| Results | Performance tracking | 6 endpoints |
| **Total** | **Complete RESTful API** | **29 endpoints** |

**File:** `/docs/API-Endpoint-Plan.md`

---

### Database Schema Details

| Table | Purpose | Key Fields |
|-------|---------|------------|
| Users | Stores all user accounts | UserID, Email, Role |
| Events | Event information | EventID, OrganiserID, EventDate |
| Categories | Race distance categories | CategoryID, CategoryName |
| EventCategories | Links events to categories | EventCategoryID, EventID, CategoryID |
| Enrolments | Participant registrations | EnrolmentID, ParticipantID, EventCategoryID |
| Results | Performance results | ResultID, EnrolmentID, FinishTime |

**Key Relationships**:
- One Organiser → Many Events (1:M)
- Many Events ↔ Many Categories (M:M via EventCategories)
- One EventCategory → Many Enrolments (1:M)
- One Enrolment → One Result (1:1)

- ### Sample Data Included

The SQL script seeds the database with realistic test data:

**Organisers**:
1. John Smith - Event organiser
2. Sarah Johnson - Event organiser

**Participants**:
1. Thabo Mokoena - Registered for multiple events
2. Linda Botha - Active participant

**Events**:
1. Cape Town Cycle Tour - 35,000 capacity
2. Soweto Marathon - 30,000 capacity
3. Durban Summer Run - 10,000 capacity

**Categories**: 5km, 10km, 21km, 42km, 100km

This data allows immediate testing of all system functionality without needing to create test data manually.

## Database Setup Instructions

### Prerequisites
- SQL Server (Express/Developer/Enterprise)
- SQL Server Management Studio (SSMS)
- GitHub account (for cloning)

---

## CI/CD Pipeline

The repository includes a GitHub Actions workflow that validates:
- The presence of the `/docs` folder
- ERD file (PNG or PDF)
- API Endpoint Plan file (MD or PDF)
- SQL script file
- Required tables in the SQL script

**Green Build Screenshot:** 
CI-CD-Green-Build.png


---
## Setup Instructions

1. Clone the repository:
```bash
git clone https://github.com/ST10446906/RaceDay-POE.git
cd RaceDay-POE
```

2. Review the documentation in the `/docs` folder.

3. Run the SQL script to set up the database.

4. Follow the API Endpoint Plan for development.

---

## Technologies Used
- Frontend: Web-based interface
- Backend: RESTful API
- Database: SQL
- CI/CD: GitHub Actions

  ## Version History

### Part 1 - System Planning (Current)
- ✅ ERD created with 6+ entities
- ✅ API endpoint plan with 29 endpoints
- ✅ SQL database script with schema and sample data
- ✅ GitHub Actions CI/CD validation workflow
- ✅ Comprehensive documentation in README

### Part 2 - API Development (Upcoming)
- Build RESTful API in C#
- Connect to SQL Server database
- Implement role-based access control
- Write unit tests

### Part 3 - MVC Application (Upcoming)
- Build MVC web application
- Integrate Azure Blob Storage
- Containerize with Docker

### Completed Milestones

| Date | Milestone | Status |
|------|-----------|--------|
| 2026-01-XX | Repository created | ✅ |
| 2026-01-XX | ERD designed | ✅ |
| 2026-01-XX | API endpoints planned | ✅ |
| 2026-01-XX | Database script written | ✅ |
| 2026-01-XX | CI/CD configured | ✅ |
| 2026-01-XX | Documentation complete | ✅ |


## Development Notes

### Important Design Decisions

1. **Single Users Table**: Uses a Role field instead of separate Organiser/Participant tables to simplify authentication.

2. **Unique Constraints**: 
   - Email is unique in Users table
   - Event+Category combination is unique in EventCategories
   - Participant+EventCategory combination is unique in Enrolments

3. **Cascading Deletes**: When an Event or Category is deleted, related EventCategories are automatically removed (CASCADE).

4. **Status Management**:
   - Enrolments: Registered → Cancelled/Completed
   - Results: Pending → Completed/Disqualified

### Future Enhancements
- Payment processing integration
- Live weather and route information
- Participant analytics and insights
- Automated notifications
- Mobile application

### Troubleshooting Common Issues

#### Issue: SQL script fails with "database in use"
**Solution**: Kill existing connections before dropping:
sql

ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE RaceDayDB;



# RaceDay - Part 2: RESTful API

ASP.NET Core 8 Web API (C#) with Entity Framework Core (Code-First) on SQL Server, session-based
authentication, Swagger, and xUnit tests run by GitHub Actions.

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


