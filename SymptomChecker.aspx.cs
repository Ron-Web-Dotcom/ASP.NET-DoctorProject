using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the AI Symptom Checker page (SymptomChecker.aspx).
/// Patients describe their symptoms in a free-text box; the page submits the
/// description to SymptomHandler.ashx via AJAX, which calls
/// <see cref="OpenAIService.GetSpecialistRecommendation"/> and returns a JSON
/// object containing a recommended specialist and a plain-English reason.
/// All processing is asynchronous — this Page class requires no postback handlers.
/// </summary>
public partial class SymptomChecker : Page
{
    /// <summary>
    /// Standard page load. All interaction is driven by SymptomHandler.ashx via AJAX.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // Logic handled client-side via SymptomHandler.ashx
    }
}

