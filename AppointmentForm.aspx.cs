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
/// Code-behind for the patient Appointment Form page (AppointmentForm.aspx).
/// Collects patient intake details, runs an AI triage assessment via GPT-4,
/// and saves the full record (including the triage result) to the Appointments table.
/// </summary>
public partial class RegisterationForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Handles the Book Appointment button click.
    /// 1. Reads all patient intake fields from the form.
    /// 2. Calls OpenAIService.GetTriage to obtain an AI urgency level and clinical note.
    ///    If the AI call fails, GetTriage returns a safe fallback ("Error" / "Pending")
    ///    so the form submission is never blocked by an API failure.
    /// 3. Inserts the full record into the Appointments table using parameterised
    ///    queries to prevent SQL injection.
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        string FirstName = TextBox1.Text;
        string LastName  = TextBox2.Text;
        string Age       = TextBox3.Text;
        string Services  = DropDownList1.SelectedValue;
        string PhoneNum  = TextBox4.Text;
        string Time      = DropDownList2.SelectedValue;
        string Email     = TextBox8.Text;
        string Address1  = TextBox5.Text;
        string Address2  = TextBox6.Text;
        string City      = TextBox7.Text;
        string Issue     = TextBox9.Text;

        // Run GPT-4 triage before saving; falls back gracefully if the API is unavailable
        OpenAIService.TriageResult triage = OpenAIService.GetTriage(
            FirstName, LastName, Age, Services, Issue);

        using (SqlConnection connection = connectionManager.GetMembersConnection())
        using (SqlCommand myCommand = new SqlCommand())
        {
            myCommand.Connection = connection;

            string insertSQL =
                "INSERT INTO [Appointments] " +
                "(FirstName, Lastname, Age, Services, PhoneNum, Time, Email, Address1, Address2, City, Issue, AITriage, AINote) " +
                "VALUES " +
                "(@FirstName, @LastName, @Age, @Services, @PhoneNum, @Time, @Email, @Address1, @Address2, @City, @Issue, @AITriage, @AINote)";

            myCommand.CommandText = insertSQL;
            myCommand.Parameters.AddWithValue("@FirstName", FirstName);
            myCommand.Parameters.AddWithValue("@LastName",  LastName);
            myCommand.Parameters.AddWithValue("@Age",       Age);
            myCommand.Parameters.AddWithValue("@Services",  Services);
            myCommand.Parameters.AddWithValue("@PhoneNum",  PhoneNum);
            myCommand.Parameters.AddWithValue("@Time",      Time);
            myCommand.Parameters.AddWithValue("@Email",     Email);
            myCommand.Parameters.AddWithValue("@Address1",  Address1);
            myCommand.Parameters.AddWithValue("@Address2",  Address2);
            myCommand.Parameters.AddWithValue("@City",      City);
            myCommand.Parameters.AddWithValue("@Issue",     Issue);
            myCommand.Parameters.AddWithValue("@AITriage",  triage.Triage); // e.g. "Urgent", "High", "Medium", "Low", "Error"
            myCommand.Parameters.AddWithValue("@AINote",    triage.Note);   // brief clinical pre-assessment

            myCommand.ExecuteNonQuery();
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

    /// <summary>
    /// Handles the "Pick a date" button click — makes the calendar control visible.
    /// </summary>
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
