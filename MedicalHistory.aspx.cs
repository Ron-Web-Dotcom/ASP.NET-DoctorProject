using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Medical History Summariser (MedicalHistory.aspx).
///
/// Feature 31 — Medical History Summariser:
///   The patient fills in up to five structured fields (current conditions,
///   current medications, known allergies, past surgeries, and family history).
///   GPT-4 generates a concise, professional clinical summary suitable for
///   sharing with any healthcare provider.
///
///   At least one field must be populated before submission. The result panel
///   includes a Print button so patients can bring a physical copy to their
///   next appointment.
/// </summary>
public partial class MedicalHistory : System.Web.UI.Page
{
    /// <summary>
    /// Standard page lifecycle handler. No initialisation is required on first load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Generate Clinical Summary button click.
    ///
    /// Flow:
    ///  1. Read all five history fields, trimming whitespace.
    ///  2. Validate that at least one field contains data.
    ///  3. Call OpenAIService.GetMedicalHistorySummary with the patient's data.
    ///  4. HTML-encode the result and render it in the result panel.
    ///  5. Switch from the form panel to the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string conditions    = TxtConditions.Text.Trim();
        string medications   = TxtMedications.Text.Trim();
        string allergies     = TxtAllergies.Text.Trim();
        string surgeries     = TxtSurgeries.Text.Trim();
        string familyHistory = TxtFamilyHistory.Text.Trim();

        if (string.IsNullOrWhiteSpace(conditions)  && string.IsNullOrWhiteSpace(medications) &&
            string.IsNullOrWhiteSpace(allergies)   && string.IsNullOrWhiteSpace(surgeries)   &&
            string.IsNullOrWhiteSpace(familyHistory))
        {
            LblError.Text    = "Please fill in at least one field before generating your summary.";
            LblError.Visible = true;
            return;
        }
        LblError.Visible = false;

        string summary      = OpenAIService.GetMedicalHistorySummary(conditions, medications, allergies, surgeries, familyHistory);
        LitSummary.Text     = System.Web.HttpUtility.HtmlEncode(summary);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    /// <summary>
    /// Handles the Edit &amp; Regenerate button click.
    /// Returns the page to the entry form so the patient can update their data.
    /// </summary>
    protected void BtnReset_Click(object sender, EventArgs e)
    {
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
