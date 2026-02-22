<%@ WebHandler Language="C#" Class="ChatHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// HTTP handler for the AI chat assistant (ChatHandler.ashx).
/// Accepts POST requests from the AIChatAssistant.aspx front-end,
/// forwards the message and optional conversation history to GPT-4 via
/// OpenAIService.GetChatResponse, and returns a JSON response.
///
/// Expected form fields:
///   message  — the user's latest chat message (required, max 1 000 chars)
///   history  — JSON array of prior {role, content} turns (optional)
///
/// Response format:
///   { "reply": "..." }   on success
///   { "error": "..." }   on bad request
/// </summary>
public class ChatHandler : IHttpHandler
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

        string userMessage      = context.Request.Form["message"];
        string conversationJson = context.Request.Form["history"];  // optional prior turns

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            context.Response.Write("{\"error\":\"No message provided\"}");
            return;
        }

        // Truncate oversized messages to protect against abuse and excessive API cost
        if (userMessage.Length > 1000)
            userMessage = userMessage.Substring(0, 1000);

        string reply = OpenAIService.GetChatResponse(userMessage, conversationJson);

        var serializer = new JavaScriptSerializer();
        context.Response.Write(serializer.Serialize(new { reply = reply }));
    }

    /// <summary>
    /// Returns false so IIS creates a new handler instance per request,
    /// avoiding shared state between concurrent chat sessions.
    /// </summary>
    public bool IsReusable { get { return false; } }
}
