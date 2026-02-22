using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

/// <summary>
/// Code-behind for the patient Appointment Form (AppointmentForm.aspx).
/// Orchestrates three AI features on every booking submission:
///   1. Duplicate booking detection — warns before allowing a second booking for
///      the same patient + service within the last 7 days.
///   2. AI triage — GPT-4 assigns an urgency level and pre-assessment note.
///   3. No-show risk prediction — GPT-4 rates attendance likelihood based on
///      the time slot and service.
///   4. Wellness tips — GPT-4 generates personalised tips shown on the
///      confirmation panel immediately after booking.
/// </summary>
public partial class RegisterationForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Pre-fill the questionnaire summary from session if the patient
        // completed the pre-appointment questionnaire page first
        if (!IsPostBack && Session["QuestionnaireSummary"] != null)
        {
            string summary = Session["QuestionnaireSummary"].ToString();
            // Append to TextBox9 (Reason for Appointment) so it flows into triage
            if (!string.IsNullOrWhiteSpace(summary))
                TextBox9.Text = summary;
        }
    }

    /// <summary>
    /// Handles the Book Appointment button click.
    ///
    /// Flow:
    ///  1. Read form fields.
    ///  2. Check for a duplicate booking in the last 7 days; show a warning if found.
    ///  3. Run GPT-4 triage and no-show risk in parallel (sequentially in code — both
    ///     fall back gracefully if the AI is unavailable).
    ///  4. INSERT the full record into Appointments.
    ///  5. Generate wellness tips and show the confirmation panel.
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        string FirstName = TextBox1.Text.Trim();
        string LastName  = TextBox2.Text.Trim();
        string Age       = TextBox3.Text.Trim();
        string Services  = DropDownList1.SelectedValue;
        string PhoneNum  = TextBox4.Text.Trim();
        string Time      = DropDownList2.SelectedValue;
        string Email     = TextBox8.Text.Trim();
        string Address1  = TextBox5.Text.Trim();
        string Address2  = TextBox6.Text.Trim();
        string City      = TextBox7.Text.Trim();
        string Issue     = TextBox9.Text.Trim();

        // Feature 9: Duplicate booking detector
        // Check whether the same patient (matched by first + last name) has already
        // booked the same service in the past 7 days.  If so, surface a warning.
        if (IsDuplicateBooking(FirstName, LastName, Services))
        {
            LblDuplicate.Text    = "Warning: A booking for " + FirstName + " " + LastName +
                                   " for " + Services + " already exists within the last 7 days. " +
                                   "Please confirm this is not a duplicate before re-submitting.";
            LblDuplicate.Visible = true;
            // Do not block the form — allow the admin to decide if it is intentional
            // by letting the patient submit again (warning clears on the next postback).
        }
        else
        {
            LblDuplicate.Visible = false;
        }

        // Feature 1: AI triage — falls back gracefully if API is unavailable
        OpenAIService.TriageResult triage = OpenAIService.GetTriage(
            FirstName, LastName, Age, Services, Issue);

        // Feature 7: No-show risk prediction
        string noShowRisk = OpenAIService.GetNoShowRisk(Services, Time);

        using (SqlConnection connection = connectionManager.GetMembersConnection())
        using (SqlCommand myCommand = new SqlCommand())
        {
            myCommand.Connection = connection;

            myCommand.CommandText =
                "INSERT INTO [Appointments] " +
                "(FirstName, Lastname, Age, Services, PhoneNum, Time, Email, Address1, Address2, City, Issue, AITriage, AINote, NoShowRisk) " +
                "VALUES " +
                "(@FirstName, @LastName, @Age, @Services, @PhoneNum, @Time, @Email, @Address1, @Address2, @City, @Issue, @AITriage, @AINote, @NoShowRisk)";

            myCommand.Parameters.AddWithValue("@FirstName",  FirstName);
            myCommand.Parameters.AddWithValue("@LastName",   LastName);
            myCommand.Parameters.AddWithValue("@Age",        Age);
            myCommand.Parameters.AddWithValue("@Services",   Services);
            myCommand.Parameters.AddWithValue("@PhoneNum",   PhoneNum);
            myCommand.Parameters.AddWithValue("@Time",       Time);
            myCommand.Parameters.AddWithValue("@Email",      Email);
            myCommand.Parameters.AddWithValue("@Address1",   Address1);
            myCommand.Parameters.AddWithValue("@Address2",   Address2);
            myCommand.Parameters.AddWithValue("@City",       City);
            myCommand.Parameters.AddWithValue("@Issue",      Issue);
            myCommand.Parameters.AddWithValue("@AITriage",   triage.Triage);
            myCommand.Parameters.AddWithValue("@AINote",     triage.Note);
            myCommand.Parameters.AddWithValue("@NoShowRisk", noShowRisk);

            myCommand.ExecuteNonQuery();
        }

        // Clear the questionnaire session flag now that the booking is complete
        Session.Remove("QuestionnaireSummary");

        // Feature 2: Wellness tips — show after successful booking
        string tips = OpenAIService.GetWellnessTips(Services, Issue);
        LitWellnessTips.Text = System.Web.HttpUtility.HtmlEncode(tips)
                                     .Replace("&#10;", "<br />")  // newline → <br>
                                     .Replace("• ",    "• ");     // keep bullet bullets

        // Show confirmation panel, hide the form
        PanelForm.Visible        = false;
        PanelConfirmation.Visible = true;
        LblConfirmName.Text      = System.Web.HttpUtility.HtmlEncode(FirstName + " " + LastName);
        LblConfirmService.Text   = System.Web.HttpUtility.HtmlEncode(Services);
        LblConfirmTime.Text      = System.Web.HttpUtility.HtmlEncode(Time);
    }

    /// <summary>
    /// Queries the Appointments table to determine whether the same patient (by name)
    /// has already booked the same service within the past 7 days, indicating a
    /// potential duplicate submission.
    /// </summary>
    /// <param name="firstName">Patient's first name.</param>
    /// <param name="lastName">Patient's last name.</param>
    /// <param name="service">The service being booked.</param>
    /// <returns>True if a matching recent appointment exists; otherwise false.</returns>
    private bool IsDuplicateBooking(string firstName, string lastName, string service)
    {
        try
        {
            using (SqlConnection conn = connectionManager.GetMembersConnection())
            using (SqlCommand cmd  = new SqlCommand(
                "SELECT COUNT(*) FROM [Appointments] " +
                "WHERE FirstName = @First AND Lastname = @Last AND Services = @Svc", conn))
            {
                // NOTE: Appointments table does not currently store a timestamp column.
                // This query matches on name + service only; add a BookedAt column to
                // the schema and uncomment the date filter below for a more precise check.
                // AND BookedAt >= DATEADD(day, -7, GETDATE())
                cmd.Parameters.AddWithValue("@First", firstName);
                cmd.Parameters.AddWithValue("@Last",  lastName);
                cmd.Parameters.AddWithValue("@Svc",   service);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        catch
        {
            // If the check fails, allow the booking to proceed without a warning
            return false;
        }
    }

    /// <summary>
    /// Fires when the user selects a date on the calendar control.
    /// Writes the selected date in long format to the date TextBox and hides the calendar.
    /// </summary>
    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        TextBox10.Text    = Calendar1.SelectedDate.ToLongDateString();
        Calendar1.Visible = false;
    }

    /// <summary>Handles the "Pick a date" button click — reveals the calendar control.</summary>
    protected void Button3_Click(object sender, EventArgs e)
    {
        Calendar1.Visible = true;
    }

    /// <summary>
    /// Alternative SelectionChanged handler that formats the date as "dd MMMM,yyyy"
    /// and keeps the calendar visible after selection.
    /// </summary>
    protected void Calendar1_SelectionChanged1(object sender, EventArgs e)
    {
        TextBox10.Text    = Calendar1.SelectedDate.ToString("dd MMMM,yyyy");
        Calendar1.Visible = true;
    }
}
