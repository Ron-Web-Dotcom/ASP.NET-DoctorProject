using System;
using System.Web.UI;

/// <summary>
/// Code-behind for AppointmentEmailPreview.aspx.
/// Accepts appointment details entered by the patient and calls
/// <see cref="OpenAIService.GetConfirmationEmailDraft"/> to generate a
/// personalised confirmation email draft via GPT-4.
/// </summary>
public partial class AppointmentEmailPreview : Page
{
    /// <summary>
    /// Standard page load handler. No data-binding required on load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Generate Email Draft button click.
    /// Validates that the patient name and appointment date fields are populated,
    /// calls GPT-4 to draft the email, HTML-encodes the response for safe rendering,
    /// and reveals the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string name = TxtPatientName.Text.Trim();
        string date = TxtAppointmentDate.Text.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(date))
        {
            LblError.Text    = "Please enter your name and appointment date.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string service  = DdlService.SelectedValue;
        string timeSlot = DdlTimeSlot.SelectedValue;

        string draft = OpenAIService.GetConfirmationEmailDraft(name, service, timeSlot, date)
                       ?? "Unable to generate email draft at this time. Please try again later.";

        LitEmail.Text        = System.Web.HttpUtility.HtmlEncode(draft);
        PanelResult.Visible  = true;
    }
}
