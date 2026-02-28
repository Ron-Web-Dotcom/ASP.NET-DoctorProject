# Portmore Medical Center — ASP.NET Web Application

A full-featured medical clinic web application built with **ASP.NET Web Forms (.NET 4.5)** and **SQL Server LocalDB**, enhanced with **20 GPT-4–powered AI features** across patient-facing and admin workflows.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Tech Stack](#tech-stack)
3. [Getting Started](#getting-started)
4. [Database Setup](#database-setup)
5. [OpenAI Configuration](#openai-configuration)
6. [Project Structure](#project-structure)
7. [AI Features — Patient-Facing](#ai-features--patient-facing)
8. [AI Features — Admin-Facing](#ai-features--admin-facing)
9. [All Pages Reference](#all-pages-reference)
10. [OpenAI Service Methods](#openai-service-methods)
11. [Database Schema](#database-schema)

---

## Project Overview

Portmore Medical Center is a clinic management website that allows patients to:
- Book, cancel, and reschedule appointments
- Complete pre-appointment health questionnaires
- Check medication interactions
- Log and analyse daily symptoms
- Chat with an AI assistant and use a symptom checker

Admin staff can:
- View all appointments, users, and contact enquiries
- See AI-generated triage levels, no-show risk ratings, and sentiment badges
- Generate follow-up emails, referral letters, complaint responses, and newsletters
- Simplify clinical notes and forecast weekly demand
- Create professional staff biographies

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Web Forms, .NET 4.5 |
| Language | C# |
| Database | SQL Server LocalDB (`.mdf`) |
| UI | Bootstrap 3.3.7, jQuery 1.9.1, Bootstrap Glyphicons |
| AI | OpenAI GPT-4 via REST API (`OpenAIService.cs`) |
| HTTP Handlers | `.ashx` (chat, symptoms, translation) |

---

## Getting Started

### Prerequisites
- Visual Studio 2015 or later (or Visual Studio Code with C# extension)
- .NET Framework 4.5
- SQL Server LocalDB (ships with Visual Studio)
- An OpenAI API key with GPT-4 access

### Run Locally
1. Clone or download the repository.
2. Open `ASP.NET-DoctorProject.sln` in Visual Studio.
3. Add your OpenAI API key to `Web.config` (see [OpenAI Configuration](#openai-configuration)).
4. Run the database migration scripts (see [Database Setup](#database-setup)).
5. Press **F5** to build and run. The site opens on `localhost`.

---

## Database Setup

The application uses **SQL Server LocalDB** with the database file at `App_Data/SignUpDB.mdf`.

Run the following migration scripts **once** against `SignUpDB.mdf` using SQL Server Management Studio or the Visual Studio SQL editor:

| Script | Purpose |
|--------|---------|
| `App_Data/AITriageSchema.sql` | Adds `AITriage` and `AINote` columns to `Appointments` |
| `App_Data/AIFeaturesSchema.sql` | Adds `NoShowRisk` to `Appointments`; adds `Sentiment` and `SentimentReason` to `Contacts` |

### Expected Tables

| Table | Key Columns |
|-------|-------------|
| `Appointments` | FirstName, Lastname, Age, Services, PhoneNum, Time, Email, Address1, Address2, City, Issue, **AITriage**, **AINote**, **NoShowRisk** |
| `Contacts` | MemberID, FirstName, LastName, Email, PhoneNum, Message, **Sentiment**, **SentimentReason** |
| `Users` | MemberID, FirstName, LastName, Email, Password, Month, Day, Year |
| `Admins` | Id, FirstName, LastName, TypeOFUser, Email, Password, Confirm |

---

## OpenAI Configuration

Open `Web.config` and set your API key in the `<appSettings>` section:

```xml
<appSettings>
  <add key="OpenAIApiKey" value="sk-YOUR-KEY-HERE" />
</appSettings>
```

All AI features degrade gracefully — if the key is absent or the API call fails, every method returns a sensible fallback string so the application continues to function normally.

---

## Project Structure

```
ASP.NET-DoctorProject/
│
├── App_Code/
│   ├── OpenAIService.cs        # All 24 GPT-4 methods (single static class)
│   └── connectionManager.cs    # SQL connection helper
│
├── App_Data/
│   ├── SignUpDB.mdf             # SQL Server LocalDB database
│   ├── AITriageSchema.sql       # Migration: triage columns
│   └── AIFeaturesSchema.sql     # Migration: sentiment + no-show risk columns
│
├── Content/                    # Bootstrap CSS
├── Scripts/                    # jQuery + Bootstrap JS
├── Styles/                     # Custom CSS
├── fonts/                      # Bootstrap Glyphicon fonts
├── images/                     # Doctor photos + clinic imagery
│
├── MasterPage.master           # Site-wide layout, navbar, language toggle
│
├── — Patient Pages —
├── AppointmentForm.aspx/.cs    # Booking form (triage, duplicate check, wellness, checklist, education)
├── Questionnaire.aspx/.cs      # Pre-appointment health questionnaire
├── MedicationChecker.aspx/.cs  # Medication interaction checker
├── SymptomDiary.aspx/.cs       # Multi-day symptom logger + AI trend analysis
├── CancelAppointment.aspx/.cs  # Cancel/reschedule with AI rescheduling message
├── SymptomChecker.aspx/.cs     # AI symptom-to-specialist recommender
├── AIChatAssistant.aspx        # Real-time AI chat assistant
├── ContactForm.aspx/.cs        # Contact form with sentiment analysis
├── SignUp.aspx/.cs             # Patient registration
├── SignIn2.aspx/.cs            # Patient login
│
├── — Admin Pages —
├── AdminView.aspx/.cs          # Full admin dashboard (all admin AI panels)
├── admin.aspx/.cs              # Admin login
├── AdminSignUpaspx.aspx/.cs    # Admin registration
├── StaffBioGenerator.aspx/.cs  # AI staff biography generator
│
├── — HTTP Handlers —
├── ChatHandler.ashx            # WebSocket/AJAX handler for AI chat
├── SymptomHandler.ashx         # AJAX handler for symptom checker
├── TranslateHandler.ashx       # AJAX handler for multi-language translation
│
├── — Specialty & Info Pages —
├── Cardiology.aspx, Gynaecology.aspx, Opticology.aspx ...
├── StaffMembers.aspx, Directors.aspx, Facilites.aspx ...
├── HomePage.aspx, Vision Mission.aspx ...
│
└── Web.config                  # Connection string + OpenAI API key
```

---

## AI Features — Patient-Facing

### 1. Pre-Appointment Questionnaire Analyser
**Page:** `Questionnaire.aspx`
**Method:** `OpenAIService.GetQuestionnaireAnalysis()`

Patient answers 5 questions before booking (service, symptom duration, severity 1–10, medications, allergies, extra info). GPT-4 produces a clinical pre-screening summary stored in `Session["QuestionnaireSummary"]`, which auto-populates the Reason field on `AppointmentForm.aspx`.

---

### 2. Wellness Tips Generator
**Page:** `AppointmentForm.aspx` (confirmation panel)
**Method:** `OpenAIService.GetWellnessTips()`

After a patient books an appointment, GPT-4 generates 3–5 personalised bullet-point wellness tips based on the service selected and the patient's stated reason. Displayed in a green alert on the booking confirmation screen.

---

### 3. Appointment Rescheduler
**Page:** `CancelAppointment.aspx`
**Method:** `OpenAIService.GetRescheduleMessage()`

When a patient needs to cancel, they enter their name, service, and optional reason. GPT-4 drafts an empathetic, encouraging message that acknowledges the cancellation and invites them to rebook promptly. No appointment records are deleted — this is a self-service guidance page.

---

### 4. Multi-Language Support
**Handler:** `TranslateHandler.ashx`
**Method:** `OpenAIService.TranslateText()`
**Location:** Navbar Language dropdown (all pages via `MasterPage.master`)

A language dropdown in the navbar lets users switch between **English, Spanish, French, Mandarin, Arabic,** and **Portuguese**. Selecting a language POSTs the visible page content to `TranslateHandler.ashx`, which calls GPT-4 and replaces the content area with the translation. Selecting "English" reloads the original page.

---

### 5. Medication Interaction Checker
**Page:** `MedicationChecker.aspx`
**Method:** `OpenAIService.CheckMedicationInteractions()`

Patients list their current medications (one per line or comma-separated). GPT-4 reviews the list for known interactions and monitoring requirements, and returns a plain-English report. Every response begins with a mandatory disclaimer: *"This is general educational information only and does not replace advice from your doctor or pharmacist."*

---

### 6. Symptom Diary
**Page:** `SymptomDiary.aspx`
**Method:** `OpenAIService.AnalyseSymptomDiary()`

Patients log daily symptom entries (date + description) stored in browser session. When they click **Analyse**, GPT-4 reviews all entries and returns:
- A **trend summary** (2–4 sentences on symptom progression)
- A **recommendation** (monitor / see GP soon / seek urgent care)

Diary entries persist across postbacks within a session. Patients can clear the diary or bring the analysis to their appointment.

---

### 7. Smart Time Slot Recommender
**Page:** `AppointmentForm.aspx` (inline button)
**Method:** `OpenAIService.GetTimeSlotRecommendation()`

A "Suggest Best Time for Me" button sits next to the service dropdown. GPT-4 recommends the most suitable time slot based on the service type and any reason the patient has already typed, with a brief explanation. The patient still selects their preferred time from the dropdown.

---

### 8. AI Readiness Checklist
**Page:** `AppointmentForm.aspx` (confirmation panel)
**Method:** `OpenAIService.GetReadinessChecklist()`

After booking, GPT-4 generates a tailored 4–6 item checklist of what the patient should bring and do before their visit (e.g. fast for X hours, bring ID, wear loose clothing), specific to the booked specialty. Displayed in a yellow alert on the confirmation screen.

---

### 9. Patient Education Card
**Page:** `AppointmentForm.aspx` (confirmation panel)
**Method:** `OpenAIService.GetPatientEducationCard()`

After booking, GPT-4 writes a short plain-English overview of the patient's booked specialty — what the department focuses on, what typically happens during the appointment, and one tip for getting the most from the visit. Displayed in a blue alert on the confirmation screen.

---

### 10. Duplicate Booking Detector
**Page:** `AppointmentForm.aspx`
**Method:** `IsDuplicateBooking()` (database query, no AI call)

Before inserting a new appointment, the system queries the `Appointments` table for an existing record with the same first name, last name, and service. If a match is found, a yellow warning banner is shown. The booking is not blocked — the patient can still submit if intentional.

### 21. Patient Appointment Dashboard
**Page:** `MyAppointments.aspx`
**Method:** `OpenAIService.GetAppointmentPreparationTip()`

After signing in, patients land on their personal dashboard instead of being redirected to the contact form. The dashboard:

- Queries the `Appointments` table by email and lists **all the patient's bookings** in a styled table.
- Each row shows the **service**, **time slot**, **AI triage badge** (colour-coded: Urgent / High / Medium / Low), the **AI pre-assessment note** written by GPT-4 at booking time, and the **no-show risk badge**.
- A **Get AI Tip** button on each row calls GPT-4 and returns 2–3 sentences of personalised preparation advice specific to that specialty and time of day — shown in a blue callout below the table.
- If the patient has no bookings, an empty-state panel with a "Book Your First Appointment" button is shown instead.
- The page is **session-guarded**: unauthenticated visitors are redirected to the Sign In page.
- Quick-link cards at the bottom lead to Cancel / Reschedule, AI Chat Assistant, and the Contact Form.

**Session change:** `SignIn2.aspx.cs` now also stores `Session["Email"]` on login, and redirects to `MyAppointments.aspx` instead of `ContactForm.aspx`.

---

## AI Features — Admin-Facing

All admin AI features are accessible from **`AdminView.aspx`** after logging in.

### 11. AI Triage — Urgency Classification
**Triggered:** Automatically on every appointment submission
**Method:** `OpenAIService.GetTriage()`
**Stored in:** `Appointments.AITriage` + `Appointments.AINote`

GPT-4 assigns a triage priority (**Urgent / High / Medium / Low**) and a 2–3 sentence clinical pre-assessment note for the treating doctor, based on the patient's name, age, service, and stated reason. Displayed with colour-coded badges in the AdminView Appointments grid.

| Level | Colour |
|-------|--------|
| Urgent | Red |
| High | Amber |
| Medium | Blue |
| Low | Green |

---

### 12. Triage Escalation Alert
**Location:** Top of `AdminView.aspx`
**Method:** DB query on `Page_Load`

On every admin dashboard load, the system counts appointments where `AITriage = 'Urgent'`. If **3 or more** are found, a prominent red alert banner is displayed at the top of the page so staff are immediately aware.

---

### 13. Sentiment Analysis on Contact Messages
**Triggered:** Automatically on every contact form submission
**Method:** `OpenAIService.GetSentiment()`
**Stored in:** `Contacts.Sentiment` + `Contacts.SentimentReason`

GPT-4 classifies the emotional tone of each patient message as **Positive / Neutral / Distressed / Urgent**. Displayed with colour-coded badges in the AdminView Contacts grid so staff can prioritise distressed or urgent enquiries without reading every message.

| Level | Colour |
|-------|--------|
| Urgent | Red |
| Distressed | Orange |
| Neutral | Default |
| Positive | Green |

---

### 14. No-Show Risk Predictor
**Triggered:** Automatically on every appointment submission
**Method:** `OpenAIService.GetNoShowRisk()`
**Stored in:** `Appointments.NoShowRisk`

GPT-4 rates each appointment's no-show likelihood as **Low / Medium / High**, based on the time slot and service type. Admin staff can proactively send reminders to high-risk patients. Colour-coded in the AdminView Appointments grid.

---

### 15. AI Follow-Up Email Drafter
**Location:** `AdminView.aspx` — Follow-Up Email Drafter panel
**Method:** `OpenAIService.GetFollowUpEmail()`

Admin enters a patient's first name, service, and optional AI note. GPT-4 drafts a warm, professional post-appointment follow-up email thanking the patient and inviting them to rebook if needed. Ready to copy and send.

---

### 16. Doctor's Note Simplifier
**Location:** `AdminView.aspx` — Doctor's Note Simplifier panel
**Method:** `OpenAIService.SimplifyNote()`

Admin pastes any clinical note or doctor's dictation. GPT-4 rewrites it in plain, patient-friendly English — same meaning, no jargon. Useful for generating patient-facing summaries from clinical records.

---

### 17. AI Dashboard Insights
**Location:** `AdminView.aspx` — AI Dashboard Insights panel
**Method:** `OpenAIService.GetAdminInsights()`

The Refresh button queries the database for appointment counts by service and by triage level, then asks GPT-4 to produce 3–5 sentences of operational insight to help the admin team understand patient demand and prioritise resources.

---

### 18. Weekly Demand Forecast
**Location:** `AdminView.aspx` — Weekly Demand Forecast panel
**Method:** `OpenAIService.GetWeeklyDemandForecast()`

The Generate Forecast button pulls the current service booking distribution from the database and asks GPT-4 to predict which services will be busiest next week, flag any trends, and suggest staffing focus areas.

---

### 19. AI Referral Letter Drafter
**Location:** `AdminView.aspx` — AI Referral Letter Drafter panel
**Method:** `OpenAIService.GetReferralLetter()`

Admin fills in patient name, referring doctor, source department, destination department, and reason. GPT-4 generates a formally structured referral letter dated today, signed by the referring doctor — ready to print.

---

### 20. Complaint Escalation Handler
**Location:** `AdminView.aspx` — Complaint Escalation Handler panel
**Method:** `OpenAIService.GetComplaintResponse()`

Admin pastes a distressed patient message. GPT-4 drafts a formal, empathetic complaint response that acknowledges the concern, apologises for any distress, explains it will be reviewed, and provides a contact placeholder — signed by the Patient Experience Team.

---

### 21. Monthly Health Newsletter Generator
**Location:** `AdminView.aspx` — Monthly Newsletter Generator panel
**Method:** `OpenAIService.GetMonthlyNewsletter()`

The Generate Newsletter button reads the top 5 most-booked services from the database and asks GPT-4 to write a patient-facing monthly newsletter (health tips, service highlights, rebook reminder). Output is plain text, ready to paste into an email campaign.

---

### 22. Staff Bio Generator
**Page:** `StaffBioGenerator.aspx`
**Method:** `OpenAIService.GetStaffBio()`

Admin enters a doctor's name, specialty, qualifications, years of experience, and any additional details (research, languages, awards). GPT-4 writes a polished 2–3 paragraph biography in third person, suitable for the staff profile pages.

---

## All Pages Reference

| Page | Role | Description |
|------|------|-------------|
| `HomePage.aspx` | Public | Landing page with services overview |
| `AppointmentForm.aspx` | Public | Book an appointment (AI triage, duplicate check, wellness tips, checklist, education card, time recommender) |
| `Questionnaire.aspx` | Public | Pre-appointment health questionnaire |
| `MedicationChecker.aspx` | Public | Medication interaction checker |
| `SymptomDiary.aspx` | Public | Daily symptom logger with AI trend analysis |
| `CancelAppointment.aspx` | Public | Cancel/reschedule with AI rescheduling message |
| `SymptomChecker.aspx` | Public | AI symptom-to-specialist recommender |
| `AIChatAssistant.aspx` | Public | Real-time GPT-4 chat assistant |
| `ContactForm.aspx` | Public | Contact form with AI sentiment analysis |
| `MyAppointments.aspx` | Patient | Personal dashboard — all bookings, AI triage badges, per-appointment prep tips |
| `SignUp.aspx` | Public | Patient registration |
| `SignIn2.aspx` | Public | Patient login |
| `AdminView.aspx` | Admin | Full dashboard: all grids + all admin AI panels |
| `admin.aspx` | Admin | Admin login |
| `AdminSignUpaspx.aspx` | Admin | Admin registration |
| `StaffBioGenerator.aspx` | Admin | AI staff biography generator |
| `Cardiology.aspx` | Public | Cardiology department info |
| `Gynaecology.aspx` | Public | Gynaecology department info |
| `Opticology.aspx` | Public | Opticology department info |
| `Paediatrician.aspx` | Public | Paediatrics department info |
| `Radiology.aspx` | Public | Radiology department info |
| `Surgeon.aspx` | Public | Surgery department info |
| `GeneralPractitionersaspx.aspx` | Public | GP department info |
| `StaffMembers.aspx` | Public | Staff directory |
| `Directors.aspx` | Public | Directors page |
| `Facilites.aspx` | Public | Clinic facilities |
| `Vision Mission.aspx` | Public | Vision and mission statement |
| `GoogleMap.aspx` | Public | Clinic location map |

---

## OpenAI Service Methods

All AI calls are centralised in `App_Code/OpenAIService.cs`. Every public method falls back gracefully if the API key is missing or the call fails.

| Method | Feature | Returns |
|--------|---------|---------|
| `GetTriage()` | Auto-triage on booking | `TriageResult { Triage, Note }` |
| `GetSpecialistRecommendation()` | Symptom checker | `SpecialistResult { Specialist, Reason }` |
| `GetAdminInsights()` | Dashboard insights | `string` |
| `GetContactReply()` | Contact auto-reply draft | `string` |
| `GetHealthTip()` | Daily health tip | `string` |
| `GetChatResponse()` | AI chat assistant | `string` |
| `GetWellnessTips()` | Post-booking wellness tips | `string` |
| `GetNoShowRisk()` | No-show risk prediction | `"Low"` \| `"Medium"` \| `"High"` |
| `GetSentiment()` | Contact message sentiment | `SentimentResult { Level, Reason }` |
| `GetFollowUpEmail()` | Follow-up email draft | `string` |
| `SimplifyNote()` | Doctor's note simplifier | `string` |
| `GetQuestionnaireAnalysis()` | Questionnaire pre-screening | `string` |
| `GetRescheduleMessage()` | Rescheduling message | `string` |
| `TranslateText()` | Page translation | `string` |
| `GetTimeSlotRecommendation()` | Smart time slot suggestion | `string` |
| `CheckMedicationInteractions()` | Medication interaction check | `string` |
| `AnalyseSymptomDiary()` | Symptom diary trend analysis | `SymptomDiaryResult { Summary, Recommendation }` |
| `GetReadinessChecklist()` | Pre-visit readiness checklist | `string` |
| `GetPatientEducationCard()` | Specialty education overview | `string` |
| `GetWeeklyDemandForecast()` | Staffing demand forecast | `string` |
| `GetReferralLetter()` | Formal referral letter | `string` |
| `GetComplaintResponse()` | Complaint escalation response | `string` |
| `GetStaffBio()` | Staff biography generator | `string` |
| `GetMonthlyNewsletter()` | Monthly patient newsletter | `string` |
| `GetAppointmentPreparationTip()` | Per-appointment prep tip on patient dashboard | `string` |

---

## Database Schema

### Appointments

```sql
CREATE TABLE [Appointments] (
    Id        INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(100),
    Lastname  NVARCHAR(100),
    Age       NVARCHAR(10),
    Services  NVARCHAR(100),
    PhoneNum  NVARCHAR(20),
    Time      NVARCHAR(50),
    Email     NVARCHAR(200),
    Address1  NVARCHAR(200),
    Address2  NVARCHAR(200),
    City      NVARCHAR(100),
    Issue     NVARCHAR(MAX),
    -- AI columns (added via migration scripts)
    AITriage   NVARCHAR(20),
    AINote     NVARCHAR(MAX),
    NoShowRisk NVARCHAR(10)
);
```

### Contacts

```sql
CREATE TABLE [Contacts] (
    MemberID  INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName  NVARCHAR(100),
    Email     NVARCHAR(200),
    PhoneNum  NVARCHAR(20),
    Message   NVARCHAR(MAX),
    -- AI columns (added via migration scripts)
    Sentiment       NVARCHAR(20),
    SentimentReason NVARCHAR(500)
);
```

---

*Portmore Medical Center — ASP.NET Doctor Project*
*Built with ASP.NET Web Forms, SQL Server LocalDB, Bootstrap 3, and OpenAI GPT-4*
