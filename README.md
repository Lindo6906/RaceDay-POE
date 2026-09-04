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

### Entity Relationship Diagram (ERD)
The ERD includes 6 entities:
- **Users** - Stores Organisers and Participants
- **Events** - Event information created by organisers
- **Categories** - Event categories (5km, 10km, 21km, 42km)
- **EventCategories** - Junction table linking Events and Categories
- **Enrolments** - Participant registrations for event categories
- **Results** - Participant results for events

**File:** `/docs/ERD.png`

---

### API Endpoint Plan
The API endpoint plan covers:
- Authentication (register and login)
- User Profile management
- Event CRUD operations
- Category CRUD operations
- Event enrolments
- Results management

**File:** `/docs/API-Endpoint-Plan.md`

---

### Database Schema
The SQL script creates the full database schema with:
- All CREATE TABLE statements
- Primary and foreign keys
- Constraints (NOT NULL, UNIQUE, DEFAULT)
- Sample data
- Performance indexes

**File:** `/docs/RaceDay_Database.sql`

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


