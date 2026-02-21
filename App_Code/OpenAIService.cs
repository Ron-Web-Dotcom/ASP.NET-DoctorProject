using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Script.Serialization;

/// <summary>
/// Calls the OpenAI GPT-4 API to generate a triage priority and pre-diagnosis note
/// for incoming patient appointment submissions.
/// </summary>
public static class OpenAIService
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

    public class TriageResult
    {
        public string Triage { get; set; }  // "Urgent" | "High" | "Medium" | "Low" | "Pending" | "Error"
        public string Note   { get; set; }
    }

    /// <summary>
    /// Sends patient intake data to GPT-4 and returns a triage assessment.
    /// Falls back to a "Pending" result if the API key is not configured or the call fails.
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

        string requestJson = Json.Serialize(requestPayload);

        try
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type",  "application/json");
                client.Headers.Add("Authorization", "Bearer " + apiKey);

                string responseJson = client.UploadString(
                    "https://api.openai.com/v1/chat/completions", requestJson);

                // Parse the outer OpenAI response envelope
                var outer   = (Dictionary<string, object>)Json.DeserializeObject(responseJson);
                var choices = (object[])outer["choices"];
                var choice  = (Dictionary<string, object>)choices[0];
                var message = (Dictionary<string, object>)choice["message"];
                string content = message["content"].ToString().Trim();

                // GPT sometimes wraps output in ```json ... ``` — strip that
                if (content.StartsWith("```"))
                {
                    int start = content.IndexOf('{');
                    int end   = content.LastIndexOf('}');
                    if (start >= 0 && end > start)
                        content = content.Substring(start, end - start + 1);
                }

                // Parse the inner triage JSON returned by GPT
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
            // Log to Application event or trace; don't crash the form submission
            System.Diagnostics.Trace.TraceError("OpenAIService.GetTriage error: " + ex.Message);
            return new TriageResult { Triage = "Error", Note = "AI triage could not be completed at this time." };
        }
    }

    // -----------------------------------------------------------------------
    // Shared private helper
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sends a single-turn user message to GPT-4 and returns the raw content string.
    /// Returns null on API key not configured or on any error.
    /// </summary>
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

    // -----------------------------------------------------------------------
    // Symptom-to-specialist checker
    // -----------------------------------------------------------------------

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

        // Strip optional markdown code fences
        if (content.StartsWith("```")) {
            int s = content.IndexOf('{'), e = content.LastIndexOf('}');
            if (s >= 0 && e > s) content = content.Substring(s, e - s + 1);
        }

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
    // Admin AI dashboard insights
    // -----------------------------------------------------------------------

    /// <summary>
    /// Given a plain-text summary of appointment statistics, returns a concise operational insight.
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
    // Contact form auto-reply draft
    // -----------------------------------------------------------------------

    /// <summary>
    /// Drafts a professional reply to a patient's contact form message on behalf of the clinic.
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
    // Daily health tip
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates a daily health tip. Callers should cache this in Application state keyed by date.
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
    // General site assistant (chat)
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
    /// Sends a conversation to GPT-4 and returns the assistant's reply.
    /// conversationJson is a JSON array of {role, content} objects representing prior turns (may be null).
    /// </summary>
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
                    {
                        messages.Add(new Dictionary<string, string>
                        {
                            { "role",    turn["role"].ToString() },
                            { "content", turn["content"].ToString() }
                        });
                    }
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

            string requestJson = Json.Serialize(requestPayload);

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type",  "application/json");
                client.Headers.Add("Authorization", "Bearer " + apiKey);

                string responseJson = client.UploadString(
                    "https://api.openai.com/v1/chat/completions", requestJson);

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
}
