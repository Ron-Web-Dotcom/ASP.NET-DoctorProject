using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the AI Insurance &amp; Cost Estimator (InsuranceEstimator.aspx).
///
/// Feature 27 — AI Insurance &amp; Cost Estimator:
///   Patient selects a service and their insurance type. GPT-4 produces a
///   plain-English guide covering what is typically covered, questions to ask
///   their insurer, and general cost factors. Includes a mandatory disclaimer.
/// </summary>
public partial class InsuranceEstimator : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnEstimate_Click(object sender, EventArgs e)
    {
        string service       = DdlService.SelectedValue;
        string insuranceType = DdlInsurance.SelectedValue;

        if (string.IsNullOrWhiteSpace(service))
        {
            LblError.Text    = "Please select a service before generating your guide.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string guide = OpenAIService.GetInsuranceGuide(service, insuranceType);

        LitService.Text   = System.Web.HttpUtility.HtmlEncode(service);
        LitInsurance.Text = System.Web.HttpUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(insuranceType) ? "Not specified" : insuranceType);
        LitGuide.Text     = System.Web.HttpUtility.HtmlEncode(guide);
        PanelResult.Visible = true;
    }
}
