using System;
using System.Web.UI;

/// <summary>
/// Code-behind for PreSurgerySupport.aspx.
/// Takes the patient's procedure type and optional concern text, then calls
/// <see cref="OpenAIService.GetPreSurgerySupport"/> to produce a compassionate,
/// evidence-based pre-operative support guide via GPT-4.
/// </summary>
public partial class PreSurgerySupport : Page
{
    /// <summary>
    /// Standard page load handler. No data-binding required on load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Get Support button click.
    /// Reads the selected procedure and optional concerns, calls GPT-4 for a
    /// support guide, HTML-encodes the output for safe rendering, and shows
    /// the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string procedure = DdlProcedure.SelectedValue;
        string concerns  = TxtConcerns.Text.Trim();

        string guide = OpenAIService.GetPreSurgerySupport(procedure, concerns)
                       ?? "Unable to generate your support guide at this time. Please contact our team directly.";

        LitSupport.Text     = System.Web.HttpUtility.HtmlEncode(guide);
        PanelResult.Visible = true;
    }
}
