using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Mental Health Check-In page (MentalHealthCheckIn.aspx).
///
/// Feature 32 — Mental Health Check-In:
///   The patient rates five wellbeing dimensions on a 1–5 scale:
///     Mood, Sleep Quality, Anxiety (1=very anxious, 5=calm),
///     Energy Levels, and Social Connection.
///   An optional free-text notes field allows the patient to add context.
///
///   GPT-4 produces an empathetic wellbeing summary, 3–4 evidence-based
///   support suggestions tailored to the scores, and clear guidance on when
///   to seek professional help.
///
///   IMPORTANT: This is not a clinical assessment. The result panel and the
///   ASPX disclaimer both include crisis line signposting (NHS 111, Samaritans).
/// </summary>
public partial class MentalHealthCheckIn : System.Web.UI.Page
{
    /// <summary>
    /// Standard page lifecycle handler. No initialisation is required on first load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Get My Wellbeing Summary button click.
    ///
    /// Flow:
    ///  1. Read the five dropdown scores and optional notes field.
    ///  2. Call OpenAIService.GetMentalHealthCheckIn with the scores and notes.
    ///  3. HTML-encode the result and render it in the result panel.
    ///  4. Switch from the form panel to the result panel.
    ///
    /// No minimum-score validation is applied because all dropdowns have a
    /// pre-selected default value (3 = Okay / moderate), so a response is always valid.
    /// </summary>
    protected void BtnCheckIn_Click(object sender, EventArgs e)
    {
        string mood    = DdlMood.SelectedValue;
        string sleep   = DdlSleep.SelectedValue;
        string anxiety = DdlAnxiety.SelectedValue;
        string energy  = DdlEnergy.SelectedValue;
        string social  = DdlSocial.SelectedValue;
        string notes   = TxtNotes.Text.Trim();

        string result       = OpenAIService.GetMentalHealthCheckIn(mood, sleep, anxiety, energy, social, notes);
        LitResult.Text      = System.Web.HttpUtility.HtmlEncode(result);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    /// <summary>
    /// Handles the Check In Again button click.
    /// Clears optional notes and returns the page to the entry form.
    /// Dropdown selections intentionally retain their previous values so the
    /// patient can adjust individual scores rather than re-entering everything.
    /// </summary>
    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtNotes.Text       = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
