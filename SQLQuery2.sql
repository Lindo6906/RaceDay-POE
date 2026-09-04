CREATE INDEX IX_Events_OrganiserID ON Events(OrganiserID);
CREATE INDEX IX_Events_EventDate ON Events(EventDate);
CREATE INDEX IX_Enrolments_ParticipantID ON Enrolments(ParticipantID);
CREATE INDEX IX_Enrolments_EventCategoryID ON Enrolments(EventCategoryID);
CREATE INDEX IX_Enrolments_Status ON Enrolments(Status);
CREATE INDEX IX_Results_EnrolmentID ON Results(EnrolmentID);
GO