using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Medication Interaction Checker page (MedicationChecker.aspx).
///
/// Feature 2 — Medication Interaction Checker:
/// The patient enters a list of their current medications. OpenAIService.CheckMedicationInteractions
/// reviews the list for any well-known interactions or monitoring requirements and returns
/// a plain-English report with a mandatory clinical disclaimer.
///
/// No data is stored — the interaction check is a stateless, real-time AI call.
/// </summary>
public partial class MedicationChecker : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Check Interactions button click.
    /// Validates input, calls the AI service, and shows the results panel.
    /// </summary>
    protected void BtnCheck_Click(object sender, EventArgs e)
    {
        string meds = TxtMedications.Text.Trim();

        if (string.IsNullOrWhiteSpace(meds))
        {
            LblError.Text    = "Please enter at least one medication before checking.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string report     = OpenAIService.CheckMedicationInteractions(meds);
        LitResult.Text    = System.Web.HttpUtility.HtmlEncode(report);
        PanelForm.Visible = false;
        PanelResult.Visible = true;
    }

    /// <summary>Resets the form so the patient can check a different medication list.</summary>
    protected void BtnCheckAnother_Click(object sender, EventArgs e)
    {
        TxtMedications.Text   = string.Empty;
        PanelResult.Visible   = false;
        PanelForm.Visible     = true;
    }
}
