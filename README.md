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

**Green Build Screenshot:** `CI-CD-Green-Build.png`

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


