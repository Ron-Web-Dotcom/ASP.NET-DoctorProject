using System;
using System.Web.UI;

/// <summary>
/// Code-behind for AllergyFoodGuide.aspx.
/// Takes a comma-separated list of patient allergies and calls
/// <see cref="OpenAIService.GetAllergyFoodGuide"/> to produce a personalised
/// food safety guide via GPT-4.
/// </summary>
public partial class AllergyFoodGuide : Page
{
    /// <summary>
    /// Standard page load handler. No data-binding required on load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Generate Guide button click.
    /// Validates that at least one allergy has been entered, calls GPT-4,
    /// HTML-encodes the result for XSS safety, and reveals the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string allergies = TxtAllergies.Text.Trim();

        if (string.IsNullOrWhiteSpace(allergies))
        {
            LblError.Text    = "Please enter at least one allergy.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string guide = OpenAIService.GetAllergyFoodGuide(allergies)
                       ?? "Unable to generate your food safety guide at this time. Please try again later.";

        LitGuide.Text       = System.Web.HttpUtility.HtmlEncode(guide);
        PanelResult.Visible = true;
    }
}
