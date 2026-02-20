using System;
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
}
