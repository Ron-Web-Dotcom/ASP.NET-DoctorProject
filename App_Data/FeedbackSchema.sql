-- ============================================================
-- FeedbackSchema.sql
-- Run this script ONCE against SignUpDB.mdf to create the
-- Feedback table used by AppointmentFeedback.aspx.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'Feedback'
)
BEGIN
    CREATE TABLE [Feedback] (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        PatientName     NVARCHAR(100)  NOT NULL,
        Email           NVARCHAR(200)  NOT NULL,
        Service         NVARCHAR(100)  NOT NULL,
        Rating          INT            NOT NULL CHECK (Rating BETWEEN 1 AND 5),
        Comment         NVARCHAR(MAX)  NOT NULL,
        Sentiment       NVARCHAR(20)   NULL,
        SentimentReason NVARCHAR(500)  NULL,
        SubmittedAt     DATETIME       NOT NULL DEFAULT GETDATE()
    );
END
