using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Staff Bio Generator admin page (StaffBioGenerator.aspx).
///
/// Feature 9 — Staff Bio Generator:
/// Admin staff enter a new doctor's name, specialty, qualifications, years of experience,
/// and any additional details. OpenAIService.GetStaffBio returns a polished 2-3 paragraph
/// biography ready to paste into the staff profile page.
///
/// Access is not session-guarded here as it is intended to be accessed from the admin area
/// via the AdminView link. The page itself does not modify any database records.
/// </summary>
public partial class StaffBioGenerator : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Generate Biography button click.
    /// Validates required fields, calls GPT-4, and shows the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string name          = TxtName.Text.Trim();
        string specialty     = DdlSpecialty.SelectedValue;
        string qualifications = TxtQualifications.Text.Trim();
        string years         = TxtYears.Text.Trim();
        string extra         = TxtExtra.Text.Trim();

        if (string.IsNullOrWhiteSpace(name) || specialty.StartsWith("Select") ||
            string.IsNullOrWhiteSpace(qualifications) || string.IsNullOrWhiteSpace(years))
        {
            LblError.Text    = "Please fill in all required fields.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string bio          = OpenAIService.GetStaffBio(name, specialty, qualifications, years, extra);
        LitBio.Text         = System.Web.HttpUtility.HtmlEncode(bio);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    /// <summary>Resets the form so admin can generate another biography.</summary>
    protected void BtnGenerateAnother_Click(object sender, EventArgs e)
    {
        TxtName.Text           = string.Empty;
        TxtQualifications.Text = string.Empty;
        TxtYears.Text          = string.Empty;
        TxtExtra.Text          = string.Empty;
        PanelResult.Visible    = false;
        PanelForm.Visible      = true;
    }
}
