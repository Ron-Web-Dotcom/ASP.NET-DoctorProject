using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Cancel/Reschedule Appointment page (CancelAppointment.aspx).
///
/// Feature 3 — Appointment Rescheduler:
/// When a patient needs to cancel their appointment, this page collects their
/// name, the service they booked, and an optional cancellation reason, then
/// calls OpenAIService.GetRescheduleMessage to generate a personalised,
/// understanding message that encourages them to rebook promptly.
///
/// No appointment record is deleted here — this is a self-service guidance page.
/// Actual cancellation is handled by clinic staff once the patient contacts them.
/// </summary>
public partial class CancelAppointment : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Submit Cancellation Request button click.
    ///
    /// Flow:
    ///  1. Validate that a name and service were supplied.
    ///  2. Call OpenAIService.GetRescheduleMessage with the patient's details.
    ///  3. Display the AI-drafted rescheduling message and reveal the rebook button.
    /// </summary>
    protected void BtnCancel_Click(object sender, EventArgs e)
    {
        string firstName = TxtFirstName.Text.Trim();
        string service   = DdlService.SelectedValue;
        string reason    = TxtReason.Text.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || service.StartsWith("Select"))
        {
            LblError.Text    = "Please enter your name and select the service you booked.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        // Generate a personalised, empathetic rescheduling message via GPT-4
        string msg = OpenAIService.GetRescheduleMessage(firstName, service, reason);

        LitRescheduleMsg.Text = System.Web.HttpUtility.HtmlEncode(msg);
        PanelForm.Visible     = false;
        PanelResult.Visible   = true;
    }
}
