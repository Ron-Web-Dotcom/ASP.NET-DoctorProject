<%@ WebHandler Language="C#" Class="TranslateHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// HTTP handler for the multi-language translation feature (TranslateHandler.ashx).
///
/// Feature 4 — Multi-Language Support:
/// Accepts POST requests from any page that has the language toggle enabled.
/// Passes the supplied text to OpenAIService.TranslateText and returns the
/// translated result as JSON.
///
/// Expected form fields:
///   text     — the source text to translate (required, max 2 000 chars)
///   language — target language name (required, e.g. "Spanish", "French", "Mandarin")
///
/// Response format:
///   { "translated": "..." }   on success
///   { "error": "..." }        on bad request
/// </summary>
public class TranslateHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        // Only accept POST to prevent URL-based data leakage
        if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 405;
            context.Response.Write("{\"error\":\"Method not allowed\"}");
            return;
        }

        string text     = context.Request.Form["text"];
        string language = context.Request.Form["language"];

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(language))
        {
            context.Response.Write("{\"error\":\"Both text and language are required\"}");
            return;
        }

        // Truncate to protect against excessive API usage
        if (text.Length > 2000)
            text = text.Substring(0, 2000);

        string translated = OpenAIService.TranslateText(text, language);

        var serializer = new JavaScriptSerializer();
        context.Response.Write(serializer.Serialize(new { translated = translated }));
    }

    /// <summary>
    /// Returns false so IIS creates a new handler instance per request,
    /// avoiding any shared state between concurrent translation requests.
    /// </summary>
    public bool IsReusable { get { return false; } }
}
