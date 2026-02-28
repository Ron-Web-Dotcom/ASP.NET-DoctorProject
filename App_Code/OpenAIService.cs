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
}
