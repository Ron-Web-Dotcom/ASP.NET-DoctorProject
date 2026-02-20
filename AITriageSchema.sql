-- =============================================================
-- AI Triage Schema Migration
-- Run this script ONCE against SignUpDB.mdf to add the two
-- columns required for AI-generated patient triage data.
--
-- How to run:
--   Option A – SQL Server Management Studio (SSMS):
--     1. Connect to (LocalDB)\MSSQLLocalDB
--     2. Open SignUpDB
--     3. Open a New Query window and paste + execute this script.
--
--   Option B – sqlcmd (command line):
--     sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d SignUpDB -i AITriageSchema.sql
-- =============================================================

-- Add AI triage priority column (Urgent / High / Medium / Low / Pending / Error)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Appointments' AND COLUMN_NAME = 'AITriage'
)
BEGIN
    ALTER TABLE [Appointments] ADD [AITriage] NVARCHAR(20) NULL;
    PRINT 'Added column: Appointments.AITriage';
END
ELSE
    PRINT 'Column already exists: Appointments.AITriage';

-- Add AI pre-assessment note column
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Appointments' AND COLUMN_NAME = 'AINote'
)
BEGIN
    ALTER TABLE [Appointments] ADD [AINote] NVARCHAR(MAX) NULL;
    PRINT 'Added column: Appointments.AINote';
END
ELSE
    PRINT 'Column already exists: Appointments.AINote';

GO
