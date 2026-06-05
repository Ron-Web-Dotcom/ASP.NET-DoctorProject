# Portmore Medical Center — ASP.NET Web Application

A full-featured medical clinic web application built with **ASP.NET Web Forms (.NET 4.5)** and **SQL Server LocalDB**, enhanced with **50 GPT-4–powered AI features** across patient-facing and admin workflows.

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
12. [Feature Showcase — Sample Inputs & Outputs](#feature-showcase--sample-inputs--outputs)

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
| `App_Data/FeedbackSchema.sql` | Creates the `Feedback` table for post-appointment ratings |

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

### 22. AI Diet & Lifestyle Planner
**Page:** `LifestylePlanner.aspx`
**Method:** `OpenAIService.GetLifestylePlan()`

Patient selects their upcoming specialty and optionally provides their age and other conditions. GPT-4 generates a personalised plan with three clearly labelled sections: **Diet Recommendations**, **Exercise & Activity**, and **Lifestyle Tips** — all tailored to the selected specialty. Ends with a reminder to follow the doctor's advice. Accessible from the AI Tools navbar dropdown.

---

### 23. AI Insurance & Cost Estimator
**Page:** `InsuranceEstimator.aspx`
**Method:** `OpenAIService.GetInsuranceGuide()`

Patient picks a service and their insurance type. GPT-4 returns a structured guide covering: what is typically covered for that specialty, five practical questions to ask the insurer, and general cost factors. Always closes with a mandatory disclaimer to contact the insurance provider and the clinic's billing team. Accessible from the AI Tools navbar dropdown.

---

### 24. Doctor Availability Viewer
**Page:** `DoctorAvailability.aspx`
**Method:** `OpenAIService.GetDoctorFocusSummary()`

Patient selects a specialty from a dropdown. The page looks up the relevant doctors from a static roster (7 specialties, 10 named doctors with typical time slots) and calls GPT-4 once per doctor to generate a short, patient-friendly 2-sentence summary of that doctor's focus areas. Results are shown as individual doctor cards with available time slot badges and a "Book with this Doctor" button. Accessible from the AI Tools navbar dropdown.

**Doctor roster:**

| Specialty | Doctors |
|-----------|---------|
| Cardiology | Dr. Michael Knapton, Dr. Patrick H Maxwell |
| General Practitioner | Dr. Mike More, Dr. Shirley Pointer |
| Gynaecology | Dr. Ann-Marie Ingle, Dr. Evelyn Barker |
| Opticology | Dr. Roland Siker |
| Paediatrics | Dr. Sharon Peacock |
| Radiology | Dr. Kate Lancaster |
| Surgery | Dr. Jag Ahluwalia |

---

### 25. Post-Appointment Feedback
**Page:** `AppointmentFeedback.aspx`
**Method:** `OpenAIService.GetSentiment()` (reuses existing method)
**DB migration:** `App_Data/FeedbackSchema.sql`

Patients submit a star rating (1–5) and a free-text comment after their visit. GPT-4 analyses the comment using the existing sentiment classifier (Positive / Neutral / Distressed / Urgent) and the sentiment badge is shown instantly on the thank-you screen. All submissions — name, email, service, rating, comment, sentiment — are persisted to the new `Feedback` table. Accessible from the AI Tools navbar dropdown.

**Admin view** (`AdminView.aspx`):
- New `GridViewFeedback` shows all submissions with colour-coded rating (green/amber/red) and sentiment badges.
- **Generate Summary** button calls `OpenAIService.GetFeedbackSummary()` — GPT-4 analyses total submissions, average rating, sentiment breakdown, and last 10 comments to produce an executive-style summary for management.

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

### 30. Emergency Symptom Triage
**Page:** `EmergencyTriage.aspx`
**Method:** `OpenAIService.GetEmergencyTriage()`

Patient describes their symptoms in a free-text box. GPT-4 classifies the situation into exactly one of four urgency levels — **EMERGENCY / URGENT / APPOINTMENT / SELF-CARE** — with a colour-coded banner, a reason, and clear next-step instructions. Uses temperature 0.1 for maximum consistency. Always reminds the patient to call 999 if in doubt. Accessible from the AI Tools navbar.

---

### 31. Medical History Summariser
**Page:** `MedicalHistory.aspx`
**Method:** `OpenAIService.GetMedicalHistorySummary()`

Patient fills in structured fields: current conditions, medications, allergies, past surgeries, and family history. GPT-4 generates a concise, professional clinical summary suitable for sharing with any healthcare provider. The result page includes a **Print** button so patients can bring a physical copy to appointments.

---

### 32. Mental Health Check-In
**Page:** `MentalHealthCheckIn.aspx`
**Method:** `OpenAIService.GetMentalHealthCheckIn()`

Five dropdown ratings (1–5) covering mood, sleep quality, anxiety, energy, and social connection, plus an optional free-text notes field. GPT-4 returns an empathetic wellbeing summary, 3–4 evidence-based suggestions, and guidance on when to seek professional help. Includes crisis line signposting in the disclaimer.

---

### 33. Health Goal Planner
**Page:** `HealthGoalPlanner.aspx`
**Method:** `OpenAIService.GetHealthGoalPlan()`

Patient enters a health goal (e.g. "lose 10kg", "lower blood pressure"), their current activity level, and any existing conditions. GPT-4 produces a structured **4-week action plan** with Week 1–4 steps and a "Tips for Success" section. Accessible from the AI Tools navbar.

---

### 34. Second Opinion Question Generator
**Page:** `SecondOpinionPrompts.aspx`
**Method:** `OpenAIService.GetSecondOpinionQuestions()`

Patient pastes or types a diagnosis or treatment plan. GPT-4 generates 8–10 intelligent questions grouped into: *Understanding the Diagnosis*, *Treatment Options*, and *Next Steps & Lifestyle*. The result page includes a **Print** button so patients can bring the list to their next appointment.

---

### 35. Post-Treatment Recovery Tracker
**Page:** `RecoveryTracker.aspx`
**Method:** `OpenAIService.AnalyseRecovery()`

Multi-entry recovery log (similar in design to the Symptom Diary). Patient adds daily entries with a date and free-text recovery note. Once 2+ entries exist, an **Analyse My Recovery** button calls GPT-4, which returns a recovery trend, positive signs, areas to watch, and a colour-coded recommendation badge: *On Track / Contact Care Team / Contact Doctor Promptly*. Entries are session-based.

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

### 36. Staff Performance Report
**Location:** `AdminView.aspx` — Staff Performance Report panel
**Method:** `OpenAIService.GetStaffPerformanceReport()`

Combines appointment volume data, triage distribution, no-show rates, and patient feedback into a single GPT-4 management report covering: busiest departments, patient satisfaction trends, operational concerns, and 3 specific actionable recommendations for the management team.

---

### 37. Clinical Audit Report
**Location:** `AdminView.aspx` — Clinical Audit Report panel
**Method:** `OpenAIService.GetClinicalAuditReport()`

Pulls appointment data, triage breakdown, no-show rates, feedback sentiment, and contact sentiment into a formal clinical governance document with: Executive Summary, Key Findings, Areas of Concern, Recommendations, and Conclusion. Suitable for clinical governance meetings.

---

### 38. Staff Training Topic Recommender
**Location:** `AdminView.aspx` — Staff Training Topic Recommender panel
**Method:** `OpenAIService.GetTrainingRecommendations()`

Analyses complaint themes from the Contacts table and feedback themes from the Feedback table to recommend the top 5 staff training priorities, each with a justification and a suggested training format (workshop, e-learning, or role play).

---

### 39. Social Media Post Generator
**Location:** `AdminView.aspx` — Social Media Post Generator panel
**Method:** `OpenAIService.GetSocialMediaPost()`

Admin enters a health awareness topic and selects a platform (Facebook, Instagram, Twitter/X, LinkedIn). GPT-4 writes a ready-to-post message with appropriate length, tone, call-to-action, and 3–5 relevant hashtags for the chosen platform.

---

### 40. Meeting Agenda Generator
**Location:** `AdminView.aspx` — Meeting Agenda Generator panel
**Method:** `OpenAIService.GetMeetingAgenda()`

Admin provides a meeting title, attendees, discussion topics (one per line), and date. GPT-4 generates a professionally structured meeting agenda with a header, timed agenda items per topic, Any Other Business, and a Next Steps & Close section.

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
| `LifestylePlanner.aspx` | Public | AI diet, exercise & lifestyle plan by specialty |
| `InsuranceEstimator.aspx` | Public | AI insurance coverage guide & cost estimator |
| `DoctorAvailability.aspx` | Public | Doctor roster by specialty with AI focus summaries |
| `AppointmentFeedback.aspx` | Public | Post-appointment star rating with AI sentiment analysis |
| `EmergencyTriage.aspx` | Public | 4-level emergency symptom triage |
| `MedicalHistory.aspx` | Public | Medical history summariser |
| `MentalHealthCheckIn.aspx` | Public | Mental health check-in (5 scores + AI analysis) |
| `HealthGoalPlanner.aspx` | Public | 4-week health goal action planner |
| `SecondOpinionPrompts.aspx` | Public | Second opinion question generator from a diagnosis |
| `RecoveryTracker.aspx` | Public | Post-procedure recovery log and AI trend analysis |
| `AppointmentEmailPreview.aspx` | Public | AI-drafted personalised appointment confirmation email |
| `AllergyFoodGuide.aspx` | Public | Personalised allergy & food safety guide |
| `PreSurgerySupport.aspx` | Public | Pre-surgery anxiety support and preparation guide |
| `SpecialistComparison.aspx` | Public | Plain-English comparison of two medical specialties |
| `HealthAgeCalculator.aspx` | Public | Lifestyle-based health age estimate with improvement tips |
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
| `GetLifestylePlan()` | Personalised diet, exercise & lifestyle plan | `string` |
| `GetInsuranceGuide()` | Insurance coverage guide & questions to ask | `string` |
| `GetDoctorFocusSummary()` | 2-sentence focus summary per doctor | `string` |
| `GetFeedbackSummary()` | Executive summary of all patient feedback | `string` |
| `GetEmergencyTriage()` | 4-level urgency classification with action steps | `string` |
| `GetMedicalHistorySummary()` | Structured clinical summary from patient history | `string` |
| `GetMentalHealthCheckIn()` | Wellbeing summary + support suggestions from 5 scores | `string` |
| `GetHealthGoalPlan()` | Personalised 4-week health goal action plan | `string` |
| `GetSecondOpinionQuestions()` | 8-10 smart questions from a diagnosis or treatment plan | `string` |
| `AnalyseRecovery()` | Recovery trend analysis + recommendation badge | `string` |
| `GetStaffPerformanceReport()` | Management performance report from combined data | `string` |
| `GetClinicalAuditReport()` | Formal clinical governance audit document | `string` |
| `GetTrainingRecommendations()` | Top 5 staff training priorities from complaint/feedback data | `string` |
| `GetSocialMediaPost()` | Platform-specific health awareness social post | `string` |
| `GetMeetingAgenda()` | Structured clinical meeting agenda | `string` |
| `GetConfirmationEmailDraft()` | Personalised appointment confirmation email draft | `string` |
| `GetAllergyFoodGuide()` | Personalised food safety guide for listed allergies | `string` |
| `GetPreSurgerySupport()` | Compassionate pre-operative anxiety support guide | `string` |
| `GetSpecialistComparison()` | Plain-English comparison of two specialties | `string` |
| `GetHealthAge()` | Lifestyle-based health age estimate with improvement tips | `string` |
| `GetDischargeSummary()` | Formal patient discharge summary document | `string` |
| `GetPressRelease()` | Professional press release for clinic announcements | `string` |
| `GetJobDescription()` | Full healthcare job description in NHS style | `string` |
| `GetIncidentReport()` | Formal clinical incident report from free-text description | `string` |
| `GetDnaLetter()` | Compassionate DNA (Did Not Attend) letter | `string` |

---

## AI Features — New Patient Pages (41–45)

### 41. Appointment Confirmation Email Preview
**Page:** `AppointmentEmailPreview.aspx`
**Method:** `OpenAIService.GetConfirmationEmailDraft()`

Patient enters their name, appointment date, service, and time slot. GPT-4 drafts a warm personalised confirmation email with preparation tips specific to the booked specialty. A **Copy** button copies the text to clipboard for easy use.

---

### 42. AI Allergy & Food Safety Guide
**Page:** `AllergyFoodGuide.aspx`
**Method:** `OpenAIService.GetAllergyFoodGuide()`

Patient lists their allergies and GPT-4 generates a structured food safety guide covering: foods to avoid, safe food swaps, label-reading tips, and eating-out advice. Always closes with a reminder to carry prescribed emergency medication and consult a dietitian.

---

### 43. Pre-Surgery Anxiety Support
**Page:** `PreSurgerySupport.aspx`
**Method:** `OpenAIService.GetPreSurgerySupport()`

Patient selects their procedure type and optionally describes their concerns. GPT-4 provides a compassionate, evidence-based support guide covering: what to expect, anxiety management techniques (breathing, visualisation), practical preparation steps, and what to expect on the day. Always signposts the clinical team for medical questions.

---

### 44. Specialist Comparison Tool
**Page:** `SpecialistComparison.aspx`
**Method:** `OpenAIService.GetSpecialistComparison()`

Patient selects two specialties from a dropdown. GPT-4 explains what each specialty does, when to see each, the key difference between them, and neutral guidance on how to decide. A **Book an Appointment** quick-link is shown below the result.

---

### 45. Health Age Calculator
**Page:** `HealthAgeCalculator.aspx`
**Method:** `OpenAIService.GetHealthAge()`

Patient fills in six lifestyle indicators: actual age, smoking status, exercise frequency, diet quality, sleep hours, and stress level. GPT-4 estimates a "health age" (which may be higher or lower than actual age), explains which factors influenced it, acknowledges positive habits, and suggests the top 3 improvements. Always includes a disclaimer that this is educational, not a clinical assessment. A quick-link to the Health Goal Planner is shown below the result.

---

## AI Features — New Admin Panels (46–50)

### 46. Patient Discharge Summary Generator
**Location:** `AdminView.aspx` — Clinical Documents section
**Method:** `OpenAIService.GetDischargeSummary()`

Admin enters patient name, diagnosis, treatment provided, discharge medications, and follow-up instructions. GPT-4 formats a formal discharge summary document with all standard sections. Includes a **Print** button.

---

### 47. AI Press Release Generator
**Location:** `AdminView.aspx` — Clinical Documents section
**Method:** `OpenAIService.GetPressRelease()`

Admin enters a news topic (e.g. "New MRI scanner installed") and optional key points. GPT-4 writes a full professional press release with FOR IMMEDIATE RELEASE header, headline, dateline, body paragraphs, a director quote, clinic boilerplate, and contact details.

---

### 48. Job Description Generator
**Location:** `AdminView.aspx` — Clinical Documents section
**Method:** `OpenAIService.GetJobDescription()`

Admin enters a job title, department, and optional key requirements. GPT-4 generates a complete healthcare job description in NHS/professional style with: Job Summary, Key Responsibilities, Essential & Desirable Requirements, What We Offer, and How to Apply sections. Includes a **Print** button.

---

### 49. Clinical Incident Report Writer
**Location:** `AdminView.aspx` — Clinical Documents section
**Method:** `OpenAIService.GetIncidentReport()`

Admin describes an incident in free text and optionally provides a date and location. GPT-4 converts the description into a formal clinical incident report with: Reference number, Incident Description (formalised), Immediate Actions, Contributing Factors, Risk Level, and Recommendations. Includes a **Print** button.

---

### 50. Patient DNA (Did Not Attend) Letter
**Location:** `AdminView.aspx` — Clinical Documents section
**Method:** `OpenAIService.GetDnaLetter()`

Admin selects the patient name, service missed, appointment date, and time slot. GPT-4 generates a professional, compassionate DNA letter that expresses concern for the patient's wellbeing (not blame), encourages rebooking, reminds them of the cancellation policy, and provides contact details. Includes a **Print** button.

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

---

## Feature Showcase — Sample Inputs & Outputs

Each entry below shows a realistic example of what a user types in and what GPT-4 returns on screen. All AI output is HTML-encoded before rendering. When no API key is configured every method falls back gracefully to a hardcoded string.

---

### 1. AI Triage — auto-assigned on every booking
**Page:** `AppointmentForm.aspx`

**Input:**
```
Name: Maria Brown | Age: 58 | Service: Cardiology
Reason: Chest pain and shortness of breath for 3 days
```
**Output (stored in DB, colour-coded in admin grid):**
```
AITriage : Urgent
AINote   : Patient presents with chest pain and shortness of breath lasting
           3 days, which may indicate acute coronary syndrome or pulmonary
           pathology. Recommend immediate ECG and troponin levels on arrival.
           Prioritise for first available cardiologist slot.
```
Badge: 🔴 Urgent

---

### 2. Symptom Checker
**Page:** `SymptomChecker.aspx`

**Input:**
```
Sharp pain behind my right eye, sensitivity to light, and nausea for two days
```
**Output:**
```
Recommended Specialist: Opticology / Neurology

Reason: The combination of sharp retro-orbital pain, photosensitivity, and
nausea may indicate ocular migraine, increased intraocular pressure, or a
neurological concern. An Opticology specialist can rule out eye-related causes;
a neurological assessment is also advised if symptoms persist.
```

---

### 3. Appointment Rescheduler
**Page:** `CancelAppointment.aspx`

**Input:**
```
Name: James Clarke | Service: Gynaecology | Reason: Work conflict
```
**Output:**
```
Hi James, we completely understand that life gets busy and plans change. Your
Gynaecology appointment has been noted for cancellation. We encourage you to
rebook at your earliest convenience — your health is important and our team is
here whenever you're ready. Simply visit our booking page or call us to find a
new time that works. We look forward to seeing you soon!
```

---

### 4. Multi-Language Support
**Navbar → Language dropdown → Spanish**

**Original:** `Welcome to Portmore Medical Center. Book an appointment today.`

**Output:**
```
Bienvenido al Centro Médico Portmore. Reserve una cita con nuestro equipo hoy.
```

---

### 5. Medication Interaction Checker
**Page:** `MedicationChecker.aspx`

**Input:**
```
Warfarin, Aspirin, Ibuprofen, Metformin
```
**Output:**
```
IMPORTANT: This is general educational information only and does not replace
advice from your doctor or pharmacist.

INTERACTIONS FOUND
• Warfarin + Aspirin: Both thin the blood. Combining them significantly increases
  the risk of internal bleeding and requires close medical supervision.
• Warfarin + Ibuprofen: NSAIDs raise Warfarin's blood-thinning effect and
  irritate the stomach lining, increasing bleeding risk.

MONITORING NOTES
• Warfarin requires regular INR blood tests. Vitamin K-rich foods (spinach,
  broccoli) can affect how well it works.
• Metformin should be paused before contrast dye procedures (e.g. CT scans).

Please bring this list to your appointment and discuss with your doctor.
```

---

### 6. Symptom Diary Analyser
**Page:** `SymptomDiary.aspx` — after adding 3+ entries and clicking Analyse.

**Entries:**
```
Mon 10 Mar: Mild lower back pain, 4/10
Tue 11 Mar: Pain worsened to 6/10, stiff on waking
Wed 12 Mar: 7/10, radiating to left leg, difficulty walking
```
**Output:**
```
TREND SUMMARY
Your diary shows a clear worsening trend. Pain has escalated from mild (4/10)
to severe (7/10) with new radiation to the left leg — a pattern that may
suggest nerve involvement or disc-related pathology.

RECOMMENDATION: See your GP soon — the progression and new leg symptoms
warrant prompt assessment.
```

---

### 7. Smart Time Slot Recommender
**Page:** `AppointmentForm.aspx` — "Suggest Best Time for Me" button.

**Input:** `Service: Radiology | Reason: Scheduled MRI follow-up`

**Output:**
```
For a Radiology MRI appointment, a morning slot (8am–10am) is ideal. Scanners
are freshest and less likely to be running behind schedule. An early slot also
means you can fast overnight if required and have results reviewed the same
afternoon.
```

---

### 8. AI Readiness Checklist
**Page:** `AppointmentForm.aspx` — confirmation panel, auto-generated.

**Service booked:** Radiology (MRI)

**Output:**
```
• Fast for at least 4 hours before your MRI (unless advised otherwise).
• Remove all metal jewellery, piercings, and hair clips before arrival.
• Inform staff of any metal implants, pacemakers, or surgical clips.
• Bring your referral letter and any previous scan images or reports.
• Wear comfortable, loose-fitting clothing with no metal fastenings.
• Arrive 15 minutes early to complete a safety screening questionnaire.
```

---

### 9. Patient Education Card
**Page:** `AppointmentForm.aspx` — confirmation panel, auto-generated.

**Service booked:** Cardiology

**Output:**
```
Cardiology focuses on the heart and circulatory system, diagnosing and treating
conditions such as heart disease, high blood pressure, arrhythmias, and heart
failure.

During your appointment your doctor will typically review your medical history,
listen to your heart, check your blood pressure, and may order an ECG or blood
tests. Some visits include an echocardiogram — an ultrasound of the heart.

To get the most from your visit, write down any symptoms you've noticed (including
when they occur), bring a full medication list, and don't hesitate to ask
questions — no question is too small when it comes to your heart health.
```

---

### 10. Duplicate Booking Detector
**Page:** `AppointmentForm.aspx` — yellow warning shown before submission.

**Output:**
```
⚠️ A booking for Maria Brown — Cardiology already exists in our system.
   If this is intentional (e.g. a follow-up), you may still proceed.
```

---

### 11. Triage Escalation Alert
**Page:** `AdminView.aspx` — auto-shown at top of page.

**Condition:** 4 Urgent appointments in DB.

**Output (red banner):**
```
⚠️ ALERT: 4 appointments are currently classified as URGENT.
   Immediate clinical review recommended. See the Appointments grid below.
```

---

### 12. Sentiment Analysis on Contact Messages
**Page:** `ContactForm.aspx` — runs automatically on every submission.

**Patient message:**
```
I have been waiting 6 weeks for my cardiology referral and nobody has called me.
I am very worried and feel completely ignored.
```
**Output (stored in DB, orange badge in admin grid):**
```
Sentiment: Distressed
Reason   : Patient is expressing significant anxiety and frustration over a
           delayed referral, suggesting unmet care needs requiring prompt
           follow-up.
```

---

### 13. No-Show Risk Predictor
**Triggered automatically on every booking.**

**Service:** Cardiology | **Slot:** 4:00 PM – 5:00 PM

**Output (stored in DB):**
```
NoShowRisk: High
```
Badge: 🟠 Orange — admin can proactively send a reminder.

---

### 14. AI Follow-Up Email Drafter
**Page:** `AdminView.aspx`

**Input:**
```
Patient: Maria | Service: Cardiology
Note: ECG normal, follow-up bloods requested
```
**Output:**
```
Dear Maria,

Thank you for attending your Cardiology appointment at Portmore Medical Center.
We hope you are feeling a little more reassured following your visit.

Your ECG results were within normal limits, which is encouraging. As discussed,
we have requested follow-up blood tests to give us a more complete picture. Our
team will be in touch once the results are available.

In the meantime, please contact us if your symptoms change or worsen.

Warm regards,
Portmore Medical Center Patient Care Team
```

---

### 15. Doctor's Note Simplifier
**Page:** `AdminView.aspx`

**Input (clinical):**
```
Acute exacerbation of COPD secondary to suspected viral URTI. SpO2 94% on
room air. Administered nebulised salbutamol 2.5mg and oral prednisolone 30mg.
Recommend outpatient follow-up within 72 hours.
```
**Output (plain English):**
```
Your breathing condition (COPD) has flared up, likely triggered by a viral
chest infection. Your oxygen levels were slightly lower than normal on arrival.
You were given a breathing treatment through a mask (salbutamol nebuliser) and
steroid tablets (prednisolone) to reduce the inflammation in your airways.
Please come back within 3 days for a check-up.
```

---

### 16. AI Dashboard Insights
**Page:** `AdminView.aspx` — Refresh button.

**Output:**
```
Cardiology is your busiest service with 18 bookings this week, followed by
General Practice (14) and Gynaecology (9). You have 4 Urgent-triage appointments
outstanding — recommend reviewing these for immediate scheduling priority.
No-show risk is elevated in the 4–5pm slots across all services; consider
automated reminders for that time band. Booking volume is up 12% on last month.
```

---

### 17. Weekly Demand Forecast
**Page:** `AdminView.aspx`

**Output:**
```
Cardiology and General Practice are forecast to be the highest-demand services
next week, with Cardiology showing a 20% uplift on the prior week. Tuesday and
Wednesday mornings are projected to be peak periods. Consider an additional
Cardiology slot on those days and strengthen front-desk staffing for 8–11am.
Radiology demand appears stable.
```

---

### 18. AI Referral Letter Drafter
**Page:** `AdminView.aspx`

**Input:**
```
Patient: James Turner | Referring: Dr. Anne Reid | From: General Practice
To: Cardiology | Reason: Persistent palpitations, family history of arrhythmia
```
**Output:**
```
Portmore Medical Center
16 March 2026

Dear Cardiology Colleague,

RE: Referral — Mr. James Turner

I write to refer Mr. James Turner for specialist assessment. He presents with
a 6-week history of persistent palpitations and a paternal family history of
atrial fibrillation. An initial ECG was inconclusive. I would be grateful for
your assessment and consideration of Holter monitoring and echocardiography.

Yours sincerely,
Dr. Anne Reid, General Practitioner, Portmore Medical Center
```

---

### 19. Complaint Escalation Handler
**Page:** `AdminView.aspx`

**Input:**
```
I waited 2 hours past my appointment time with no explanation. Staff were
dismissive when I asked how long it would be.
```
**Output:**
```
Dear Patient,

Thank you for contacting us. Please accept our sincere apologies for the
experience you had during your recent visit.

We are sorry you waited significantly beyond your scheduled time without a
clear explanation. Your concern has been escalated to our Patient Experience
Manager, who will review the circumstances and follow up with you within
3 working days.

We value you as a patient and are committed to making this right.

Kind regards,
Patient Experience Team, Portmore Medical Center
```

---

### 20. Monthly Newsletter Generator
**Page:** `AdminView.aspx`

**Output (top services: Cardiology, General Practice, Radiology):**
```
Dear Portmore Medical Center Patients,

Happy March! Cardiology and General Practice have been our most in-demand
services this month — a reminder of how important routine heart health checks
are. If you've been putting off a cardiovascular review, now is the time.

Our Radiology team has also been busy supporting many diagnostic needs this
month. Remember: slots fill up quickly. Book early to secure your preferred time.

Wishing you a healthy and happy month ahead.

The Portmore Medical Center Team
```

---

### 21. Patient Appointment Dashboard
**Page:** `MyAppointments.aspx` — after signing in.

**Dashboard table (sample row):**
```
Service    | Time Slot         | Triage    | No-Show Risk
-----------|-------------------|-----------|-------------
Cardiology | 9:00 AM–10:00 AM  | 🔴 Urgent | 🟠 High
```

**Click "Get AI Tip" on that row:**
```
For a morning Cardiology appointment, avoid caffeine and heavy meals from
the evening before. Bring any previous ECG or test results you have at home.
Arriving 10 minutes early allows time for a resting blood pressure check
before you see the doctor.
```

---

### 22. AI Diet & Lifestyle Planner
**Page:** `LifestylePlanner.aspx`

**Input:** `Specialty: Cardiology | Age: 55 | Conditions: High blood pressure`

**Output:**
```
1. DIET RECOMMENDATIONS
• Follow a DASH or Mediterranean diet — vegetables, fruits, whole grains, lean protein.
• Reduce sodium to under 2,300mg per day; avoid processed foods and takeaways.
• Increase potassium-rich foods: bananas, spinach, sweet potato.
• Choose olive oil over butter; limit saturated fat.

2. EXERCISE & ACTIVITY
• 30 minutes brisk walking, 5 days a week — low impact, heart-healthy.
• Try swimming or cycling as alternatives.
• Always warm up before exercise.

3. LIFESTYLE TIPS
• Monitor blood pressure at home and log readings to share with your doctor.
• Prioritise 7–8 hours sleep — poor sleep raises blood pressure.
• Practice daily stress management: deep breathing, yoga, or mindfulness.

Always follow your doctor's personalised advice above all other guidance.
```

---

### 23. AI Insurance & Cost Estimator
**Page:** `InsuranceEstimator.aspx`

**Input:** `Service: Cardiology | Insurance: Private Health Insurance`

**Output:**
```
1. WHAT IS TYPICALLY COVERED
• Initial specialist consultation — usually covered subject to excess.
• Diagnostic tests (ECG, echocardiogram) may require pre-authorisation.
• Emergency cardiac hospital admissions typically covered.

2. QUESTIONS TO ASK YOUR INSURER
- Is Cardiology included under my specialist cover?
- What is my excess or co-payment for a consultant appointment?
- Do I need a GP referral before I can claim?
- Are diagnostic tests covered under the same claim?
- What is my annual cardiac benefit limit?

3. COST GUIDANCE
Costs vary widely by plan tier and depend on whether diagnostics are included
in the same session. Complex procedures are subject to your benefit schedule.

This is general guidance only. Contact your insurer and our billing team for
exact details.
```

---

### 24. Doctor Availability Viewer
**Page:** `DoctorAvailability.aspx` — select Cardiology.

**Output (doctor card):**
```
Dr. Michael Knapton — Cardiology
Available slots: [8am–9am]  [11am–12pm]  [2pm–3pm]

Dr. Knapton specialises in interventional cardiology and heart failure management
at Portmore Medical Center, bringing a patient-centred approach to complex cardiac
conditions.
                                        [Book with this Doctor]
```

---

### 25. Post-Appointment Feedback
**Page:** `AppointmentFeedback.aspx`

**Input:** `Rating: ⭐⭐⭐⭐⭐ | Service: Cardiology | Comment: "Dr. Knapton was brilliant — very reassuring."`

**Output (thank-you screen):**
```
✅ Thank you for your feedback!

Sentiment: 🟢 POSITIVE
"Patient expressed high satisfaction with clinical care and doctor communication."
```

---

### 26. Emergency Symptom Triage
**Page:** `EmergencyTriage.aspx`

**Input:**
```
Sudden severe chest pain radiating to my left arm, sweating heavily, feeling
sick and dizzy. Started 20 minutes ago.
```
**Output:**
```
┌─────────────────────────────────────┐
│  🔴  EMERGENCY                      │
└─────────────────────────────────────┘

Level: EMERGENCY
Reason: These are classic warning signs of a myocardial infarction (heart
attack). This is a time-critical medical emergency.
Action: Call 999 immediately or go to A&E right now. Do not drive yourself.
Chew an aspirin (300mg) if available and not allergic.

If symptoms worsen, seek immediate medical attention.
```

---

### 27. Medical History Summariser
**Page:** `MedicalHistory.aspx`

**Input:**
```
Conditions : Type 2 Diabetes, Hypertension
Medications: Metformin 500mg BD, Amlodipine 5mg OD
Allergies  : Penicillin (rash)
Surgeries  : Appendectomy 2008
Family Hx  : Father — MI age 62
```
**Output:**
```
MEDICAL HISTORY SUMMARY

ACTIVE CONDITIONS
• Type 2 Diabetes Mellitus — under medical management
• Hypertension — under medical management

CURRENT MEDICATIONS
• Metformin 500mg — twice daily
• Amlodipine 5mg — once daily

KNOWN ALLERGIES
• Penicillin — reaction: rash (document as drug allergy)

SURGICAL HISTORY
• Appendectomy (2008) — uncomplicated

FAMILY HISTORY
• Paternal: Myocardial infarction at age 62 — elevated cardiovascular risk

CLINICAL NOTE
Given the family history of premature MI and current cardiovascular risk factors,
routine cardiovascular screening and HbA1c monitoring are recommended.
```

---

### 28. Mental Health Check-In
**Page:** `MentalHealthCheckIn.aspx`

**Input:** `Mood: 2 | Sleep: 2 | Anxiety: 2 | Energy: 2 | Social: 1 | Notes: "Feeling isolated since losing my job."`

**Output:**
```
1. WELLBEING SUMMARY
Thank you for checking in honestly. Your scores suggest you're going through a
particularly difficult time — low mood, disrupted sleep, and feelings of isolation
are a completely understandable response to job loss.

2. SUGGESTED SUPPORT
• Build a simple daily routine: consistent wake and sleep times stabilise mood.
• Reach out — even a short call or text to someone you trust can make a real difference.
• Gentle movement: a 20-minute daily walk in natural light improves mood and energy.
• Limit news and social media scrolling, which can amplify anxiety.

3. WHEN TO SEEK HELP
If you have felt this way for more than two weeks, or you are having thoughts of
harming yourself, please speak to your GP as soon as possible.
Samaritans: 116 123 (free, 24/7).
```

---

### 29. Health Goal Planner
**Page:** `HealthGoalPlanner.aspx`

**Input:** `Goal: Lower blood pressure, lose 8kg | Activity: 2–3x/week | Conditions: Hypertension`

**Output:**
```
WEEK 1 — Foundation
• Replace one processed meal per day with a home-cooked option (less salt).
• Begin 20-minute brisk walks, 3 times this week.
• Download a BP app and take morning readings.

WEEK 2 — Building
• Extend walks to 30 minutes, 4 times per week.
• Reduce takeaways to once this week; add two extra vegetable portions daily.

WEEK 3 — Progress
• Introduce a second form of exercise — try swimming or cycling.
• Review your BP log and note any downward trend.

WEEK 4 — Consolidate
• Maintain the exercise habit. Set a 3-month weight goal.
• Book a GP follow-up to review blood pressure readings.

TIPS FOR SUCCESS
• Tell someone your goal — accountability doubles your success rate.
• Celebrate small wins — each healthy meal and walk is progress.

Always consult your doctor before making significant changes to your health routine.
```

---

### 30. Second Opinion Question Generator
**Page:** `SecondOpinionPrompts.aspx`

**Input:** `I've been told I need a total knee replacement for severe osteoarthritis.`

**Output:**
```
UNDERSTANDING THE DIAGNOSIS
1. How severe is my arthritis compared to others who have had this surgery?
2. Can I see the imaging that shows why replacement is now necessary?
3. What are the long-term risks if I delay or decline the surgery?

TREATMENT OPTIONS
4. Are there non-surgical options we haven't tried — injections, specialist physio?
5. What type of implant would be used, and how long is it expected to last?
6. What does recovery look like — how long until I can walk normally?
7. What are the risks of complications such as infection or implant failure?

NEXT STEPS & LIFESTYLE
8. What can I do before surgery to improve my outcome (strength, weight loss)?
9. Would you recommend I seek a second orthopaedic opinion before proceeding?
```

---

### 31. Post-Treatment Recovery Tracker
**Page:** `RecoveryTracker.aspx`

**Entries:** Knee replacement, 4 daily logs.
```
Day 1: Pain 8/10, bed rest
Day 3: Pain 6/10, walking with crutches
Day 5: Pain 5/10, walking unaided to bathroom
Day 7: Pain 3/10, 10-min physio exercises completed
```
**Output:**
```
RECOVERY TREND
Clear and encouraging improvement. Pain has reduced from severe (8/10) to mild
(3/10). Early mobilisation and physio engagement are both positive indicators.

POSITIVE SIGNS : Consistent pain reduction; early mobilisation achieved.
AREAS TO WATCH : Monitor swelling — contact team if it increases or becomes hot/red.

RECOMMENDATION: ✅ Recovery appears on track
```

---

### 32. Appointment Confirmation Email Preview
**Page:** `AppointmentEmailPreview.aspx`

**Input:** `Name: David Thompson | Service: Cardiology | Date: 25 Mar 2026 | Time: 9:00 AM`

**Output (copyable draft):**
```
Dear David,

We are delighted to confirm your upcoming appointment at Portmore Medical Center.

Service : Cardiology
Date    : 25 March 2026
Time    : 9:00 AM – 10:00 AM

PREPARING FOR YOUR VISIT
• Avoid caffeine and heavy meals on the morning of your appointment.
• Bring a list of your current medications and any previous ECG results.
• Wear comfortable, loose-fitting clothing.
• Arrive 10 minutes early for a resting blood pressure check.

To reschedule: appointments@portmoremedical.com | +1 (876) 555-0100

We look forward to welcoming you.
The Portmore Medical Center Team
```

---

### 33. AI Allergy & Food Safety Guide
**Page:** `AllergyFoodGuide.aspx`

**Input:** `peanuts, tree nuts, shellfish`

**Output:**
```
FOODS TO AVOID
• Peanuts : peanut butter, satay sauce, many Asian dishes, baked goods,
  cereal bars. Hidden in: ice cream, sauces, mixed snacks.
• Tree Nuts: marzipan, nut oils, pesto, many desserts, salad toppings.
• Shellfish: paella, sushi, some soups. Hidden in: Worcester sauce, seafood crisps.

SAFE FOOD SWAPS
• Peanut butter  → sunflower seed butter or tahini
• Prawn crackers → plain rice crackers
• Mixed nuts     → pumpkin or sunflower seeds (check labels)

LABEL READING TIPS
• Look for the "Contains:" allergen box — peanuts, tree nuts, and crustaceans
  must be declared in bold under UK/EU law.
• Check "May contain traces of nuts" — indicates shared production equipment.
• Be cautious with "natural flavouring" — it may mask allergens.

EATING OUT SAFELY
• Always tell your server about all three allergies before ordering.
• Ask specifically about shared surfaces and shared cooking oil.
• Avoid buffet-style restaurants where cross-contamination risk is highest.

IMPORTANT REMINDER
Always carry any prescribed emergency medication (such as an EpiPen) and consult
your doctor or dietitian for personalised advice.
```

---

### 34. Pre-Surgery Anxiety Support
**Page:** `PreSurgerySupport.aspx`

**Input:** `Procedure: Cardiac Surgery | Concern: Terrified of the general anaesthetic`

**Output:**
```
WHAT TO EXPECT
Cardiac surgery is performed by a highly skilled team — surgeon, anaesthetist,
and specialist nurses — who are with you every moment. You will not feel or
remember anything during the procedure.

MANAGING ANXIETY
• Box breathing: inhale 4 counts, hold 4, exhale 4, hold 4. Repeat 4 times.
• Visualise yourself recovering well; focus on the positive outcome.
• Share your specific fear with your anaesthetist — they hear this often and
  can explain exactly how you'll be monitored throughout.
• Write worries down; externalising them reduces their power significantly.

PRACTICAL PREPARATION
• Follow fasting instructions precisely — usually nothing after midnight.
• Shower with antiseptic wash the night before and morning of surgery.
• Arrange a trusted person to accompany you and take you home.

ON THE DAY
• The team will introduce themselves and answer your questions before theatre.
• A nurse will be present immediately when you wake up to reassure you.

REMEMBER
Your clinical team is here to support you every step of the way.
Please ask them any medical questions you have.
```

---

### 35. Specialist Comparison Tool
**Page:** `SpecialistComparison.aspx`

**Input:** `Cardiology  vs  General Practitioner`

**Output:**
```
CARDIOLOGY — WHAT THEY DO
A Cardiologist focuses exclusively on the heart and cardiovascular system,
diagnosing and treating coronary artery disease, heart failure, and arrhythmias.

CARDIOLOGY — WHEN TO SEE THEM
• Referred by GP following abnormal ECG or echo result.
• Experiencing chest pain, palpitations, or unexplained breathlessness.
• Family history of heart disease requiring specialist assessment.

GENERAL PRACTITIONER — WHAT THEY DO
A GP is your first point of contact for any health concern — they assess,
diagnose, treat a wide range of conditions, and refer to specialists as needed.

GENERAL PRACTITIONER — WHEN TO SEE THEM
• You have a new symptom you are unsure about.
• You need a check-up, blood tests, or repeat prescription.
• You want a referral to see a Cardiologist or any other specialist.

KEY DIFFERENCE
A GP is your generalist first-stop; a Cardiologist is a specialist you would
typically see after GP referral for heart-specific concerns.

RECOMMENDATION
Start with your GP — they will assess your symptoms and refer to a Cardiologist
if clinically indicated.
```

---

### 36. Health Age Calculator
**Page:** `HealthAgeCalculator.aspx`

**Input:** `Age: 45 | Smoking: Non-smoker | Exercise: 2–3x/week | Diet: Good | Sleep: 6hrs | Stress: High`

**Output:**
```
HEALTH AGE ESTIMATE
Your estimated health age is approximately 48. The two factors pulling your
health age above your actual age are elevated stress and consistently getting
only 6 hours of sleep — both have significant cumulative effects on
cardiovascular health, immune function, and cognitive performance.

TOP 3 IMPROVEMENTS
1. Sleep — Prioritising 7–8 hours nightly would have the single biggest impact.
2. Stress — 10 minutes of daily mindfulness or a pre-bed digital detox can
   reduce cortisol and improve sleep quality simultaneously.
3. Exercise — increasing to 4–5 sessions per week would meaningfully reduce
   your cardiovascular risk profile.

POSITIVE HABITS
You are a non-smoker, maintain a good diet, and already exercise regularly —
significant positives already working in your favour.

DISCLAIMER
This is a general wellness estimate for educational purposes only and is not a
medical assessment. Please speak to your GP for personalised health advice.
```

---

### 37. Staff Performance Report
**Page:** `AdminView.aspx`

**Output:**
```
STAFF PERFORMANCE REPORT — Portmore Medical Center

1. BUSIEST DEPARTMENTS
Cardiology leads demand (28%), followed by General Practice (22%) and
Gynaecology (15%). All three are operating at high utilisation.

2. PATIENT SATISFACTION
Average feedback rating: 4.2 / 5. Sentiment: 71% Positive, 21% Neutral,
6% Distressed, 2% Urgent.

3. OPERATIONAL CONCERNS
• 4 Urgent-triage appointments currently unresolved.
• No-show rates highest in 4–5pm Cardiology slots (est. 18%).
• 2 Distressed feedback submissions remain unacknowledged.

4. RECOMMENDATIONS
• Introduce SMS reminders 48 hours before all appointments.
• Assign a Patient Liaison to contact Distressed feedback patients within 24hrs.
• Review Cardiology scheduling capacity to address sustained high demand.
```

---

### 38. Clinical Audit Report
**Page:** `AdminView.aspx`

**Output (excerpt):**
```
CLINICAL AUDIT REPORT — Portmore Medical Center
Date: 16 March 2026

EXECUTIVE SUMMARY
Overall clinical performance is satisfactory with patient satisfaction trending
positively. Escalation of urgent triage cases and no-show management present
clear opportunities for process improvement.

KEY FINDINGS
• 28% of all appointments: Cardiology — the highest of all services.
• 4 appointments hold Urgent triage classification requiring same-day review.
• Patient feedback average: 4.2 / 5.
• Most common complaint theme: waiting times.
• 8% of contact submissions classified Distressed or Urgent.

RECOMMENDATIONS
1. Establish a same-day triage review protocol for all Urgent appointments.
2. Implement automated reminders (SMS/email) 24–48 hours before appointments.
3. Create a Distressed Patient Pathway with a guaranteed 24-hour response.
```

---

### 39. Staff Training Topic Recommender
**Page:** `AdminView.aspx`

**Output:**
```
1. PATIENT COMMUNICATION & WAIT TIME MANAGEMENT
   Why    : Waiting times are the most cited complaint theme.
   Format : Half-day workshop with role-play scenarios.

2. DISTRESSED PATIENT RESPONSE PROTOCOL
   Why    : 8% of contact submissions classified Distressed or Urgent.
   Format : E-learning module + in-person debrief.

3. APPOINTMENT SCHEDULING & NO-SHOW REDUCTION
   Why    : No-show risk highest in late afternoon slots; no reminder system in place.
   Format : E-learning with process update guide.

4. CLINICAL TRIAGE AWARENESS FOR FRONT-DESK STAFF
   Why    : 4 Urgent cases unresolved — gap between AI classification and escalation.
   Format : Practical workshop with GP facilitator.

5. COMPLAINT HANDLING & DE-ESCALATION
   Why    : Response time not formalised; multiple submissions unacknowledged.
   Format : Role-play with Patient Experience Manager.
```

---

### 40. Social Media Post Generator
**Page:** `AdminView.aspx`

**Input:** `Topic: World Heart Day | Platform: Instagram`

**Output:**
```
❤️ It's World Heart Day — and your heart deserves some love!

Did you know up to 80% of premature heart events are preventable through
lifestyle changes?

✅ Move for 30 minutes a day
✅ Eat more vegetables and less salt
✅ Know your blood pressure numbers
✅ Never ignore chest pain or palpitations

Book a cardiovascular check with our Cardiology team at Portmore Medical Center.
Early detection saves lives. 💙

#WorldHeartDay #HeartHealth #PortmoreMedical #HealthyHeart #CardiovascularHealth
```

---

### 41. Meeting Agenda Generator
**Page:** `AdminView.aspx`

**Input:** `Title: Monthly Clinical Governance | Date: 28 Mar 2026 | Topics: Triage review, Feedback, Staffing rota`

**Output:**
```
PORTMORE MEDICAL CENTER — MEETING AGENDA
Title     : Monthly Clinical Governance Meeting
Date      : 28 March 2026
Attendees : Dr. Clarke (Medical Director), Head Nurse Williams, Admin Manager Brown

1. Welcome & Apologies                              5 min
2. Triage Escalation Review                        20 min
   • Current Urgent triage cases
   • Escalation protocol compliance update
3. Patient Feedback Analysis                       15 min
   • Satisfaction score summary (March 2026)
   • Distressed submission follow-up status
4. Staffing Rota                                   15 min
   • April rota sign-off | Easter bank holiday cover
5. Any Other Business                               5 min
6. Next Steps & Close                               5 min
   Next meeting: 25 April 2026

Minutes to be circulated within 48 hours.
```

---

### 42. Patient Discharge Summary Generator
**Page:** `AdminView.aspx` — Clinical Documents section.

**Input:**
```
Patient  : Maria Brown
Diagnosis: Hypertensive crisis, resolved
Treatment: IV labetalol administered. BP stabilised. Monitored 4 hours.
Meds     : Amlodipine 10mg OD, Ramipril 5mg OD
Follow-up: GP review in 5 days. Repeat bloods in 2 weeks.
```
**Output:**
```
PATIENT DISCHARGE SUMMARY
Reference : PMC-DS-2026-0316 | Date: 16 March 2026
Patient   : Maria Brown

DIAGNOSIS
Hypertensive crisis — resolved. Admission BP: 210/120mmHg.
Discharge BP: 148/88mmHg. No evidence of end-organ damage.

TREATMENT PROVIDED
IV labetalol administered to achieve blood pressure control. Patient monitored
for 4 hours post-treatment. Counselled on medication adherence and lifestyle.

DISCHARGE MEDICATIONS
• Amlodipine 10mg — once daily (dose increased)
• Ramipril 5mg — once daily (continued)

FOLLOW-UP INSTRUCTIONS
• GP review within 5 days — bring home BP log if available.
• Repeat blood tests (U&E, renal function) in 2 weeks.
• Return to A&E if BP exceeds 180/110mmHg or symptoms recur.

Prepared by: Portmore Medical Center Clinical Team
```

---

### 43. AI Press Release Generator
**Page:** `AdminView.aspx` — Clinical Documents section.

**Input:** `Topic: New 3T MRI scanner | Key points: 40% faster scans, from April 2026, £2M investment`

**Output:**
```
FOR IMMEDIATE RELEASE

PORTMORE MEDICAL CENTER UNVEILS STATE-OF-THE-ART 3T MRI SCANNER,
CUTTING PATIENT SCAN TIMES BY 40%

Portmore, Jamaica — 16 March 2026 — Portmore Medical Center today announced the
operational launch of its new 3T MRI scanner — a £2M investment available from
April 2026. Scan times are reduced by 40% with superior image quality enabling
earlier, more precise diagnoses across Neurology, Orthopaedics, and Cardiology.

"This is a landmark moment for our patients," said Dr. James Clarke, Medical
Director, Portmore Medical Center. "Faster imaging means faster diagnoses and
better outcomes."

Contact: media@portmoremedical.com | +1 (876) 555-0100
```

---

### 44. Job Description Generator
**Page:** `AdminView.aspx` — Clinical Documents section.

**Input:** `Role: Senior Cardiac Nurse Specialist | Dept: Cardiology | Requirements: 5yr cardiac nursing, NMC, ECG competency`

**Output:**
```
JOB TITLE  : Senior Cardiac Nurse Specialist
DEPARTMENT : Cardiology
REPORTS TO : Lead Cardiologist / Director of Nursing

JOB SUMMARY
Portmore Medical Center is seeking an experienced Senior Cardiac Nurse Specialist
to deliver expert care to cardiac patients, support diagnostics, and contribute
to team development.

KEY RESPONSIBILITIES
• Deliver evidence-based care for heart failure, arrhythmias, and post-MI patients.
• Perform and interpret 12-lead ECGs.
• Educate patients and families on cardiac conditions and medications.
• Maintain accurate clinical records in line with NMC standards.
• Mentor junior nursing staff.

ESSENTIAL REQUIREMENTS
• Registered Nurse (NMC) — active registration required.
• Minimum 5 years post-qualification experience, 3+ in a cardiac setting.
• Demonstrable ECG recording and interpretation competency.

HOW TO APPLY
Submit your CV and cover letter to careers@portmoremedical.com
```

---

### 45. Clinical Incident Report Writer
**Page:** `AdminView.aspx` — Clinical Documents section.

**Input:**
```
Description: Patient fell from waiting room chair and hit head on floor.
             Staff responded immediately and administered first aid.
Date: 15 March 2026 | Location: Cardiology Waiting Room
```
**Output:**
```
CLINICAL INCIDENT REPORT
Reference  : PMC-IR-2026-0892
Date       : 15 March 2026
Location   : Cardiology Waiting Room, Portmore Medical Center

INCIDENT DESCRIPTION
A patient sustained a fall from a seated position, resulting in head contact with
the floor surface. Clinical staff responded promptly and administered first aid.

IMMEDIATE ACTIONS TAKEN
• First aid assessment performed; neurological observations commenced.
• Incident reported to senior nurse on duty and formally documented.
• Waiting room inspected for contributing environmental factors.

CONTRIBUTING FACTORS
Potential patient instability (faintness/vertigo); chair design and layout
to be assessed for fall-risk compliance.

RISK LEVEL: MEDIUM
Justification: Head injury with potential for delayed neurological effects.

RECOMMENDATIONS
1. Conduct a formal waiting room fall-risk assessment.
2. Replace standard chairs with armchairs in patient waiting areas.
3. Implement welfare checks for patients waiting beyond 30 minutes.
4. Ensure all staff complete falls prevention refresher training annually.

Report prepared by: Portmore Medical Center Governance Team
```

---

### 46. Patient DNA (Did Not Attend) Letter
**Page:** `AdminView.aspx` — Clinical Documents section.

**Input:** `Patient: John Smith | Service: Cardiology | Date: 10 Mar 2026 | Time: 9:00 AM`

**Output:**
```
Dear Mr. John Smith,

I am writing regarding your Cardiology appointment on 10 March 2026 at 9:00 AM,
which we note you were unfortunately unable to attend.

We hope you are keeping well. We understand that circumstances sometimes make it
difficult to attend planned appointments and we are not writing to cause distress.

However, as we were unable to offer your slot to another patient, we kindly
encourage you to rebook as soon as possible. Your Cardiology review is important
and we would not want any delay to affect your care.

Where possible, please give us at least 24 hours notice for future cancellations.

To rebook: appointments@portmoremedical.com | +1 (876) 555-0100

Kind regards,
Patient Services, Portmore Medical Center
```

---

### 47. Staff Bio Generator
**Page:** `StaffBioGenerator.aspx`

**Input:** `Name: Dr. Michael Knapton | Specialty: Cardiology | Qualifications: MBBS, MD, FRCP | Years: 22`

**Output:**
```
Dr. Michael Knapton is a highly experienced Consultant Cardiologist at Portmore
Medical Center, bringing over 22 years of specialist expertise in cardiovascular
medicine to his practice.

Dr. Knapton holds an MBBS, MD, and is a Fellow of the Royal College of Physicians
(FRCP). His clinical interests include the diagnosis and management of heart
failure, coronary artery disease, and cardiac arrhythmias.

A warm and dedicated physician, Dr. Knapton takes pride in ensuring every patient
feels heard and fully informed during their consultation at Portmore Medical Center.
```

---

### 48. AI Feedback Summary
**Page:** `AdminView.aspx` — Generate Summary button in Feedback section.

**Output (24 submissions, avg 4.3/5):**
```
Overall patient satisfaction remains strong at 4.3 / 5. The most consistently
praised themes are clinical expertise and doctor communication, with multiple
patients highlighting that staff explained procedures clearly. The primary area
for improvement is waiting times, particularly for afternoon Cardiology slots.
One submission was classified Distressed and has been flagged for follow-up.

Recommended action: Review afternoon Cardiology scheduling buffers and consider
a proactive update message for patients waiting beyond 20 minutes.
```

---

### 49. Daily Health Tip
**Page:** `HomePage.aspx` — generated once per day, cached for all visitors.

**Output (example):**
```
💡 Today's Health Tip: Staying hydrated is one of the simplest things you can
do for your health. Aim for 6–8 glasses of water per day — more if you are
active or in warm weather. Proper hydration supports kidney function,
concentration, and even mood. Keep a glass on your desk as a visual reminder.
```

---

### 50. Pre-Appointment Questionnaire Analyser
**Page:** `Questionnaire.aspx` — summary passed through to `AppointmentForm.aspx`.

**Input:**
```
Main symptom : Chest tightness and breathlessness on exertion
Duration     : 3 weeks | Severity: 7/10
Prior episodes: Yes — twice in the last month
Medications  : Aspirin, Atorvastatin
```
**Output (pre-screening summary, stored in session):**
```
Patient reports recurring chest tightness and exertional breathlessness (severity
7/10) over 3 weeks, with two prior episodes in the last month. Current medications
include Aspirin and Atorvastatin, suggesting existing cardiovascular risk
management. Clinical assessment should prioritise ruling out acute coronary
syndrome or heart failure. Early ECG and cardiac biomarkers recommended on arrival.
```

---

*Portmore Medical Center — ASP.NET Doctor Project*
*Built with ASP.NET Web Forms, SQL Server LocalDB, Bootstrap 3, and OpenAI GPT-4*
