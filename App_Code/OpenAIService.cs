using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Script.Serialization;

/// <summary>
/// Static service class that wraps the OpenAI GPT-4 Chat Completions API.
/// Every public method corresponds to one AI feature in the application.
/// The private CallGpt helper handles all HTTP transport and response parsing,
/// keeping feature methods focused on prompt engineering only.
/// </summary>
public static class OpenAIService
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

    // -----------------------------------------------------------------------
    // Shared private helper
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sends a single-turn user message to GPT-4 and returns the raw content string.
    /// Returns null if the API key is not configured or if any network/parse error occurs.
    /// </summary>
    /// <param name="userContent">The user-role message to send.</param>
    /// <param name="temp">Sampling temperature (0 = deterministic, 1 = creative).</param>
    /// <param name="maxTokens">Maximum tokens in the completion.</param>
    private static string CallGpt(string userContent, double temp = 0.7, int maxTokens = 400)
    {
        string apiKey = ConfigurationManager.AppSettings["OpenAIApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            return null;

        var payload = new Dictionary<string, object>
        {
            { "model",       "gpt-4" },
            { "temperature", temp },
            { "max_tokens",  maxTokens },
            { "messages", new[] { new Dictionary<string, string> { { "role", "user" }, { "content", userContent } } } }
        };

        try
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type",  "application/json");
                client.Headers.Add("Authorization", "Bearer " + apiKey);
                string resp    = client.UploadString("https://api.openai.com/v1/chat/completions", Json.Serialize(payload));
                var outer      = (Dictionary<string, object>)Json.DeserializeObject(resp);
                var choices    = (ArrayList)outer["choices"];
                var msgDict    = (Dictionary<string, object>)((Dictionary<string, object>)choices[0])["message"];
                return msgDict["content"].ToString().Trim();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("OpenAIService.CallGpt error: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Strips optional markdown code fences (```json ... ```) that GPT sometimes wraps around JSON output.
    /// </summary>
    private static string StripCodeFences(string s)
    {
        if (!s.StartsWith("```")) return s;
        int start = s.IndexOf('{');
        int end   = s.LastIndexOf('}');
        return (start >= 0 && end > start) ? s.Substring(start, end - start + 1) : s;
    }

    // -----------------------------------------------------------------------
    // 1. Patient appointment triage
    // -----------------------------------------------------------------------

    /// <summary>
    /// Triage priority assigned by GPT-4 together with a brief clinical pre-assessment note.
    /// </summary>
    public class TriageResult
    {
        /// <summary>One of: Urgent | High | Medium | Low | Pending | Error</summary>
        public string Triage { get; set; }
        /// <summary>2–3 sentence clinical pre-assessment for the treating doctor.</summary>
        public string Note   { get; set; }
    }

    /// <summary>
    /// Sends patient intake data to GPT-4 and returns a triage priority and pre-assessment note.
    /// Falls back to a "Pending" result if the API key is absent or the call fails.
    /// </summary>
    public static TriageResult GetTriage(string firstName, string lastName, string age, string services, string issue)
    {
        string apiKey = ConfigurationManager.AppSettings["OpenAIApiKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            return new TriageResult { Triage = "Pending", Note = "AI triage not configured. Add your OpenAI API key to Web.config." };

        string prompt =
            "You are a medical triage assistant at a clinic. Based on the patient intake below, " +
            "assess urgency and provide a brief pre-assessment note for the treating doctor.\n\n" +
            "Patient: " + firstName + " " + lastName + "\n" +
            "Age: " + age + "\n" +
            "Service Requested: " + services + "\n" +
            "Reason for Appointment: " + issue + "\n\n" +
            "Respond with ONLY valid JSON (no markdown, no code blocks) in exactly this format:\n" +
            "{\"triage\":\"Urgent\",\"note\":\"Your 2-3 sentence clinical pre-assessment here.\"}\n\n" +
            "Triage levels: Urgent = life-threatening / severe, High = significant concern, " +
            "Medium = routine but needs attention, Low = minor / follow-up.";

        var requestPayload = new Dictionary<string, object>
        {
            { "model",       "gpt-4" },
            { "temperature", 0.2 },
            { "max_tokens",  250 },
            { "messages", new[]
                {
                    new Dictionary<string, string>
                    {
                        { "role",    "user" },
                        { "content", prompt }
                    }
                }
            }
        };

        try
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type",  "application/json");
                client.Headers.Add("Authorization", "Bearer " + apiKey);

                string responseJson = client.UploadString(
                    "https://api.openai.com/v1/chat/completions", Json.Serialize(requestPayload));

                var outer   = (Dictionary<string, object>)Json.DeserializeObject(responseJson);
                var choices = (ArrayList)outer["choices"];
                var choice  = (Dictionary<string, object>)choices[0];
                var message = (Dictionary<string, object>)choice["message"];
                string content = StripCodeFences(message["content"].ToString().Trim());

                var inner = (Dictionary<string, object>)Json.DeserializeObject(content);
                return new TriageResult
                {
                    Triage = inner["triage"].ToString(),
                    Note   = inner["note"].ToString()
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("OpenAIService.GetTriage error: " + ex.Message);
            return new TriageResult { Triage = "Error", Note = "AI triage could not be completed at this time." };
        }
    }

    // -----------------------------------------------------------------------
    // 2. Symptom-to-specialist checker
    // -----------------------------------------------------------------------

    /// <summary>Specialist recommendation returned by GetSpecialistRecommendation.</summary>
    public class SpecialistResult
    {
        public string Specialist { get; set; }
        public string Reason     { get; set; }
    }

    /// <summary>
    /// Given a patient's symptom description, returns the most appropriate specialist service.
    /// </summary>
    public static SpecialistResult GetSpecialistRecommendation(string symptoms)
    {
        string prompt =
            "A patient at Portmore Medical Center describes the following symptoms or health concerns:\n\n" +
            "\"" + symptoms + "\"\n\n" +
            "Which ONE of these specialist services best matches their needs?\n" +
            "- Cardiology\n- General Practitioner\n- Gynaecology\n- Opticology\n- Paediatrics\n- Radiology\n- Surgery\n\n" +
            "Respond ONLY with valid JSON (no markdown, no code blocks) in exactly this format:\n" +
            "{\"specialist\":\"Service Name\",\"reason\":\"1-2 sentence plain-English explanation for the patient.\"}";

        string content = CallGpt(prompt, temp: 0.3, maxTokens: 150);

        if (content == null)
            return new SpecialistResult { Specialist = "General Practitioner", Reason = "Please book a General Practitioner appointment and they will refer you to the right specialist." };

        content = StripCodeFences(content);

        try
        {
            var inner = (Dictionary<string, object>)Json.DeserializeObject(content);
            return new SpecialistResult
            {
                Specialist = inner["specialist"].ToString(),
                Reason     = inner["reason"].ToString()
            };
        }
        catch
        {
            return new SpecialistResult { Specialist = "General Practitioner", Reason = content };
        }
    }

    // -----------------------------------------------------------------------
    // 3. Admin AI dashboard insights
    // -----------------------------------------------------------------------

    /// <summary>
    /// Given a plain-text summary of appointment statistics, returns 3-5 sentences of
    /// operational insight to help the admin team prioritise resources.
    /// </summary>
    public static string GetAdminInsights(string dataSummary)
    {
        string prompt =
            "You are an AI analyst for Portmore Medical Center. " +
            "Based on the following appointment statistics, provide 3-5 sentences of concise operational insight " +
            "to help the admin team understand patient demand and prioritise resources:\n\n" +
            dataSummary;

        return CallGpt(prompt, temp: 0.5, maxTokens: 300)
               ?? "AI insights are unavailable at this time.";
    }

    // -----------------------------------------------------------------------
    // 4. Contact form auto-reply draft
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a short, professional reply to a patient's contact form message on behalf of the clinic.
    /// </summary>
    public static string GetContactReply(string firstName, string patientMessage)
    {
        string prompt =
            "Draft a short, professional, and warm email reply from Portmore Medical Center to a patient named " +
            firstName + " who sent the following enquiry:\n\n\"" + patientMessage + "\"\n\n" +
            "Acknowledge their message, be helpful and reassuring, and invite them to call or visit if needed. " +
            "Sign off as 'The Portmore Medical Center Team'. Keep the reply under 120 words.";

        return CallGpt(prompt, temp: 0.7, maxTokens: 200)
               ?? "Thank you for contacting Portmore Medical Center. A member of our team will be in touch with you shortly.";
    }

    // -----------------------------------------------------------------------
    // 5. Daily health tip
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a daily health tip. Callers should cache the result in Application state keyed by date.
    /// </summary>
    public static string GetHealthTip()
    {
        string date = DateTime.Now.ToString("MMMM d, yyyy");
        string prompt =
            "Generate one concise, practical, and positive health tip for patients of a medical center for today, " +
            date + ". Make it actionable and 2-3 sentences long. " +
            "Do not include a headline, bullet points, or any formatting — just the tip as plain prose.";

        return CallGpt(prompt, temp: 0.8, maxTokens: 120)
               ?? "Stay hydrated, get plenty of rest, and don't hesitate to book a check-up if you have any health concerns — early detection saves lives.";
    }

    // -----------------------------------------------------------------------
    // 6. General site assistant (chat)
    // -----------------------------------------------------------------------

    private const string ChatSystemPrompt =
        "You are the helpful AI assistant for Portmore Medical Center. " +
        "Your role is to help patients and visitors learn about the clinic, its services, staff, and how to book appointments.\n\n" +
        "Services offered:\n" +
        "- Cardiology (heart-related conditions)\n" +
        "- General Practitioner (routine check-ups, general health concerns)\n" +
        "- Gynaecology (women's health)\n" +
        "- Opticology (eye care and vision)\n" +
        "- Paediatrics (child health)\n" +
        "- Radiology (medical imaging and scans)\n" +
        "- Surgery (surgical procedures)\n\n" +
        "Patients can book an appointment via the Appointment Form on the website. " +
        "They can Sign Up for an account or Sign In to access personalised features. " +
        "The Contact Form is available for general enquiries.\n\n" +
        "Be friendly, professional, and concise. " +
        "For medical emergencies always advise the patient to call emergency services or go to the nearest emergency room immediately. " +
        "Do not provide specific diagnoses, prescribe treatments, or replace professional medical advice.";

    /// <summary>
    /// Sends a multi-turn conversation to GPT-4 and returns the assistant reply.
    /// </summary>
    /// <param name="userMessage">The user's latest message.</param>
    /// <param name="conversationJson">Optional JSON array of prior {role, content} turns.</param>
    public static string GetChatResponse(string userMessage, string conversationJson = null)
    {
        string apiKey = ConfigurationManager.AppSettings["OpenAIApiKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            return "The AI assistant is not configured yet. Please contact clinic staff for assistance.";

        try
        {
            var messages = new List<Dictionary<string, string>>();
            messages.Add(new Dictionary<string, string> { { "role", "system" }, { "content", ChatSystemPrompt } });

            if (!string.IsNullOrEmpty(conversationJson))
            {
                try
                {
                    var history = (ArrayList)Json.DeserializeObject(conversationJson);
                    foreach (Dictionary<string, object> turn in history)
                        messages.Add(new Dictionary<string, string>
                        {
                            { "role",    turn["role"].ToString() },
                            { "content", turn["content"].ToString() }
                        });
                }
                catch { /* ignore malformed history */ }
            }

            messages.Add(new Dictionary<string, string> { { "role", "user" }, { "content", userMessage } });

            var requestPayload = new Dictionary<string, object>
            {
                { "model",       "gpt-4" },
                { "temperature", 0.7 },
                { "max_tokens",  500 },
                { "messages",    messages }
            };

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type",  "application/json");
                client.Headers.Add("Authorization", "Bearer " + apiKey);

                string responseJson = client.UploadString(
                    "https://api.openai.com/v1/chat/completions", Json.Serialize(requestPayload));

                var outer   = (Dictionary<string, object>)Json.DeserializeObject(responseJson);
                var choices = (ArrayList)outer["choices"];
                var choice  = (Dictionary<string, object>)choices[0];
                var message = (Dictionary<string, object>)choice["message"];
                return message["content"].ToString().Trim();
            }
        }
        catch (WebException ex)
        {
            System.Diagnostics.Trace.TraceError("OpenAIService.GetChatResponse WebException: " + ex.Message);
            return "I'm sorry, I'm having trouble connecting right now. Please try again or contact us via the Contact Form.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("OpenAIService.GetChatResponse error: " + ex.Message);
            return "I'm sorry, something went wrong. Please try again later.";
        }
    }

    // -----------------------------------------------------------------------
    // 7. Wellness tips generator (Feature 1)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates personalised post-booking wellness tips based on the service the
    /// patient selected and any symptoms they described. Displayed on the
    /// AppointmentForm confirmation panel immediately after booking.
    /// </summary>
    /// <param name="service">The clinic service selected (e.g. "Cardiology").</param>
    /// <param name="issue">Brief description of the patient's reason for the appointment.</param>
    /// <returns>3-5 bullet-point wellness tips as a plain string.</returns>
    public static string GetWellnessTips(string service, string issue)
    {
        string prompt =
            "A patient at Portmore Medical Center has just booked a " + service + " appointment. " +
            "Their stated reason is: \"" + issue + "\"\n\n" +
            "Write 3-5 concise, actionable, and encouraging wellness tips tailored to their situation. " +
            "Format each tip on its own line starting with '• '. " +
            "Keep language simple, positive, and non-alarmist. Do not diagnose or prescribe — " +
            "these are general healthy-living suggestions to help the patient feel supported before their visit.";

        return CallGpt(prompt, temp: 0.7, maxTokens: 250)
               ?? "• Stay hydrated and get adequate rest before your appointment.\n" +
                  "• Write down any questions or symptoms to discuss with your doctor.\n" +
                  "• Avoid strenuous activity if you are experiencing pain or discomfort.";
    }

    // -----------------------------------------------------------------------
    // 8. Appointment no-show risk predictor (Feature 7)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Estimates the likelihood that a patient will miss their appointment based on the
    /// time slot and service type. The result is stored in the Appointments table
    /// so admin staff can proactively send reminders to high-risk patients.
    /// </summary>
    /// <param name="service">The service booked (e.g. "Radiology").</param>
    /// <param name="timeSlot">The time slot chosen (e.g. "8am to 9am").</param>
    /// <returns>One of: Low | Medium | High</returns>
    public static string GetNoShowRisk(string service, string timeSlot)
    {
        string prompt =
            "You are a healthcare operations analyst. Based on historical no-show patterns at medical clinics, " +
            "assess the no-show risk for the following appointment:\n\n" +
            "Service: " + service + "\n" +
            "Time Slot: " + timeSlot + "\n\n" +
            "Consider factors such as: early-morning or late-evening slots tend to have higher no-show rates; " +
            "specialist services (e.g. Radiology, Surgery) tend to have lower no-show rates than general services; " +
            "routine check-ups have higher no-show rates than urgent referrals.\n\n" +
            "Respond with ONLY valid JSON in exactly this format (no markdown):\n" +
            "{\"risk\":\"Medium\"}\n\n" +
            "Risk levels: Low = patient very likely to attend, Medium = some risk, High = significant no-show risk.";

        string content = CallGpt(prompt, temp: 0.2, maxTokens: 30);
        if (content == null) return "Medium";

        content = StripCodeFences(content);
        try
        {
            var inner = (Dictionary<string, object>)Json.DeserializeObject(content);
            string risk = inner["risk"].ToString();
            // Validate to one of the expected values
            if (risk == "Low" || risk == "Medium" || risk == "High") return risk;
            return "Medium";
        }
        catch
        {
            return "Medium";
        }
    }

    // -----------------------------------------------------------------------
    // 9. Sentiment analysis on contact messages (Feature 5)
    // -----------------------------------------------------------------------

    /// <summary>Result of a sentiment analysis call.</summary>
    public class SentimentResult
    {
        /// <summary>One of: Positive | Neutral | Distressed | Urgent</summary>
        public string Level { get; set; }
        /// <summary>One-line explanation of why this sentiment level was assigned.</summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// Analyses the emotional tone of a patient's contact message.
    /// Results are stored in the Contacts table so admin staff can prioritise
    /// distressed or urgent enquiries.
    /// </summary>
    /// <param name="message">The patient's raw contact message.</param>
    public static SentimentResult GetSentiment(string message)
    {
        string prompt =
            "Analyse the emotional tone of the following patient message sent to a medical center:\n\n" +
            "\"" + message + "\"\n\n" +
            "Respond with ONLY valid JSON (no markdown) in exactly this format:\n" +
            "{\"level\":\"Neutral\",\"reason\":\"One sentence explaining the sentiment.\"}\n\n" +
            "Sentiment levels:\n" +
            "  Positive  — patient is satisfied, grateful, or making a routine enquiry\n" +
            "  Neutral   — matter-of-fact, no strong emotion\n" +
            "  Distressed — patient sounds worried, anxious, or upset\n" +
            "  Urgent    — patient indicates a time-sensitive or potentially serious situation";

        string content = CallGpt(prompt, temp: 0.2, maxTokens: 100);
        if (content == null)
            return new SentimentResult { Level = "Neutral", Reason = "Sentiment analysis unavailable." };

        content = StripCodeFences(content);
        try
        {
            var inner = (Dictionary<string, object>)Json.DeserializeObject(content);
            return new SentimentResult
            {
                Level  = inner["level"].ToString(),
                Reason = inner["reason"].ToString()
            };
        }
        catch
        {
            return new SentimentResult { Level = "Neutral", Reason = "Could not parse sentiment." };
        }
    }

    // -----------------------------------------------------------------------
    // 10. AI follow-up email drafter (Feature 6)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a personalised post-appointment follow-up email for the admin team to review
    /// and send to the patient. Incorporates the patient's service and AI triage note
    /// to make the message contextually relevant.
    /// </summary>
    /// <param name="firstName">Patient's first name.</param>
    /// <param name="service">The service they were seen for.</param>
    /// <param name="aiNote">The AI pre-assessment note stored at booking time.</param>
    /// <returns>A draft follow-up email as plain text.</returns>
    public static string GetFollowUpEmail(string firstName, string service, string aiNote)
    {
        string prompt =
            "Draft a short, warm, professional follow-up email from Portmore Medical Center to a patient " +
            "named " + firstName + " who recently had a " + service + " appointment.\n\n" +
            "Context from their intake note: \"" + aiNote + "\"\n\n" +
            "The email should: thank them for their visit, encourage them to follow any advice given, " +
            "remind them that the clinic is available if they have further concerns, and invite them to " +
            "book a follow-up if needed. Sign off as 'The Portmore Medical Center Team'. " +
            "Keep it under 150 words. Output plain text only — no HTML tags.";

        return CallGpt(prompt, temp: 0.7, maxTokens: 250)
               ?? "Dear " + firstName + ",\n\nThank you for your recent visit to Portmore Medical Center. " +
                  "We hope your appointment was helpful. Please do not hesitate to contact us if you have " +
                  "any further questions or concerns.\n\nThe Portmore Medical Center Team";
    }

    // -----------------------------------------------------------------------
    // 11. Doctor's note simplifier (Feature 8)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Rewrites a clinical note in plain, patient-friendly English.
    /// Useful for generating patient-facing summaries from doctor dictation.
    /// </summary>
    /// <param name="clinicalNote">The raw clinical or doctor's note text.</param>
    /// <returns>A plain-English rewrite suitable for sharing with the patient.</returns>
    public static string SimplifyNote(string clinicalNote)
    {
        string prompt =
            "You are a medical communication specialist. Rewrite the following clinical note in simple, " +
            "clear, and friendly language suitable for the patient (not the doctor). " +
            "Avoid medical jargon. Keep the same meaning but make it easy to understand. " +
            "Output plain text only — no headings, no bullet points, no HTML.\n\n" +
            "Clinical note:\n\"" + clinicalNote + "\"";

        return CallGpt(prompt, temp: 0.5, maxTokens: 350)
               ?? "Your doctor's notes have been recorded. Please contact the clinic if you have any questions.";
    }

    // -----------------------------------------------------------------------
    // 12. Pre-appointment questionnaire analyser (Feature 1)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Analyses a patient's answers to the pre-appointment screening questionnaire
    /// and produces a concise clinical summary for the doctor. The summary is stored
    /// in the session and included in the full triage when the appointment is booked.
    /// </summary>
    /// <param name="service">Service the patient is booking.</param>
    /// <param name="duration">How long they have had symptoms (e.g. "3 days").</param>
    /// <param name="severity">Self-reported severity 1-10.</param>
    /// <param name="medications">Current medications or "None".</param>
    /// <param name="allergies">Known allergies or "None".</param>
    /// <param name="additionalInfo">Any other information the patient wanted to mention.</param>
    /// <returns>A 2-4 sentence clinical summary to pre-populate the appointment record.</returns>
    public static string GetQuestionnaireAnalysis(string service, string duration,
        string severity, string medications, string allergies, string additionalInfo)
    {
        string prompt =
            "A patient at Portmore Medical Center completed a pre-appointment questionnaire for a " +
            service + " appointment. Summarise their responses into a concise 2-4 sentence clinical " +
            "pre-screening note for the treating doctor.\n\n" +
            "Symptom duration: " + duration + "\n" +
            "Self-reported severity (1=mild, 10=severe): " + severity + "\n" +
            "Current medications: " + medications + "\n" +
            "Known allergies: " + allergies + "\n" +
            "Additional information: " + additionalInfo + "\n\n" +
            "Write a professional but concise pre-screening summary. Do not diagnose. " +
            "Output plain text only.";

        return CallGpt(prompt, temp: 0.3, maxTokens: 200)
               ?? "Patient completed pre-appointment questionnaire. Please review details at time of consultation.";
    }

    // -----------------------------------------------------------------------
    // 13. Appointment rescheduler (Feature 3)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a friendly rescheduling message for a patient who wishes to cancel
    /// their appointment. Encourages them to rebook promptly and suggests the
    /// best way to contact the clinic.
    /// </summary>
    /// <param name="firstName">Patient's first name.</param>
    /// <param name="service">The service that was booked.</param>
    /// <param name="cancelReason">Brief reason for cancellation (may be empty).</param>
    /// <returns>A short rescheduling message as plain text.</returns>
    public static string GetRescheduleMessage(string firstName, string service, string cancelReason)
    {
        string reasonPart = string.IsNullOrWhiteSpace(cancelReason)
            ? "" : " Their stated reason is: \"" + cancelReason + "\".";

        string prompt =
            "A patient named " + firstName + " at Portmore Medical Center needs to cancel their " +
            service + " appointment." + reasonPart + "\n\n" +
            "Write a short, understanding, and encouraging message to them that:\n" +
            "1. Acknowledges their cancellation without judgement\n" +
            "2. Emphasises the importance of not delaying their " + service + " care\n" +
            "3. Invites them to rebook at their earliest convenience via the Appointment Form or by calling the clinic\n" +
            "4. Is warm and supportive in tone\n\n" +
            "Sign off as 'The Portmore Medical Center Team'. Keep it under 120 words. Plain text only.";

        return CallGpt(prompt, temp: 0.7, maxTokens: 200)
               ?? "Dear " + firstName + ",\n\nWe understand that plans change. Please don't forget to rebook your " +
                  service + " appointment at your earliest convenience — your health is our priority.\n\n" +
                  "The Portmore Medical Center Team";
    }

    // -----------------------------------------------------------------------
    // 14. Language translation (Feature 4)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Translates a block of plain text into the specified target language.
    /// Used by TranslateHandler.ashx to power the multi-language toggle on key pages.
    /// </summary>
    public static string TranslateText(string text, string targetLanguage)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(targetLanguage))
            return text;

        string prompt =
            "Translate the following text into " + targetLanguage + ". " +
            "Return ONLY the translated text — no explanations, no quotes, no formatting.\n\n" +
            text;

        return CallGpt(prompt, temp: 0.2, maxTokens: 600) ?? text;
    }

    // -----------------------------------------------------------------------
    // 15. Smart Time Slot Recommender (new Feature 1)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Recommends the best available time slot for a patient based on the
    /// service they are booking and any urgency they have described.
    /// The suggestion is advisory only — the patient still picks from the dropdown.
    /// </summary>
    /// <param name="service">Clinic service selected (e.g. "Cardiology").</param>
    /// <param name="urgency">Brief urgency or symptom description (may be empty).</param>
    /// <returns>2-3 sentence recommendation as plain text.</returns>
    public static string GetTimeSlotRecommendation(string service, string urgency)
    {
        string prompt =
            "A patient at Portmore Medical Center is booking a " + service + " appointment." +
            (string.IsNullOrWhiteSpace(urgency) ? "" : " They describe their concern as: \"" + urgency + "\".") + "\n\n" +
            "Based on best practices for " + service + " appointments, recommend which time of day is " +
            "most suitable for them (from the options: 8am–9am, 9am–10am, 11am–12pm, 12pm–1pm, " +
            "1pm–2pm, 2pm–3pm, 3pm–4pm, 4pm–5pm) and briefly explain why in 2-3 friendly sentences. " +
            "Output plain text only.";

        return CallGpt(prompt, temp: 0.5, maxTokens: 150)
               ?? "Morning slots (8am–10am) are generally best for specialist appointments as doctors " +
                  "are freshest and less likely to be running behind. Consider an early slot if possible.";
    }

    // -----------------------------------------------------------------------
    // 16. Medication Interaction Checker (new Feature 2)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reviews a list of medications and flags any well-known interactions,
    /// including a clear disclaimer that this is educational information only
    /// and not a substitute for professional medical advice.
    /// </summary>
    /// <param name="medications">Comma-separated list of medication names.</param>
    /// <returns>Plain-text analysis with interaction notes and disclaimer.</returns>
    public static string CheckMedicationInteractions(string medications)
    {
        string prompt =
            "A patient at Portmore Medical Center has listed the following medications they are currently taking:\n\n" +
            medications + "\n\n" +
            "Review these medications and:\n" +
            "1. List any well-known interactions between them (if any).\n" +
            "2. Note any medications that commonly require monitoring or have important dietary restrictions.\n" +
            "3. Advise the patient to discuss all medications with their doctor at their appointment.\n\n" +
            "Begin your response with: 'IMPORTANT: This is general educational information only and does " +
            "not replace advice from your doctor or pharmacist.'\n\n" +
            "Use plain English, no jargon. Keep it concise but thorough. Plain text only.";

        return CallGpt(prompt, temp: 0.2, maxTokens: 400)
               ?? "IMPORTANT: This is general educational information only and does not replace advice " +
                  "from your doctor or pharmacist.\n\nWe were unable to analyse your medications at this time. " +
                  "Please bring a complete list to your appointment and discuss with your doctor.";
    }

    // -----------------------------------------------------------------------
    // 17. Symptom Diary Analyser (new Feature 3)
    // -----------------------------------------------------------------------

    /// <summary>Result of a symptom diary analysis.</summary>
    public class SymptomDiaryResult
    {
        /// <summary>2-4 sentence trend summary of the patient's symptom progression.</summary>
        public string Summary        { get; set; }
        /// <summary>Plain-English recommendation: monitor / see GP soon / seek urgent care.</summary>
        public string Recommendation { get; set; }
    }

    /// <summary>
    /// Analyses a patient's multi-day symptom log and returns a trend summary
    /// plus a recommendation on urgency of care.
    /// </summary>
    /// <param name="diaryText">Free-text symptom log (one entry per line, newest last).</param>
    public static SymptomDiaryResult AnalyseSymptomDiary(string diaryText)
    {
        string prompt =
            "A patient has kept a symptom diary. Analyse the progression and provide a plain-English " +
            "trend summary and a recommendation.\n\n" +
            "Diary entries (oldest first):\n" + diaryText + "\n\n" +
            "Respond with ONLY valid JSON (no markdown) in exactly this format:\n" +
            "{\"summary\":\"2-4 sentence trend analysis.\",\"recommendation\":\"One clear sentence advising monitor / see GP soon / seek urgent care.\"}";

        string content = CallGpt(prompt, temp: 0.3, maxTokens: 250);
        if (content == null)
            return new SymptomDiaryResult
            {
                Summary        = "Unable to analyse diary at this time.",
                Recommendation = "Please bring your diary to your next appointment."
            };

        content = StripCodeFences(content);
        try
        {
            var inner = (System.Collections.Generic.Dictionary<string, object>)
                        new JavaScriptSerializer().DeserializeObject(content);
            return new SymptomDiaryResult
            {
                Summary        = inner["summary"].ToString(),
                Recommendation = inner["recommendation"].ToString()
            };
        }
        catch
        {
            return new SymptomDiaryResult { Summary = content, Recommendation = "Please discuss with your doctor." };
        }
    }

    // -----------------------------------------------------------------------
    // 18. AI Readiness Checklist (new Feature 4)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a personalised "what to bring / what to do before your visit"
    /// checklist tailored to the specific clinic service the patient booked.
    /// Displayed on the appointment confirmation panel.
    /// </summary>
    /// <param name="service">The clinic service booked (e.g. "Radiology").</param>
    /// <returns>Bullet-point checklist as plain text (each item starts with "• ").</returns>
    public static string GetReadinessChecklist(string service)
    {
        string prompt =
            "A patient at Portmore Medical Center has just booked a " + service + " appointment. " +
            "Generate a concise, practical checklist of 4-6 things they should do or bring " +
            "to be fully prepared for their visit.\n\n" +
            "Format each item on its own line starting with '• '. " +
            "Examples: bring ID, bring insurance card, fast for X hours, wear comfortable clothing, " +
            "bring list of medications, arrive 10 minutes early. " +
            "Tailor the items specifically to a " + service + " appointment. Plain text only.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 200)
               ?? "• Bring a valid photo ID and any insurance documentation.\n" +
                  "• Arrive 10 minutes early to complete any paperwork.\n" +
                  "• Bring a list of your current medications and dosages.\n" +
                  "• Write down any questions you want to ask the doctor.\n" +
                  "• Wear comfortable, loose-fitting clothing.";
    }

    // -----------------------------------------------------------------------
    // 19. Patient Education Card (new Feature 5)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a short plain-English overview of the patient's booked service —
    /// what to expect, common procedures, and how to get the most from the appointment.
    /// Displayed on the confirmation panel alongside wellness tips.
    /// </summary>
    /// <param name="service">The clinic service booked (e.g. "Cardiology").</param>
    /// <returns>2-4 paragraph educational overview as plain text.</returns>
    public static string GetPatientEducationCard(string service)
    {
        string prompt =
            "Write a short, friendly, plain-English overview of what a patient can expect from " +
            "a " + service + " appointment at a medical center. Cover:\n" +
            "1. What the specialty focuses on.\n" +
            "2. What typically happens during the appointment (in simple terms).\n" +
            "3. One tip for getting the most out of the visit.\n\n" +
            "Keep it to 3-4 short paragraphs. Warm and reassuring tone. Plain text only — no headings, no bullet points.";

        return CallGpt(prompt, temp: 0.6, maxTokens: 300)
               ?? "Your appointment has been confirmed with our specialist team. They will conduct " +
                  "a thorough assessment and discuss any findings with you. Don't hesitate to ask " +
                  "questions — your health team is here to help you.";
    }

    // -----------------------------------------------------------------------
    // 20. Weekly Demand Forecast (new Feature 6 admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Given a data summary of recent appointment bookings, predicts which services
    /// and days are likely to be busiest in the coming week so admin can plan staffing.
    /// </summary>
    /// <param name="dataSummary">Plain-text summary of recent appointment counts by service.</param>
    /// <returns>3-5 sentence operational forecast as plain text.</returns>
    public static string GetWeeklyDemandForecast(string dataSummary)
    {
        string prompt =
            "You are a healthcare operations analyst for Portmore Medical Center. " +
            "Based on the following recent appointment booking data, predict demand for the coming week. " +
            "Identify which services are likely to be busiest, suggest optimal staffing focus areas, " +
            "and flag any patterns worth monitoring. Keep the forecast to 4-6 sentences.\n\n" +
            "Recent data:\n" + dataSummary;

        return CallGpt(prompt, temp: 0.5, maxTokens: 300)
               ?? "Demand forecast is unavailable at this time. Please check back later.";
    }

    // -----------------------------------------------------------------------
    // 21. AI Referral Letter Drafter (new Feature 7 admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a formal inter-department referral letter from one clinic service
    /// to another, ready for the admin team to print and sign.
    /// </summary>
    /// <param name="patientName">Full name of the patient.</param>
    /// <param name="referringDoctor">Name of the referring doctor.</param>
    /// <param name="fromDepartment">Department making the referral (e.g. "General Practitioner").</param>
    /// <param name="toDepartment">Department being referred to (e.g. "Cardiology").</param>
    /// <param name="reason">Clinical reason for the referral.</param>
    /// <returns>A formal referral letter as plain text.</returns>
    public static string GetReferralLetter(string patientName, string referringDoctor,
        string fromDepartment, string toDepartment, string reason)
    {
        string prompt =
            "Draft a formal medical referral letter from Portmore Medical Center.\n\n" +
            "From: Dr. " + referringDoctor + ", " + fromDepartment + " Department\n" +
            "To: " + toDepartment + " Department\n" +
            "Patient: " + patientName + "\n" +
            "Reason for referral: " + reason + "\n\n" +
            "The letter should: be formally structured, briefly summarise the clinical reason, " +
            "request appropriate assessment, and be professional in tone. " +
            "Include today's date (" + DateTime.Now.ToString("MMMM d, yyyy") + "). " +
            "Sign off as Dr. " + referringDoctor + ". Plain text only, no HTML.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 350)
               ?? "Dear " + toDepartment + " Team,\n\nI am writing to refer " + patientName +
                  " for assessment. " + reason + "\n\nKind regards,\nDr. " + referringDoctor +
                  "\n" + fromDepartment + " Department, Portmore Medical Center";
    }

    // -----------------------------------------------------------------------
    // 22. Complaint Escalation Handler (new Feature 8 admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a formal, empathetic response to a patient complaint or distressed
    /// contact message. Designed to de-escalate and reassure the patient while
    /// maintaining a professional clinic voice.
    /// </summary>
    /// <param name="firstName">Patient's first name.</param>
    /// <param name="complaint">The patient's original complaint or message.</param>
    /// <returns>A formal complaint response as plain text.</returns>
    public static string GetComplaintResponse(string firstName, string complaint)
    {
        string prompt =
            "A patient named " + firstName + " has submitted the following complaint or distressed message " +
            "to Portmore Medical Center:\n\n\"" + complaint + "\"\n\n" +
            "Draft a formal, empathetic, and professional response that:\n" +
            "1. Acknowledges the patient's concern without admitting fault\n" +
            "2. Apologises for any distress caused\n" +
            "3. Explains that their concern will be reviewed by the appropriate team\n" +
            "4. Provides a contact number placeholder ([phone]) for follow-up\n" +
            "5. Is warm but professional in tone\n\n" +
            "Sign off as 'Patient Experience Team, Portmore Medical Center'. " +
            "Keep under 150 words. Plain text only.";

        return CallGpt(prompt, temp: 0.5, maxTokens: 250)
               ?? "Dear " + firstName + ",\n\nThank you for bringing this to our attention. " +
                  "We are sorry to hear about your experience and take all feedback very seriously. " +
                  "A member of our patient experience team will be in contact with you shortly.\n\n" +
                  "Patient Experience Team, Portmore Medical Center";
    }

    // -----------------------------------------------------------------------
    // 23. Staff Bio Generator (new Feature 9 admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a polished professional biography for a new doctor or staff member
    /// based on brief bullet-point details provided by admin. Ready to publish on
    /// the staff profile page.
    /// </summary>
    /// <param name="name">Doctor's full name.</param>
    /// <param name="specialty">Their medical specialty.</param>
    /// <param name="qualifications">Degrees and certifications (comma-separated).</param>
    /// <param name="yearsExperience">Years of experience.</param>
    /// <param name="extraDetails">Any other details (research, interests, languages, etc.).</param>
    /// <returns>A 2-3 paragraph professional biography as plain text.</returns>
    public static string GetStaffBio(string name, string specialty,
        string qualifications, string yearsExperience, string extraDetails)
    {
        string prompt =
            "Write a polished, professional 2-3 paragraph biography for a doctor at Portmore Medical Center.\n\n" +
            "Name: Dr. " + name + "\n" +
            "Specialty: " + specialty + "\n" +
            "Qualifications: " + qualifications + "\n" +
            "Years of experience: " + yearsExperience + "\n" +
            "Additional details: " + (string.IsNullOrWhiteSpace(extraDetails) ? "None provided" : extraDetails) + "\n\n" +
            "Write in third person. Warm, professional, and engaging tone. " +
            "Suitable for publishing on a medical center website. Plain text only.";

        return CallGpt(prompt, temp: 0.6, maxTokens: 300)
               ?? "Dr. " + name + " is a specialist in " + specialty + " at Portmore Medical Center " +
                  "with " + yearsExperience + " years of experience. " +
                  "They hold the following qualifications: " + qualifications + ". " +
                  "Dr. " + name + " is committed to providing the highest quality patient care.";
    }

    // -----------------------------------------------------------------------
    // 25. Appointment Preparation Tip (Patient Dashboard)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a short, personalised preparation reminder for a patient
    /// viewing a specific upcoming appointment on their dashboard.
    /// </summary>
    /// <param name="service">The booked specialty (e.g. "Cardiology").</param>
    /// <param name="timeSlot">The booked time slot (e.g. "9:00 AM – 10:00 AM").</param>
    /// <returns>2–3 sentences of plain-text preparation advice.</returns>
    public static string GetAppointmentPreparationTip(string service, string timeSlot)
    {
        string prompt =
            "A patient at Portmore Medical Center has an upcoming appointment.\n\n" +
            "Service: " + service + "\n" +
            "Time slot: " + timeSlot + "\n\n" +
            "Write 2-3 short, friendly sentences of practical preparation advice specific to this " +
            "specialty and time of day (e.g. fasting, wearing comfortable clothing, arriving early, " +
            "bringing documents). Plain text only, no bullet points, no headers.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 120)
               ?? "Please arrive 10 minutes early for your " + service + " appointment. " +
                  "Bring a photo ID, any referral letters, and a list of your current medications. " +
                  "If you have any concerns before your visit, do not hesitate to call reception.";
    }

    // -----------------------------------------------------------------------
    // 24. Monthly Health Newsletter Generator (new Feature 10 admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a patient-facing monthly health newsletter based on the top
    /// services booked that month. Suitable for copying into an email campaign.
    /// </summary>
    /// <param name="month">The month and year (e.g. "February 2026").</param>
    /// <param name="topServicesData">Plain-text summary of top booked services with counts.</param>
    /// <returns>A complete newsletter as plain text.</returns>
    // -----------------------------------------------------------------------
    // 26. AI Diet & Lifestyle Planner (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a personalised diet, exercise, and lifestyle plan for a patient
    /// based on their upcoming specialty and optional health details.
    /// </summary>
    public static string GetLifestylePlan(string specialty, string age, string conditions)
    {
        string prompt =
            "You are a friendly health advisor at Portmore Medical Center.\n\n" +
            "A patient is booked for: " + specialty + "\n" +
            "Age: " + (string.IsNullOrWhiteSpace(age) ? "not specified" : age) + "\n" +
            "Other conditions or notes: " + (string.IsNullOrWhiteSpace(conditions) ? "none" : conditions) + "\n\n" +
            "Write a personalised wellness plan with three clearly labelled sections:\n" +
            "1. DIET RECOMMENDATIONS (4-5 practical tips specific to this specialty)\n" +
            "2. EXERCISE & ACTIVITY (3-4 appropriate suggestions)\n" +
            "3. LIFESTYLE TIPS (3-4 general wellbeing tips for this condition)\n\n" +
            "Keep it friendly, motivating, and under 300 words. Plain text only, no HTML. " +
            "End with a brief reminder to follow their doctor's advice above all.";

        return CallGpt(prompt, temp: 0.5, maxTokens: 500)
               ?? "1. DIET RECOMMENDATIONS\nFocus on a balanced diet rich in vegetables, lean protein, and whole grains. " +
                  "Limit salt, sugar, and processed foods. Stay well hydrated.\n\n" +
                  "2. EXERCISE & ACTIVITY\nAim for 30 minutes of moderate activity most days. " +
                  "Consult your doctor before starting any new exercise routine.\n\n" +
                  "3. LIFESTYLE TIPS\nPrioritise sleep, manage stress, avoid smoking, and limit alcohol. " +
                  "Always follow your doctor's personalised advice.";
    }

    // -----------------------------------------------------------------------
    // 27. AI Insurance & Cost Estimator (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provides a plain-English overview of what is typically covered for a service,
    /// questions to ask the insurer, and general cost guidance.
    /// </summary>
    public static string GetInsuranceGuide(string service, string insuranceType)
    {
        string prompt =
            "You are a helpful patient services advisor at Portmore Medical Center.\n\n" +
            "A patient is asking about insurance and costs for: " + service + "\n" +
            "Their insurance type: " + (string.IsNullOrWhiteSpace(insuranceType) ? "not specified" : insuranceType) + "\n\n" +
            "Provide a structured plain-text response with three sections:\n" +
            "1. WHAT IS TYPICALLY COVERED (general guidance for this specialty — 3-4 bullet points)\n" +
            "2. QUESTIONS TO ASK YOUR INSURER (5 practical questions the patient should ask)\n" +
            "3. COST GUIDANCE (general ranges and factors that affect cost — keep vague/general, no specific figures)\n\n" +
            "Always include this disclaimer at the end: " +
            "'This is general guidance only. Contact your insurance provider and our billing team for exact details.' " +
            "Plain text only. Under 280 words.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 450)
               ?? "1. WHAT IS TYPICALLY COVERED\nCoverage varies by provider and plan. " +
                  "Consultations, diagnostics, and treatments may each be covered differently.\n\n" +
                  "2. QUESTIONS TO ASK YOUR INSURER\n- Is this specialist covered under my plan?\n" +
                  "- What is my excess/co-pay for this service?\n- Do I need a referral?\n" +
                  "- Are diagnostic tests covered separately?\n- What is my annual benefit limit?\n\n" +
                  "3. COST GUIDANCE\nCosts depend on your plan, the complexity of your visit, and any tests required.\n\n" +
                  "This is general guidance only. Contact your insurance provider and our billing team for exact details.";
    }

    // -----------------------------------------------------------------------
    // 28. Doctor Focus Summary (Doctor Availability Viewer)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a short patient-friendly summary of a doctor's focus areas
    /// within their specialty, based on their name and specialty alone.
    /// </summary>
    public static string GetDoctorFocusSummary(string doctorName, string specialty)
    {
        string prompt =
            "Write a short 2-sentence patient-friendly description of what Dr. " + doctorName +
            " specialises in within " + specialty + " at Portmore Medical Center. " +
            "Make it warm, reassuring, and specific to the specialty. Plain text only.";

        return CallGpt(prompt, temp: 0.6, maxTokens: 100)
               ?? "Dr. " + doctorName + " is a specialist in " + specialty +
                  " at Portmore Medical Center, bringing expert care and a patient-centred approach to every consultation.";
    }

    // -----------------------------------------------------------------------
    // 29. Feedback Summary (Admin — Post-Appointment Feedback)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Analyses a batch of patient feedback submissions and produces an
    /// executive-style summary for the admin dashboard.
    /// </summary>
    public static string GetFeedbackSummary(string feedbackData)
    {
        string prompt =
            "You are an operations analyst for Portmore Medical Center.\n\n" +
            "Below is a summary of recent patient feedback submissions:\n\n" +
            feedbackData + "\n\n" +
            "Write a concise 4-6 sentence executive summary covering:\n" +
            "- Overall patient satisfaction trend\n" +
            "- Any recurring positive themes\n" +
            "- Any recurring concerns or areas for improvement\n" +
            "- A recommended action for management\n\n" +
            "Professional tone. Plain text only.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 300)
               ?? "Insufficient feedback data to generate a summary at this time. " +
                  "Please collect more patient responses before generating an analysis.";
    }

    // -----------------------------------------------------------------------
    // 24. Monthly Health Newsletter Generator (new Feature 10 admin)
    // -----------------------------------------------------------------------

    public static string GetMonthlyNewsletter(string month, string topServicesData)
    {
        string prompt =
            "Write a short, friendly monthly health newsletter for patients of Portmore Medical Center " +
            "for " + month + ".\n\n" +
            "Top booked services this month:\n" + topServicesData + "\n\n" +
            "The newsletter should:\n" +
            "1. Open with a warm greeting and the month/year\n" +
            "2. Highlight 2-3 health tips relevant to the most popular services this month\n" +
            "3. Include a reminder to book appointments early\n" +
            "4. Close warmly, signed from 'The Portmore Medical Center Team'\n\n" +
            "Keep it under 250 words. Friendly, supportive tone. Plain text only — no HTML.";

        return CallGpt(prompt, temp: 0.7, maxTokens: 400)
               ?? "Dear Portmore Medical Center Patients,\n\n" +
                  "Thank you for choosing us for your healthcare this " + month + ". " +
                  "We hope you are keeping well. Remember to book your appointments early " +
                  "to secure your preferred time slot.\n\n" +
                  "The Portmore Medical Center Team";
    }

    // -----------------------------------------------------------------------
    // 30. Emergency Symptom Triage (Patient)
    // -----------------------------------------------------------------------

    public static string GetEmergencyTriage(string symptoms)
    {
        string prompt =
            "You are an emergency triage assistant at Portmore Medical Center. " +
            "A patient has described the following symptoms:\n\n" + symptoms + "\n\n" +
            "Classify their situation into EXACTLY ONE of these four levels and explain why:\n" +
            "EMERGENCY — Call 999 or go to A&E immediately. Life-threatening signs present.\n" +
            "URGENT — Visit A&E or urgent care today. Serious but not immediately life-threatening.\n" +
            "APPOINTMENT — Book a GP or specialist appointment within the next few days.\n" +
            "SELF-CARE — Manageable at home. Provide 3-4 clear self-care steps.\n\n" +
            "Format your response:\n" +
            "Level: [EMERGENCY/URGENT/APPOINTMENT/SELF-CARE]\n" +
            "Reason: [2-3 sentences explaining the classification]\n" +
            "Action: [Clear next steps for the patient]\n\n" +
            "IMPORTANT: Always end with: 'If symptoms worsen, seek immediate medical attention.' " +
            "Plain text only. Do not diagnose.";

        return CallGpt(prompt, temp: 0.1, maxTokens: 300)
               ?? "Level: APPOINTMENT\nReason: Unable to assess symptoms automatically at this time.\n" +
                  "Action: Please book an appointment with your GP as soon as possible to discuss your symptoms.\n\n" +
                  "If symptoms worsen, seek immediate medical attention.";
    }

    // -----------------------------------------------------------------------
    // 31. Medical History Summariser (Patient)
    // -----------------------------------------------------------------------

    public static string GetMedicalHistorySummary(string conditions, string medications,
        string allergies, string surgeries, string familyHistory)
    {
        string prompt =
            "You are a clinical documentation assistant at Portmore Medical Center. " +
            "A patient has provided their medical history. Generate a concise, well-structured " +
            "clinical summary suitable for sharing with any healthcare provider.\n\n" +
            "Current conditions: " + (string.IsNullOrWhiteSpace(conditions) ? "None reported" : conditions) + "\n" +
            "Current medications: " + (string.IsNullOrWhiteSpace(medications) ? "None reported" : medications) + "\n" +
            "Known allergies: " + (string.IsNullOrWhiteSpace(allergies) ? "None reported" : allergies) + "\n" +
            "Past surgeries/procedures: " + (string.IsNullOrWhiteSpace(surgeries) ? "None reported" : surgeries) + "\n" +
            "Family history: " + (string.IsNullOrWhiteSpace(familyHistory) ? "None reported" : familyHistory) + "\n\n" +
            "Write a structured clinical summary with clear sections. " +
            "Professional tone. Plain text only. Under 250 words.";

        return CallGpt(prompt, temp: 0.2, maxTokens: 400)
               ?? "MEDICAL HISTORY SUMMARY\n\n" +
                  "Current Conditions: " + (string.IsNullOrWhiteSpace(conditions) ? "None reported" : conditions) + "\n" +
                  "Medications: " + (string.IsNullOrWhiteSpace(medications) ? "None reported" : medications) + "\n" +
                  "Allergies: " + (string.IsNullOrWhiteSpace(allergies) ? "None reported" : allergies) + "\n" +
                  "Surgical History: " + (string.IsNullOrWhiteSpace(surgeries) ? "None reported" : surgeries) + "\n" +
                  "Family History: " + (string.IsNullOrWhiteSpace(familyHistory) ? "None reported" : familyHistory);
    }

    // -----------------------------------------------------------------------
    // 32. Mental Health Check-In (Patient)
    // -----------------------------------------------------------------------

    public static string GetMentalHealthCheckIn(string moodScore, string sleepScore,
        string anxietyScore, string energyScore, string socialScore, string extraNotes)
    {
        string prompt =
            "You are a compassionate mental health support advisor at Portmore Medical Center.\n\n" +
            "A patient has completed a wellbeing check-in (scores are 1=very poor to 5=excellent):\n" +
            "Mood: " + moodScore + "/5\n" +
            "Sleep quality: " + sleepScore + "/5\n" +
            "Anxiety level (1=very anxious, 5=calm): " + anxietyScore + "/5\n" +
            "Energy levels: " + energyScore + "/5\n" +
            "Social connection: " + socialScore + "/5\n" +
            "Additional notes: " + (string.IsNullOrWhiteSpace(extraNotes) ? "None" : extraNotes) + "\n\n" +
            "Provide a warm, non-clinical response with:\n" +
            "1. WELLBEING SUMMARY: A brief, empathetic summary of their current state (2-3 sentences)\n" +
            "2. SUGGESTED SUPPORT: 3-4 practical, evidence-based suggestions tailored to their scores\n" +
            "3. WHEN TO SEEK HELP: Clear guidance on when to speak to a GP or counsellor\n\n" +
            "Warm, supportive tone. Never diagnose. Plain text only.";

        return CallGpt(prompt, temp: 0.5, maxTokens: 350)
               ?? "1. WELLBEING SUMMARY\nThank you for taking the time to check in on your wellbeing. " +
                  "Your responses have been noted.\n\n" +
                  "2. SUGGESTED SUPPORT\nPrioritise regular sleep, gentle exercise, and social connection. " +
                  "Consider mindfulness or breathing exercises for stress management.\n\n" +
                  "3. WHEN TO SEEK HELP\nIf you are feeling overwhelmed, persistently low, or anxious for " +
                  "more than two weeks, please speak to your GP.";
    }

    // -----------------------------------------------------------------------
    // 33. Health Goal Planner (Patient)
    // -----------------------------------------------------------------------

    public static string GetHealthGoalPlan(string goal, string activityLevel, string conditions)
    {
        string prompt =
            "You are a health coach at Portmore Medical Center.\n\n" +
            "Patient's health goal: " + goal + "\n" +
            "Current activity level: " + (string.IsNullOrWhiteSpace(activityLevel) ? "not specified" : activityLevel) + "\n" +
            "Existing conditions: " + (string.IsNullOrWhiteSpace(conditions) ? "none" : conditions) + "\n\n" +
            "Create a realistic 4-week action plan with these sections:\n" +
            "WEEK 1: [Foundation — easy starting habits]\n" +
            "WEEK 2: [Building — slightly increase intensity/consistency]\n" +
            "WEEK 3: [Progress — add a new habit or milestone]\n" +
            "WEEK 4: [Consolidate — review and plan for the future]\n" +
            "TIPS FOR SUCCESS: [3 motivational, practical tips]\n\n" +
            "Keep each week to 2-3 bullet points. Friendly, encouraging tone. " +
            "End with: 'Always consult your doctor before making significant changes to your health routine.' " +
            "Plain text only. Under 320 words.";

        return CallGpt(prompt, temp: 0.6, maxTokens: 500)
               ?? "WEEK 1: Start with 15-minute daily walks. Track your food intake. Sleep 7-8 hours.\n" +
                  "WEEK 2: Increase walks to 30 minutes. Add one portion of vegetables per meal.\n" +
                  "WEEK 3: Try a new form of exercise. Set a weekly check-in with yourself.\n" +
                  "WEEK 4: Review your progress. Plan how to maintain your new habits long-term.\n\n" +
                  "TIPS FOR SUCCESS: Set reminders. Find an accountability partner. Celebrate small wins.\n\n" +
                  "Always consult your doctor before making significant changes to your health routine.";
    }

    // -----------------------------------------------------------------------
    // 34. Second Opinion Question Generator (Patient)
    // -----------------------------------------------------------------------

    public static string GetSecondOpinionQuestions(string diagnosisText)
    {
        string prompt =
            "You are a patient advocate at Portmore Medical Center. " +
            "A patient has received the following diagnosis or treatment plan and wants to " +
            "be well-prepared for their next appointment or a second opinion:\n\n" +
            diagnosisText + "\n\n" +
            "Generate 8-10 intelligent, specific questions the patient should ask their doctor. " +
            "Group them into:\n" +
            "UNDERSTANDING THE DIAGNOSIS: (3-4 questions)\n" +
            "TREATMENT OPTIONS: (3-4 questions)\n" +
            "NEXT STEPS & LIFESTYLE: (2-3 questions)\n\n" +
            "Questions should be direct and patient-friendly. Plain text only.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 400)
               ?? "UNDERSTANDING THE DIAGNOSIS:\n" +
                  "- What exactly is my diagnosis and what does it mean for my health?\n" +
                  "- How confident are you in this diagnosis?\n" +
                  "- What caused this condition?\n\n" +
                  "TREATMENT OPTIONS:\n" +
                  "- What are all the treatment options available to me?\n" +
                  "- What are the risks and benefits of each option?\n" +
                  "- What happens if I choose not to treat it?\n\n" +
                  "NEXT STEPS & LIFESTYLE:\n" +
                  "- What lifestyle changes should I make?\n" +
                  "- When should I follow up with you?";
    }

    // -----------------------------------------------------------------------
    // 35. Recovery Tracker Analyser (Patient)
    // -----------------------------------------------------------------------

    public static string AnalyseRecovery(string recoveryEntries, string procedure)
    {
        string prompt =
            "You are a recovery support advisor at Portmore Medical Center.\n\n" +
            "A patient recovering from " + procedure + " has logged the following daily recovery notes:\n\n" +
            recoveryEntries + "\n\n" +
            "Provide a structured recovery analysis:\n" +
            "RECOVERY TREND: (2-3 sentences on whether recovery appears on track, improving, or concerning)\n" +
            "POSITIVE SIGNS: (any encouraging indicators)\n" +
            "AREAS TO WATCH: (anything that may need attention)\n" +
            "RECOMMENDATION: One of: 'Recovery appears on track', 'Consider contacting your care team', " +
            "or 'Please contact your doctor promptly'\n\n" +
            "Supportive, non-alarming tone unless genuinely concerning. Plain text only. No diagnosis.";

        return CallGpt(prompt, temp: 0.3, maxTokens: 300)
               ?? "RECOVERY TREND: Unable to analyse entries at this time.\n\n" +
                  "RECOMMENDATION: If you have any concerns about your recovery, " +
                  "please contact your care team or call the clinic directly.";
    }

    // -----------------------------------------------------------------------
    // 36. Staff Performance Report (Admin)
    // -----------------------------------------------------------------------

    public static string GetStaffPerformanceReport(string appointmentData, string feedbackData)
    {
        string prompt =
            "You are an operations analyst for Portmore Medical Center.\n\n" +
            "APPOINTMENT DATA:\n" + appointmentData + "\n\n" +
            "PATIENT FEEDBACK DATA:\n" + feedbackData + "\n\n" +
            "Write a professional staff performance report covering:\n" +
            "1. BUSIEST DEPARTMENTS: Which services have the highest demand\n" +
            "2. PATIENT SATISFACTION: Overall trends from feedback ratings and sentiment\n" +
            "3. OPERATIONAL CONCERNS: Any triage escalations, high no-show rates, or distressed feedback\n" +
            "4. RECOMMENDATIONS: 3 specific, actionable recommendations for management\n\n" +
            "Professional report tone. Under 300 words. Plain text only.";

        return CallGpt(prompt, temp: 0.3, maxTokens: 500)
               ?? "Insufficient data to generate a staff performance report at this time. " +
                  "Please ensure appointment and feedback records are available.";
    }

    // -----------------------------------------------------------------------
    // 37. Social Media Post Generator (Admin)
    // -----------------------------------------------------------------------

    public static string GetSocialMediaPost(string topic, string platform)
    {
        string prompt =
            "You are the social media manager for Portmore Medical Center.\n\n" +
            "Write a " + platform + " post about: " + topic + "\n\n" +
            "The post should:\n" +
            "- Be appropriate for " + platform + " (length, tone, hashtag usage)\n" +
            "- Promote health awareness in a friendly, accessible way\n" +
            "- Include a call-to-action (e.g. 'Book an appointment at Portmore Medical Center')\n" +
            "- Include 3-5 relevant hashtags at the end\n" +
            "- Sound warm and professional, not clinical\n\n" +
            "Plain text only. Write the post directly without any preamble.";

        return CallGpt(prompt, temp: 0.7, maxTokens: 200)
               ?? "At Portmore Medical Center, we care about your health every day. " +
                  "Book your appointment today and let our expert team support your wellbeing journey. " +
                  "#PortmoreMedical #HealthFirst #BookNow #PatientCare #Wellness";
    }

    // -----------------------------------------------------------------------
    // 38. Clinical Audit Report (Admin)
    // -----------------------------------------------------------------------

    public static string GetClinicalAuditReport(string auditData)
    {
        string prompt =
            "You are a clinical governance analyst for Portmore Medical Center.\n\n" +
            "The following data has been compiled for a clinical audit:\n\n" +
            auditData + "\n\n" +
            "Write a formal clinical audit report with these sections:\n" +
            "EXECUTIVE SUMMARY: (3-4 sentences overview)\n" +
            "KEY FINDINGS: (4-6 bullet points of notable findings)\n" +
            "AREAS OF CONCERN: (any triage escalations, high no-show rates, negative feedback trends)\n" +
            "RECOMMENDATIONS: (3-5 specific, actionable improvement recommendations)\n" +
            "CONCLUSION: (2-3 closing sentences)\n\n" +
            "Formal, professional clinical governance tone. Plain text only.";

        return CallGpt(prompt, temp: 0.2, maxTokens: 600)
               ?? "Insufficient data to generate a clinical audit report. " +
                  "Please ensure appointment, triage, and feedback data are populated before running this report.";
    }

    // -----------------------------------------------------------------------
    // 39. Staff Training Topic Recommender (Admin)
    // -----------------------------------------------------------------------

    public static string GetTrainingRecommendations(string complaintThemes, string feedbackThemes)
    {
        string prompt =
            "You are a staff development advisor for Portmore Medical Center.\n\n" +
            "Recurring complaint themes from patients: " + complaintThemes + "\n" +
            "Recurring feedback themes: " + feedbackThemes + "\n\n" +
            "Based on these themes, recommend the top 5 staff training priorities. " +
            "For each recommendation:\n" +
            "- Name the training topic\n" +
            "- Explain why it is needed (1-2 sentences referencing the data)\n" +
            "- Suggest a format (e.g. workshop, e-learning, role play)\n\n" +
            "Professional, constructive tone. Plain text only.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 400)
               ?? "1. Patient Communication Skills — Improve clarity and empathy in patient interactions. Format: Workshop.\n" +
                  "2. Appointment Management — Reduce no-shows through better reminder processes. Format: E-learning.\n" +
                  "3. Complaint Handling — Equip staff to de-escalate distressed patients. Format: Role play.\n" +
                  "4. Clinical Documentation — Ensure accurate and timely record-keeping. Format: E-learning.\n" +
                  "5. Triage Awareness — Ensure front-desk staff can recognise urgent presentations. Format: Workshop.";
    }

    // -----------------------------------------------------------------------
    // 40. Meeting Agenda Generator (Admin)
    // -----------------------------------------------------------------------

    public static string GetMeetingAgenda(string meetingTitle, string attendees,
        string topics, string meetingDate)
    {
        string prompt =
            "You are an executive assistant at Portmore Medical Center.\n\n" +
            "Meeting title: " + meetingTitle + "\n" +
            "Date: " + meetingDate + "\n" +
            "Attendees: " + attendees + "\n" +
            "Discussion topics: " + topics + "\n\n" +
            "Generate a professional, structured meeting agenda with:\n" +
            "- Header (title, date, attendees)\n" +
            "- Welcome & Apologies (5 min)\n" +
            "- One agenda item per topic with a suggested time allocation\n" +
            "- Any Other Business (5 min)\n" +
            "- Next Steps & Close\n\n" +
            "Professional, concise format. Plain text only.";

        return CallGpt(prompt, temp: 0.3, maxTokens: 400)
               ?? "MEETING AGENDA\n" +
                  "Title: " + meetingTitle + "\n" +
                  "Date: " + meetingDate + "\n" +
                  "Attendees: " + attendees + "\n\n" +
                  "1. Welcome & Apologies (5 min)\n" +
                  "2. Discussion Items: " + topics + "\n" +
                  "3. Any Other Business (5 min)\n" +
                  "4. Next Steps & Close";
    }

    // -----------------------------------------------------------------------
    // 41. Appointment Confirmation Email Preview (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a warm, personalised appointment confirmation email that the patient
    /// can copy and send to themselves or a carer. Includes preparation reminders
    /// tailored to the specific service booked.
    /// </summary>
    /// <param name="patientName">Full name of the patient.</param>
    /// <param name="service">Clinic service booked (e.g. "Cardiology").</param>
    /// <param name="timeSlot">Selected appointment time slot.</param>
    /// <param name="appointmentDate">Date of the appointment as a string.</param>
    /// <returns>Full plain-text email draft ready to copy.</returns>
    public static string GetConfirmationEmailDraft(string patientName, string service,
        string timeSlot, string appointmentDate)
    {
        string prompt =
            "You are a patient services coordinator at Portmore Medical Center.\n\n" +
            "Draft a friendly appointment confirmation email for the following booking:\n" +
            "Patient name: " + patientName + "\n" +
            "Service: " + service + "\n" +
            "Date: " + appointmentDate + "\n" +
            "Time slot: " + timeSlot + "\n\n" +
            "The email should include:\n" +
            "1. A warm greeting addressing the patient by name\n" +
            "2. Confirmation of the appointment details (service, date, time)\n" +
            "3. 3-4 practical preparation tips specific to " + service + "\n" +
            "4. Contact details reminder ('Call us if you need to reschedule')\n" +
            "5. A warm sign-off from 'The Portmore Medical Center Team'\n\n" +
            "Friendly, professional tone. Plain text only. No HTML.";

        return CallGpt(prompt, temp: 0.5, maxTokens: 400)
               ?? "Dear " + patientName + ",\n\n" +
                  "Your appointment at Portmore Medical Center has been confirmed.\n\n" +
                  "Service: " + service + "\n" +
                  "Date: " + appointmentDate + "\n" +
                  "Time: " + timeSlot + "\n\n" +
                  "Please arrive 10 minutes early and bring a valid photo ID, your insurance card, " +
                  "and a list of any current medications.\n\n" +
                  "If you need to reschedule, please contact us as soon as possible.\n\n" +
                  "We look forward to seeing you.\n\nThe Portmore Medical Center Team";
    }

    // -----------------------------------------------------------------------
    // 42. AI Allergy & Food Safety Guide (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a personalised food safety guide for a patient based on their
    /// listed allergies. Covers safe and unsafe foods, label-reading tips, and
    /// eating-out advice. Includes a medical disclaimer.
    /// </summary>
    /// <param name="allergies">Comma-separated list of patient allergies.</param>
    /// <returns>Structured plain-text allergy and food safety guide.</returns>
    public static string GetAllergyFoodGuide(string allergies)
    {
        string prompt =
            "You are a dietitian at Portmore Medical Center.\n\n" +
            "A patient has the following allergies: " + allergies + "\n\n" +
            "Create a personalised food safety guide with these sections:\n" +
            "FOODS TO AVOID: (list the key foods and hidden sources for each allergy)\n" +
            "SAFE FOOD SWAPS: (practical alternatives for common foods they must avoid)\n" +
            "LABEL READING TIPS: (3-4 tips for spotting allergens on food labels)\n" +
            "EATING OUT SAFELY: (4 practical tips for dining out with these allergies)\n" +
            "IMPORTANT REMINDER: End with: 'Always carry any prescribed emergency medication " +
            "(such as an EpiPen) and consult your doctor or dietitian for personalised advice.'\n\n" +
            "Friendly, clear language. No jargon. Plain text only. Under 350 words.";

        return CallGpt(prompt, temp: 0.3, maxTokens: 550)
               ?? "FOODS TO AVOID\nPlease discuss specific foods to avoid with your dietitian or GP " +
                  "based on your allergy diagnosis.\n\n" +
                  "LABEL READING TIPS\nAlways check the ingredients list. Look for 'Contains' and 'May contain' " +
                  "allergen warnings. When in doubt, contact the manufacturer.\n\n" +
                  "EATING OUT SAFELY\nAlways inform restaurant staff of your allergies when ordering. " +
                  "Ask about cross-contamination risks in the kitchen.\n\n" +
                  "IMPORTANT REMINDER\nAlways carry any prescribed emergency medication " +
                  "(such as an EpiPen) and consult your doctor or dietitian for personalised advice.";
    }

    // -----------------------------------------------------------------------
    // 43. Pre-Surgery Anxiety Support (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provides compassionate, evidence-based pre-operative information and anxiety
    /// management techniques for a patient awaiting a surgical or procedural appointment.
    /// Always signposts the clinical team for medical questions.
    /// </summary>
    /// <param name="procedureType">The surgery or procedure the patient is facing.</param>
    /// <param name="concernText">Optional free-text description of the patient's concerns.</param>
    /// <returns>Supportive plain-text pre-surgery guide.</returns>
    public static string GetPreSurgerySupport(string procedureType, string concernText)
    {
        string prompt =
            "You are a compassionate pre-operative support nurse at Portmore Medical Center.\n\n" +
            "A patient is preparing for: " + procedureType + "\n" +
            (string.IsNullOrWhiteSpace(concernText) ? "" : "Their main concerns: " + concernText + "\n") + "\n" +
            "Provide a warm, reassuring pre-surgery support guide with these sections:\n" +
            "WHAT TO EXPECT: (brief, reassuring overview of a typical " + procedureType + " experience)\n" +
            "MANAGING ANXIETY: (4-5 evidence-based techniques: breathing, visualisation, etc.)\n" +
            "PRACTICAL PREPARATION: (4-5 tips for the days before surgery)\n" +
            "ON THE DAY: (3-4 things to expect when you arrive)\n" +
            "REMEMBER: End with 'Your clinical team is here to support you every step of the way. " +
            "Please ask them any medical questions you have.'\n\n" +
            "Warm, reassuring, empathetic tone. Never minimise concerns. Plain text only. Under 350 words.";

        return CallGpt(prompt, temp: 0.5, maxTokens: 550)
               ?? "WHAT TO EXPECT\nYour surgical team will ensure you are comfortable and informed " +
                  "throughout your procedure.\n\n" +
                  "MANAGING ANXIETY\nTry slow, deep breathing (inhale 4 seconds, hold 4, exhale 6). " +
                  "Focus on the positive outcome. Talk to someone you trust about your feelings.\n\n" +
                  "PRACTICAL PREPARATION\nFollow all pre-operative instructions given by your surgeon. " +
                  "Arrange someone to accompany you. Get a good night's rest beforehand.\n\n" +
                  "ON THE DAY\nArrive at your scheduled time. The team will introduce themselves and " +
                  "answer your questions before you go in.\n\n" +
                  "REMEMBER\nYour clinical team is here to support you every step of the way. " +
                  "Please ask them any medical questions you have.";
    }

    // -----------------------------------------------------------------------
    // 44. Specialist Comparison Tool (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Compares two medical specialties in plain English to help a patient understand
    /// which specialist is most appropriate for their situation.
    /// </summary>
    /// <param name="specialty1">First specialty to compare (e.g. "Cardiology").</param>
    /// <param name="specialty2">Second specialty to compare (e.g. "General Practitioner").</param>
    /// <returns>Structured plain-text comparison of both specialties.</returns>
    public static string GetSpecialistComparison(string specialty1, string specialty2)
    {
        string prompt =
            "You are a patient services advisor at Portmore Medical Center.\n\n" +
            "A patient wants to understand the difference between seeing a " +
            specialty1 + " and a " + specialty2 + ".\n\n" +
            "Write a clear, patient-friendly comparison with these sections:\n" +
            specialty1.ToUpper() + " — WHAT THEY DO: (2-3 sentences)\n" +
            specialty1.ToUpper() + " — WHEN TO SEE THEM: (2-3 examples)\n\n" +
            specialty2.ToUpper() + " — WHAT THEY DO: (2-3 sentences)\n" +
            specialty2.ToUpper() + " — WHEN TO SEE THEM: (2-3 examples)\n\n" +
            "KEY DIFFERENCE: (1-2 sentences summarising the core distinction)\n" +
            "RECOMMENDATION: Brief, neutral guidance on how to decide.\n\n" +
            "Friendly, jargon-free language. Plain text only. Under 280 words.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 450)
               ?? specialty1.ToUpper() + " — WHAT THEY DO\nSpecialists in " + specialty1 +
                  " focus on specific conditions or body systems within their field.\n\n" +
                  specialty2.ToUpper() + " — WHAT THEY DO\nSpecialists in " + specialty2 +
                  " focus on specific conditions or body systems within their field.\n\n" +
                  "KEY DIFFERENCE\nThe right specialist depends on your specific symptoms and GP referral.\n\n" +
                  "RECOMMENDATION\nSpeak to your GP who can refer you to the most appropriate specialist " +
                  "based on your individual symptoms and medical history.";
    }

    // -----------------------------------------------------------------------
    // 45. Health Age Calculator (Patient)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Estimates a patient's "health age" versus their actual age based on lifestyle
    /// and health indicators. Provides personalised improvement tips. Includes
    /// a disclaimer that this is educational, not a clinical assessment.
    /// </summary>
    /// <param name="actualAge">Patient's actual chronological age.</param>
    /// <param name="smokingStatus">Smoking status (e.g. "Non-smoker", "Smoker", "Ex-smoker").</param>
    /// <param name="exerciseFrequency">Exercise frequency (e.g. "Daily", "2-3x/week", "Rarely").</param>
    /// <param name="dietQuality">Self-rated diet quality (e.g. "Excellent", "Good", "Poor").</param>
    /// <param name="sleepHours">Average hours of sleep per night.</param>
    /// <param name="stressLevel">Self-rated stress level (e.g. "Low", "Moderate", "High").</param>
    /// <returns>Health age estimate with plain-text lifestyle improvement recommendations.</returns>
    public static string GetHealthAge(string actualAge, string smokingStatus,
        string exerciseFrequency, string dietQuality, string sleepHours, string stressLevel)
    {
        string prompt =
            "You are a preventive health advisor at Portmore Medical Center.\n\n" +
            "A patient has provided the following lifestyle data:\n" +
            "Actual age: " + actualAge + "\n" +
            "Smoking status: " + smokingStatus + "\n" +
            "Exercise frequency: " + exerciseFrequency + "\n" +
            "Diet quality (self-rated): " + dietQuality + "\n" +
            "Average sleep per night: " + sleepHours + " hours\n" +
            "Stress level: " + stressLevel + "\n\n" +
            "Based on these lifestyle factors, provide:\n" +
            "HEALTH AGE ESTIMATE: Give an estimated 'health age' (may be higher or lower than actual age) " +
            "with a brief explanation of which factors influenced it most.\n" +
            "TOP 3 IMPROVEMENTS: The three lifestyle changes that would have the biggest positive impact.\n" +
            "POSITIVE HABITS: Acknowledge any good habits already in place.\n" +
            "DISCLAIMER: End with: 'This is a general wellness estimate for educational purposes only " +
            "and is not a medical assessment. Please speak to your GP for personalised health advice.'\n\n" +
            "Encouraging, non-judgmental tone. Plain text only. Under 280 words.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 400)
               ?? "HEALTH AGE ESTIMATE\nBased on your lifestyle data, we were unable to calculate " +
                  "your health age at this time.\n\n" +
                  "TOP 3 IMPROVEMENTS\n1. Regular exercise (aim for 150 min/week)\n" +
                  "2. Balanced diet with plenty of fruit and vegetables\n" +
                  "3. 7-9 hours quality sleep per night\n\n" +
                  "DISCLAIMER\nThis is a general wellness estimate for educational purposes only " +
                  "and is not a medical assessment. Please speak to your GP for personalised health advice.";
    }

    // -----------------------------------------------------------------------
    // 46. Patient Discharge Summary Generator (Admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a formal patient discharge summary document based on the clinical
    /// details provided by the administering clinician. Suitable for filing or sharing
    /// with the patient's GP or follow-up care team.
    /// </summary>
    /// <param name="patientName">Full name of the patient.</param>
    /// <param name="diagnosis">Primary diagnosis or diagnoses.</param>
    /// <param name="treatment">Treatments or interventions carried out.</param>
    /// <param name="medications">Discharge medications and dosages.</param>
    /// <param name="followUp">Follow-up instructions or referrals.</param>
    /// <returns>Formal plain-text discharge summary document.</returns>
    public static string GetDischargeSummary(string patientName, string diagnosis,
        string treatment, string medications, string followUp)
    {
        string prompt =
            "You are a clinical documentation specialist at Portmore Medical Center.\n\n" +
            "Generate a formal patient discharge summary for:\n" +
            "Patient: " + patientName + "\n" +
            "Diagnosis: " + diagnosis + "\n" +
            "Treatment provided: " + treatment + "\n" +
            "Discharge medications: " + (string.IsNullOrWhiteSpace(medications) ? "None prescribed" : medications) + "\n" +
            "Follow-up instructions: " + (string.IsNullOrWhiteSpace(followUp) ? "None specified" : followUp) + "\n\n" +
            "Format the summary with these sections:\n" +
            "PATIENT DISCHARGE SUMMARY\n" +
            "Date: [today's date]\n" +
            "Patient: [name]\n" +
            "DIAGNOSIS\n" +
            "TREATMENT PROVIDED\n" +
            "DISCHARGE MEDICATIONS\n" +
            "FOLLOW-UP INSTRUCTIONS\n" +
            "ADDITIONAL NOTES (any relevant clinical observations)\n" +
            "Prepared by: Portmore Medical Center Clinical Team\n\n" +
            "Formal clinical documentation tone. Plain text only.";

        return CallGpt(prompt, temp: 0.2, maxTokens: 500)
               ?? "PATIENT DISCHARGE SUMMARY\n\n" +
                  "Patient: " + patientName + "\n\n" +
                  "DIAGNOSIS\n" + diagnosis + "\n\n" +
                  "TREATMENT PROVIDED\n" + treatment + "\n\n" +
                  "DISCHARGE MEDICATIONS\n" + (string.IsNullOrWhiteSpace(medications) ? "None prescribed" : medications) + "\n\n" +
                  "FOLLOW-UP INSTRUCTIONS\n" + (string.IsNullOrWhiteSpace(followUp) ? "Please contact your GP for any concerns." : followUp) + "\n\n" +
                  "Prepared by: Portmore Medical Center Clinical Team";
    }

    // -----------------------------------------------------------------------
    // 47. AI Press Release Generator (Admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a professional press release for Portmore Medical Center based on
    /// a news topic or announcement provided by admin staff.
    /// </summary>
    /// <param name="topic">The news topic or announcement (e.g. "New MRI scanner installed").</param>
    /// <param name="keyPoints">Comma-separated key points to include in the release.</param>
    /// <returns>Full plain-text press release ready for distribution.</returns>
    public static string GetPressRelease(string topic, string keyPoints)
    {
        string prompt =
            "You are a communications manager for Portmore Medical Center.\n\n" +
            "Write a professional press release about: " + topic + "\n" +
            "Key points to include: " + (string.IsNullOrWhiteSpace(keyPoints) ? "Not specified" : keyPoints) + "\n\n" +
            "The press release should include:\n" +
            "FOR IMMEDIATE RELEASE header\n" +
            "A compelling headline\n" +
            "Dateline: Portmore, Jamaica — [date]\n" +
            "Opening paragraph (the who, what, when, where, why)\n" +
            "2-3 body paragraphs with supporting detail\n" +
            "A quote attributed to 'Dr. James Clarke, Medical Director, Portmore Medical Center'\n" +
            "A brief boilerplate paragraph about Portmore Medical Center\n" +
            "Contact details: media@portmoremedical.com | +1 (876) 555-0100\n\n" +
            "Professional journalism style. Plain text only. Under 400 words.";

        return CallGpt(prompt, temp: 0.6, maxTokens: 600)
               ?? "FOR IMMEDIATE RELEASE\n\n" +
                  "PORTMORE MEDICAL CENTER ANNOUNCES: " + topic.ToUpper() + "\n\n" +
                  "Portmore, Jamaica — Portmore Medical Center is pleased to announce " + topic + ". " +
                  "This development reflects our ongoing commitment to delivering exceptional healthcare " +
                  "to the communities we serve.\n\n" +
                  "\"We are proud to continue investing in our patients and services,\" said Dr. James Clarke, " +
                  "Medical Director, Portmore Medical Center.\n\n" +
                  "About Portmore Medical Center: Portmore Medical Center is a leading private healthcare " +
                  "provider offering specialist and general medical services to patients across Jamaica.\n\n" +
                  "Contact: media@portmoremedical.com | +1 (876) 555-0100";
    }

    // -----------------------------------------------------------------------
    // 48. Job Description Generator (Admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a full professional job description for a clinical or administrative
    /// role at Portmore Medical Center, formatted in an NHS/healthcare style.
    /// </summary>
    /// <param name="roleTitle">Job title (e.g. "Senior Practice Nurse").</param>
    /// <param name="department">Department or specialty.</param>
    /// <param name="requirements">Key skills or experience requirements (comma-separated).</param>
    /// <returns>Full plain-text job description.</returns>
    public static string GetJobDescription(string roleTitle, string department, string requirements)
    {
        string prompt =
            "You are an HR manager at Portmore Medical Center.\n\n" +
            "Write a professional job description for:\n" +
            "Role: " + roleTitle + "\n" +
            "Department: " + department + "\n" +
            "Key requirements: " + (string.IsNullOrWhiteSpace(requirements) ? "Standard clinical competency" : requirements) + "\n\n" +
            "Include these sections:\n" +
            "JOB TITLE\n" +
            "DEPARTMENT\n" +
            "REPORTS TO: (suggest an appropriate reporting line)\n" +
            "JOB SUMMARY: (3-4 sentence overview)\n" +
            "KEY RESPONSIBILITIES: (6-8 bullet points)\n" +
            "ESSENTIAL REQUIREMENTS: (5-6 bullet points of must-have qualifications/skills)\n" +
            "DESIRABLE REQUIREMENTS: (3-4 nice-to-have skills)\n" +
            "WHAT WE OFFER: (3-4 benefits bullets)\n" +
            "HOW TO APPLY: 'Please submit your CV and cover letter to careers@portmoremedical.com'\n\n" +
            "Professional HR tone suitable for a Caribbean private healthcare provider. Plain text only.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 600)
               ?? "JOB TITLE: " + roleTitle + "\n" +
                  "DEPARTMENT: " + department + "\n\n" +
                  "JOB SUMMARY\nPortmore Medical Center is seeking an experienced " + roleTitle +
                  " to join our dedicated clinical team.\n\n" +
                  "KEY RESPONSIBILITIES\n- Deliver high-quality patient care\n" +
                  "- Collaborate with multidisciplinary teams\n" +
                  "- Maintain accurate clinical records\n\n" +
                  "ESSENTIAL REQUIREMENTS\n- Relevant professional qualification\n" +
                  "- Current registration with applicable regulatory body\n" +
                  "- " + (string.IsNullOrWhiteSpace(requirements) ? "Relevant clinical experience" : requirements) + "\n\n" +
                  "HOW TO APPLY\nPlease submit your CV and cover letter to careers@portmoremedical.com";
    }

    // -----------------------------------------------------------------------
    // 49. Clinical Incident Report Writer (Admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Converts a free-text description of a clinical incident into a formal,
    /// structured incident report document suitable for governance review.
    /// </summary>
    /// <param name="incidentDescription">Free-text description of what occurred.</param>
    /// <param name="incidentDate">Date the incident occurred.</param>
    /// <param name="location">Location within the clinic where the incident occurred.</param>
    /// <returns>Formal plain-text clinical incident report.</returns>
    public static string GetIncidentReport(string incidentDescription, string incidentDate, string location)
    {
        string prompt =
            "You are a clinical governance officer at Portmore Medical Center.\n\n" +
            "Convert the following incident description into a formal clinical incident report:\n\n" +
            "Incident description: " + incidentDescription + "\n" +
            "Date of incident: " + (string.IsNullOrWhiteSpace(incidentDate) ? "Not specified" : incidentDate) + "\n" +
            "Location: " + (string.IsNullOrWhiteSpace(location) ? "Not specified" : location) + "\n\n" +
            "Format as a formal clinical incident report with these sections:\n" +
            "CLINICAL INCIDENT REPORT\n" +
            "Reference: PMC-IR-[generate a plausible reference number]\n" +
            "Date of Incident:\n" +
            "Location:\n" +
            "INCIDENT DESCRIPTION: (formalise the description in professional clinical language)\n" +
            "IMMEDIATE ACTIONS TAKEN: (suggest what appropriate immediate actions would be)\n" +
            "CONTRIBUTING FACTORS: (identify any likely contributing factors)\n" +
            "RISK LEVEL: (Low / Medium / High — with brief justification)\n" +
            "RECOMMENDATIONS: (3-4 recommendations to prevent recurrence)\n" +
            "Report prepared by: Portmore Medical Center Governance Team\n\n" +
            "Formal, objective, professional tone. Plain text only.";

        return CallGpt(prompt, temp: 0.2, maxTokens: 500)
               ?? "CLINICAL INCIDENT REPORT\n\n" +
                  "Date of Incident: " + (string.IsNullOrWhiteSpace(incidentDate) ? "Not specified" : incidentDate) + "\n" +
                  "Location: " + (string.IsNullOrWhiteSpace(location) ? "Not specified" : location) + "\n\n" +
                  "INCIDENT DESCRIPTION\n" + incidentDescription + "\n\n" +
                  "IMMEDIATE ACTIONS TAKEN\nImmediate actions were taken in accordance with clinical protocol.\n\n" +
                  "RECOMMENDATIONS\nThis incident has been logged for governance review.\n\n" +
                  "Report prepared by: Portmore Medical Center Governance Team";
    }

    // -----------------------------------------------------------------------
    // 50. Patient DNA (Did Not Attend) Letter Generator (Admin)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a professional, compassionate Did Not Attend (DNA) letter for a patient
    /// who missed their appointment without prior cancellation. The letter encourages
    /// rebooking while following up on patient wellbeing.
    /// </summary>
    /// <param name="patientName">Full name of the patient.</param>
    /// <param name="service">The service the patient missed.</param>
    /// <param name="appointmentDate">Date of the missed appointment.</param>
    /// <param name="timeSlot">Time slot of the missed appointment.</param>
    /// <returns>Formal plain-text DNA letter ready to post or email.</returns>
    public static string GetDnaLetter(string patientName, string service,
        string appointmentDate, string timeSlot)
    {
        string prompt =
            "You are a patient services coordinator at Portmore Medical Center.\n\n" +
            "Write a professional, compassionate Did Not Attend (DNA) letter for:\n" +
            "Patient name: " + patientName + "\n" +
            "Service: " + service + "\n" +
            "Missed appointment date: " + appointmentDate + "\n" +
            "Time slot: " + timeSlot + "\n\n" +
            "The letter should:\n" +
            "1. Be addressed formally to the patient\n" +
            "2. Note that they missed their appointment without prior cancellation\n" +
            "3. Express concern for their wellbeing (not blame)\n" +
            "4. Encourage them to rebook as soon as possible\n" +
            "5. Remind them of the cancellation policy (24 hours notice appreciated)\n" +
            "6. Provide contact details: appointments@portmoremedical.com | +1 (876) 555-0100\n" +
            "7. Close warmly, signed from 'Patient Services, Portmore Medical Center'\n\n" +
            "Professional but compassionate tone. Plain text only. Under 250 words.";

        return CallGpt(prompt, temp: 0.4, maxTokens: 350)
               ?? "Dear " + patientName + ",\n\n" +
                  "We are writing to inform you that you did not attend your scheduled " + service +
                  " appointment on " + appointmentDate + " at " + timeSlot + ".\n\n" +
                  "We understand that circumstances can prevent attendance and hope that you are well. " +
                  "However, as we were unable to offer your slot to another patient, we kindly ask that " +
                  "you contact us to rebook your appointment at your earliest convenience.\n\n" +
                  "Where possible, please give us at least 24 hours notice if you are unable to attend " +
                  "a future appointment.\n\n" +
                  "To rebook, please contact us at appointments@portmoremedical.com or call " +
                  "+1 (876) 555-0100.\n\n" +
                  "Kind regards,\nPatient Services\nPortmore Medical Center";
    }
}
