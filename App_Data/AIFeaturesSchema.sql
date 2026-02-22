-- ============================================================
-- AIFeaturesSchema.sql
-- Migration: add AI-feature columns to existing tables
-- Run once against SignUpDB.mdf
-- ============================================================

-- --------------------------------------------------------
-- Contacts table: store sentiment analysis result
-- --------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND name = 'Sentiment'
)
BEGIN
    ALTER TABLE [dbo].[Contacts]
        ADD [Sentiment]       NVARCHAR(20)   NULL,   -- Positive | Neutral | Distressed | Urgent
            [SentimentReason] NVARCHAR(500)  NULL;   -- one-line explanation from GPT-4
END;
GO

-- --------------------------------------------------------
-- Appointments table: store no-show risk prediction
-- --------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'NoShowRisk'
)
BEGIN
    ALTER TABLE [dbo].[Appointments]
        ADD [NoShowRisk] NVARCHAR(10) NULL;   -- Low | Medium | High
END;
GO

-- --------------------------------------------------------
-- Appointments table: AI triage columns (already added in
-- AITriageSchema.sql — kept here for reference only)
-- --------------------------------------------------------
-- [AITriage] NVARCHAR(20)  NULL
-- [AINote]   NVARCHAR(MAX) NULL
