using System;
using System.Web.UI;

public partial class MedicalHistory : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string conditions     = TxtConditions.Text.Trim();
        string medications    = TxtMedications.Text.Trim();
        string allergies      = TxtAllergies.Text.Trim();
        string surgeries      = TxtSurgeries.Text.Trim();
        string familyHistory  = TxtFamilyHistory.Text.Trim();

        if (string.IsNullOrWhiteSpace(conditions) && string.IsNullOrWhiteSpace(medications) &&
            string.IsNullOrWhiteSpace(allergies)  && string.IsNullOrWhiteSpace(surgeries) &&
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

    protected void BtnReset_Click(object sender, EventArgs e)
    {
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
