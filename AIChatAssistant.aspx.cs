using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the AI Chat Assistant page (AIChatAssistant.aspx).
/// Provides a live GPT-4 powered chat interface for patients to ask general
/// health and clinic questions. All message handling is done asynchronously
/// via ChatHandler.ashx — this Page class requires no postback logic.
/// The handler maintains conversation history in session and returns the
/// assistant reply as JSON, which the page's JavaScript renders in the chat window.
/// </summary>
public partial class AIChatAssistant : Page
{
    /// <summary>
    /// Standard page load. All interaction is driven by ChatHandler.ashx via AJAX.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // No server-side logic needed; chat is handled via ChatHandler.ashx AJAX calls.
    }
}

