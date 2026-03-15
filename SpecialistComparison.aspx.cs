using System;
using System.Web.UI;

/// <summary>
/// Code-behind for SpecialistComparison.aspx.
/// Accepts two specialty selections from the patient and calls
/// <see cref="OpenAIService.GetSpecialistComparison"/> to produce a plain-English
/// comparison via GPT-4, helping the patient decide which specialist to see.
/// </summary>
public partial class SpecialistComparison : Page
{
    /// <summary>
    /// Standard page load handler. No data-binding required on load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Compare Specialists button click.
    /// Validates that both selected specialties are different, calls GPT-4 for the
    /// comparison, HTML-encodes the result, and shows the result panel.
    /// </summary>
    protected void BtnCompare_Click(object sender, EventArgs e)
    {
        string s1 = DdlSpecialty1.SelectedValue;
        string s2 = DdlSpecialty2.SelectedValue;

        if (s1 == s2)
        {
            LblError.Text    = "Please select two different specialties to compare.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string comparison = OpenAIService.GetSpecialistComparison(s1, s2)
                            ?? "Unable to generate a comparison at this time. Please try again later.";

        LitComparison.Text  = System.Web.HttpUtility.HtmlEncode(comparison);
        PanelResult.Visible = true;
    }
}
