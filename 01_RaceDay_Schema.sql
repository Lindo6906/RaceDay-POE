

IF DB_ID(N'RaceDayApiDb') IS NULL
    CREATE DATABASE RaceDayApiDb;
GO

USE RaceDayDB;
GO

/* ---------- Roles ---------- */
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
CREATE TABLE dbo.Roles (
    RoleId    INT          NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,   -- 1 = Organiser, 2 = Participant
    RoleName  NVARCHAR(20) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
);
GO

/* ---------- Users ---------- */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    UserId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    RoleId        INT           NOT NULL,
    FullName      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash  NVARCHAR(255) NOT NULL,          -- PBKDF2 hash, never plain text
    PhoneNumber   NVARCHAR(20)  NULL,
    CreatedAt     DATETIME2     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

/* ---------- Events ---------- */
IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
CREATE TABLE dbo.Events (
    EventId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    OrganiserId  INT            NOT NULL,
    Name         NVARCHAR(150)  NOT NULL,
    Description  NVARCHAR(2000) NOT NULL,
    EventDate    DATETIME2      NOT NULL,
    Location     NVARCHAR(200)  NOT NULL,
    DistanceKm   DECIMAL(6,2)   NOT NULL CONSTRAINT CK_Events_Distance CHECK (DistanceKm > 0),
    EventType    NVARCHAR(20)   NOT NULL CONSTRAINT CK_Events_Type CHECK (EventType IN (N'Run', N'Walk', N'Cycle')),
    CreatedAt    DATETIME2      NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(UserId)
);
GO

/* ---------- Categories (belong to one event) ---------- */
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
CREATE TABLE dbo.Categories (
    CategoryId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    EventId       INT          NOT NULL,
    CategoryName  NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId) ON DELETE CASCADE
);
GO

/* ---------- Enrolments (participant + category; event reached via category) ---------- */
IF OBJECT_ID(N'dbo.Enrolments', N'U') IS NULL
CREATE TABLE dbo.Enrolments (
    EnrolmentId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enrolments PRIMARY KEY,
    ParticipantId   INT          NOT NULL,
    CategoryId      INT          NOT NULL,
    EnrolmentDate   DATETIME2    NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT SYSUTCDATETIME(),
    Status          NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT N'Registered'
                    CONSTRAINT CK_Enrolments_Status CHECK (Status IN (N'Registered', N'Cancelled', N'Completed')),
    CONSTRAINT FK_Enrolments_Users      FOREIGN KEY (ParticipantId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId)    REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantId, CategoryId)
);
GO

/* ---------- Results (one per enrolment) ---------- */
IF OBJECT_ID(N'dbo.Results', N'U') IS NULL
CREATE TABLE dbo.Results (
    ResultId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Results PRIMARY KEY,
    EnrolmentId  INT          NOT NULL CONSTRAINT UQ_Results_Enrolment UNIQUE,
    FinishTime   TIME         NOT NULL,
    Position     INT          NOT NULL CONSTRAINT CK_Results_Position CHECK (Position >= 1),
    Status       NVARCHAR(20) NOT NULL CONSTRAINT DF_Results_Status DEFAULT N'Completed'
                 CONSTRAINT CK_Results_Status CHECK (Status IN (N'Pending', N'Completed', N'Disqualified')),
    RecordedAt   DATETIME2    NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES dbo.Enrolments(EnrolmentId) ON DELETE CASCADE
);
GO

/* ---------- Seed the two roles (IDs must stay 1 and 2) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = 1) INSERT INTO dbo.Roles (RoleId, RoleName) VALUES (1, N'Organiser');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleId = 2) INSERT INTO dbo.Roles (RoleId, RoleName) VALUES (2, N'Participant');
GO

