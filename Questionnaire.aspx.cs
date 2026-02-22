using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Pre-Appointment Questionnaire page (Questionnaire.aspx).
///
/// Feature 1 — Pre-Appointment Questionnaire Analyser:
/// Collects five structured questions about the patient's condition before they
/// reach the appointment form, then calls OpenAIService.GetQuestionnaireAnalysis
/// to produce a concise clinical pre-screening summary.
///
/// The summary is stored in Session["QuestionnaireSummary"] so that
/// AppointmentForm.aspx.cs can pre-populate the "Reason for Appointment" field,
/// giving the treating doctor richer context even before the consultation.
/// </summary>
public partial class Questionnaire : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the "Analyse and Continue" button click.
    ///
    /// Flow:
    ///  1. Validate that the required dropdowns have been answered.
    ///  2. Pass all answers to OpenAIService.GetQuestionnaireAnalysis.
    ///  3. Store the resulting summary in session so AppointmentForm can read it.
    ///  4. Hide the questionnaire panel and display the summary result panel.
    /// </summary>
    protected void BtnAnalyse_Click(object sender, EventArgs e)
    {
        string service    = DdlService.SelectedValue;
        string duration   = DdlDuration.SelectedValue;
        string severity   = DdlSeverity.SelectedValue;
        string meds       = TxtMedications.Text.Trim();
        string allergies  = TxtAllergies.Text.Trim();
        string additional = TxtAdditional.Text.Trim();

        // Basic validation — ensure the key dropdowns have been answered
        if (service.StartsWith("Select") || duration.StartsWith("Select") || severity.StartsWith("Select"))
        {
            LblError.Text    = "Please answer all required questions before continuing.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        // Default text for optional free-text fields
        if (string.IsNullOrWhiteSpace(meds))       meds       = "None stated";
        if (string.IsNullOrWhiteSpace(allergies))   allergies  = "None stated";
        if (string.IsNullOrWhiteSpace(additional))  additional = "None";

        // Ask GPT-4 to summarise the answers into a clinical pre-screening note
        string summary = OpenAIService.GetQuestionnaireAnalysis(
            service, duration, severity, meds, allergies, additional);

        // Store in session for AppointmentForm to read
        Session["QuestionnaireSummary"] = summary;

        // Show the result and hide the form
        LitSummary.Text              = System.Web.HttpUtility.HtmlEncode(summary);
        PanelQuestionnaire.Visible   = false;
        PanelResult.Visible          = true;
    }
}
