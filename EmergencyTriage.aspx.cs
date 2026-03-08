using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Emergency Symptom Triage page (EmergencyTriage.aspx).
///
/// Feature 30 — Emergency Symptom Triage:
///   The patient describes their symptoms in a free-text box. GPT-4 classifies
///   the situation into exactly one of four urgency levels:
///     EMERGENCY   — Call 999 immediately. Life-threatening signs present.
///     URGENT      — Visit A&amp;E or urgent care today.
///     APPOINTMENT — Book a GP or specialist appointment within days.
///     SELF-CARE   — Manageable at home with practical self-care steps.
///
///   The response text is scanned for the "Level:" prefix written by GPT-4 to
///   determine which colour-coded banner to display. Every response ends with
///   a reminder to seek immediate help if symptoms worsen.
///
///   IMPORTANT: This tool provides general guidance only. It is not a substitute
///   for professional medical assessment. The page displays a prominent 999 warning
///   above the form regardless of triage outcome.
/// </summary>
public partial class EmergencyTriage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Assess My Symptoms button click.
    ///
    /// Flow:
    ///  1. Validate that the symptoms field is not empty.
    ///  2. Call OpenAIService.GetEmergencyTriage with the symptom text.
    ///  3. Scan the response for the "Level:" line produced by GPT-4.
    ///  4. Render the appropriate colour-coded badge and the full response text.
    ///  5. Switch from the form panel to the result panel.
    /// </summary>
    protected void BtnTriage_Click(object sender, EventArgs e)
    {
        string symptoms = TxtSymptoms.Text.Trim();
        if (string.IsNullOrWhiteSpace(symptoms))
        {
            LblError.Text    = "Please describe your symptoms before submitting.";
            LblError.Visible = true;
            return;
        }
        LblError.Visible = false;

        string result = OpenAIService.GetEmergencyTriage(symptoms);

        // Determine the urgency level by scanning the GPT-4 "Level:" prefix.
        // Defaults to SELF-CARE so the page always shows a safe, actionable message.
        string level;
        if (result.IndexOf("Level: EMERGENCY", StringComparison.OrdinalIgnoreCase) >= 0)
            level = "emergency";
        else if (result.IndexOf("Level: URGENT", StringComparison.OrdinalIgnoreCase) >= 0)
            level = "urgent";
        else if (result.IndexOf("Level: APPOINTMENT", StringComparison.OrdinalIgnoreCase) >= 0)
            level = "appointment";
        else
            level = "selfcare";

        switch (level)
        {
            case "emergency":
                LitLevelBadge.Text = "<div class='level-emergency'><span class='glyphicon glyphicon-warning-sign'></span> EMERGENCY — Call 999 immediately</div>";
                break;
            case "urgent":
                LitLevelBadge.Text = "<div class='level-urgent'><span class='glyphicon glyphicon-exclamation-sign'></span> URGENT — Visit A&amp;E or urgent care today</div>";
                break;
            case "appointment":
                LitLevelBadge.Text = "<div class='level-appointment'><span class='glyphicon glyphicon-calendar'></span> APPOINTMENT — Book with your GP or specialist</div>";
                break;
            default:
                LitLevelBadge.Text = "<div class='level-selfcare'><span class='glyphicon glyphicon-home'></span> SELF-CARE — Manageable at home</div>";
                break;
        }

        LitResult.Text      = System.Web.HttpUtility.HtmlEncode(result);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    /// <summary>
    /// Handles the Assess Another Symptom button click.
    /// Clears the symptom input and returns the page to the entry form.
    /// </summary>
    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtSymptoms.Text    = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
