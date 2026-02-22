<%@ WebHandler Language="C#" Class="SymptomHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// HTTP handler for the AI symptom checker (SymptomHandler.ashx).
/// Accepts POST requests from the SymptomChecker.aspx front-end,
/// passes the patient's symptom description to GPT-4 via
/// OpenAIService.GetSpecialistRecommendation, and returns a JSON response
/// indicating the most appropriate specialist service.
///
/// Expected form fields:
///   symptoms — plain-text description of the patient's symptoms (required, max 1 000 chars)
///
/// Response format:
///   { "specialist": "...", "reason": "..." }   on success
///   { "error": "..." }                          on bad request
/// </summary>
public class SymptomHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        // Only accept POST to prevent accidental GET requests leaking data via URL
        if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 405;
            context.Response.Write("{\"error\":\"Method not allowed\"}");
            return;
        }

        string symptoms = context.Request.Form["symptoms"];

        if (string.IsNullOrWhiteSpace(symptoms))
        {
            context.Response.Write("{\"error\":\"No symptoms provided\"}");
            return;
        }

        // Truncate oversized input to protect against abuse and excessive API cost
        if (symptoms.Length > 1000)
            symptoms = symptoms.Substring(0, 1000);

        OpenAIService.SpecialistResult result = OpenAIService.GetSpecialistRecommendation(symptoms);

        var serializer = new JavaScriptSerializer();
        context.Response.Write(serializer.Serialize(new
        {
            specialist = result.Specialist,
            reason     = result.Reason
        }));
    }

    /// <summary>
    /// Returns false so IIS creates a new handler instance per request,
    /// avoiding shared state between concurrent symptom-checker sessions.
    /// </summary>
    public bool IsReusable { get { return false; } }
}
