-- Insert Users (2 Organisers, 2 Participants)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role) VALUES
('john.organiser@raceday.com', 'hashed_password_123', 'John', 'Smith', 'Organiser'),
('sarah.organiser@raceday.com', 'hashed_password_456', 'Sarah', 'Johnson', 'Organiser'),
('thabo.participant@raceday.com', 'hashed_password_789', 'Thabo', 'Mokoena', 'Participant'),
('linda.participant@raceday.com', 'hashed_password_abc', 'Linda', 'Botha', 'Participant');
GO

-- Insert Categories
INSERT INTO Categories (CategoryName, Description) VALUES
('5km', '5 kilometre run/walk'),
('10km', '10 kilometre run/walk'),
('21km', 'Half Marathon - 21.1 kilometres'),
('42km', 'Full Marathon - 42.2 kilometres'),
('100km', 'Ultra Marathon - 100 kilometres');
GO

-- Insert Events
INSERT INTO Events (OrganiserID, EventName, Description, EventDate, Location, MaxParticipants) VALUES
(1, 'Cape Town Cycle Tour', 'The world''s largest timed cycle event', '2026-03-08 06:00:00', 'Cape Town, Western Cape', 35000),
(2, 'Soweto Marathon', 'Iconic marathon through the streets of Soweto', '2026-11-02 05:30:00', 'Soweto, Gauteng', 30000),
(1, 'Durban Summer Run', 'Beautiful coastal running event', '2026-12-15 06:00:00', 'Durban, KwaZulu-Natal', 10000);
GO

-- Insert EventCategories (Linking Events to Categories)
INSERT INTO EventCategories (EventID, CategoryID) VALUES
-- Cape Town Cycle Tour (Event 1)
(1, 1), -- 5km
(1, 2), -- 10km
(1, 3), -- 21km
(1, 4), -- 42km
-- Soweto Marathon (Event 2)
(2, 3), -- 21km
(2, 4), -- 42km
-- Durban Summer Run (Event 3)
(3, 1), -- 5km
(3, 2), -- 10km
(3, 3); -- 21km
GO

-- Insert Enrolments
INSERT INTO Enrolments (ParticipantID, EventCategoryID, Status) VALUES
-- Thabo's enrolments
(3, 1, 'Registered'), -- Cape Town 5km
(3, 5, 'Registered'), -- Soweto 21km
(3, 7, 'Registered'), -- Durban 5km
-- Linda's enrolments
(4, 2, 'Registered'), -- Cape Town 10km
(4, 6, 'Registered'), -- Soweto 42km
(4, 9, 'Registered'); -- Durban 21km
GO

-- Insert Results (with some pending)
INSERT INTO Results (EnrolmentID, FinishTime, Position, Status) VALUES
-- Thabo's results (some completed, some pending)
(1, '00:25:30', 45, 'Completed'),
(2, NULL, NULL, 'Pending'),
(3, NULL, NULL, 'Pending'),
-- Linda's results
(4, '00:52:15', 78, 'Completed'),
(5, NULL, NULL, 'Pending'),
(6, '01:45:00', 112, 'Completed');
GO

