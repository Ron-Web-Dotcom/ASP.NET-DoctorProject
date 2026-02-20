<%@ WebHandler Language="C#" Class="ChatHandler" %>

using System;
using System.Web;
using System.Web.Script.Serialization;

public class ChatHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        // Only accept POST
        if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 405;
            context.Response.Write("{\"error\":\"Method not allowed\"}");
            return;
        }

        string userMessage    = context.Request.Form["message"];
        string conversationJson = context.Request.Form["history"];  // optional prior turns

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            context.Response.Write("{\"error\":\"No message provided\"}");
            return;
        }

        // Limit message length to prevent abuse
        if (userMessage.Length > 1000)
            userMessage = userMessage.Substring(0, 1000);

        string reply = OpenAIService.GetChatResponse(userMessage, conversationJson);

        var serializer = new JavaScriptSerializer();
        context.Response.Write(serializer.Serialize(new { reply = reply }));
    }

    public bool IsReusable { get { return false; } }
}
