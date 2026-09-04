-- Create database
CREATE DATABASE RaceDayDB;
GO

-- ============================================
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    DateCreated DATETIME DEFAULT GETDATE()
);
GO

-- ============================================
-- 2. EVENTS TABLE
-- Stores event information created by organisers
-- ============================================
CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(255) NOT NULL,
    Description TEXT,
    EventDate DATETIME NOT NULL,
    Location VARCHAR(255) NOT NULL,
    MaxParticipants INT NOT NULL,
    IsActive BIT DEFAULT 1,
    DateCreated DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO

-- ============================================
-- 3. CATEGORIES TABLE
-- Event categories (5km, 10km, etc.)
-- ============================================
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255)
);
GO

-- ============================================
-- 4. EVENTCATEGORIES TABLE
-- Junction table linking Events and Categories
-- ============================================
CREATE TABLE EventCategories (
    EventCategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    CONSTRAINT FK_EventCategories_Event FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE,
    CONSTRAINT FK_EventCategories_Category FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE,
    CONSTRAINT UQ_EventCategory UNIQUE (EventID, CategoryID)
);
GO

-- ============================================
-- 5. ENROLMENTS TABLE
-- Participant registrations for event categories
-- ============================================
CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventCategoryID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) DEFAULT 'Registered' CHECK (Status IN ('Registered', 'Cancelled', 'Completed')),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_EventCategory FOREIGN KEY (EventCategoryID) REFERENCES EventCategories(EventCategoryID),
    CONSTRAINT UQ_Participant_EventCategory UNIQUE (ParticipantID, EventCategoryID)
);
GO

-- ============================================
-- 6. RESULTS TABLE
-- Participant results for events
-- ============================================
CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NULL,
    Position INT NULL,
    Status VARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Completed', 'Disqualified')),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID),
    CONSTRAINT UQ_Result_Enrolment UNIQUE (EnrolmentID)
);
GO