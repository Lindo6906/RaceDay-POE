-- View all users
SELECT * FROM Users;
GO

-- View all events with organiser names
SELECT e.EventID, e.EventName, e.EventDate, e.Location, 
       u.FirstName + ' ' + u.LastName AS Organiser
FROM Events e
JOIN Users u ON e.OrganiserID = u.UserID;
GO

-- View all enrolments with participant and event details
SELECT en.EnrolmentID, 
       u.FirstName + ' ' + u.LastName AS Participant,
       ev.EventName,
       c.CategoryName,
       en.Status,
       en.EnrolmentDate
FROM Enrolments en
JOIN Users u ON en.ParticipantID = u.UserID
JOIN EventCategories ec ON en.EventCategoryID = ec.EventCategoryID
JOIN Events ev ON ec.EventID = ev.EventID
JOIN Categories c ON ec.CategoryID = c.CategoryID;
GO

-- View results with participant and event details
SELECT r.ResultID,
       u.FirstName + ' ' + u.LastName AS Participant,
       ev.EventName,
       c.CategoryName,
       r.FinishTime,
       r.Position,
       r.Status
FROM Results r
JOIN Enrolments en ON r.EnrolmentID = en.EnrolmentID
JOIN Users u ON en.ParticipantID = u.UserID
JOIN EventCategories ec ON en.EventCategoryID = ec.EventCategoryID
JOIN Events ev ON ec.EventID = ev.EventID
JOIN Categories c ON ec.CategoryID = c.CategoryID;
GO