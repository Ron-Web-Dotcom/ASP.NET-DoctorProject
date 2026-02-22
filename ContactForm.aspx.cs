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
/// Code-behind for the patient Contact Form page (ContactForm.aspx).
/// Saves the patient's enquiry to the Contacts table and then uses the
/// OpenAI service to generate a personalised draft reply, which is
/// displayed in a thank-you panel after submission.
/// </summary>
public partial class Form : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Handles the Submit button click.
    /// Persists the contact enquiry to the database using parameterised queries
    /// to prevent SQL injection, then calls OpenAIService.GetContactReply to
    /// produce an AI-drafted response.
    /// The form panel is hidden and a thank-you panel (with the AI reply) is
    /// shown on success.
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        string FirstName = TextBox1.Text;
        string LastName  = TextBox2.Text;
        string Email     = TextBox3.Text;
        string PhoneNum  = TextBox4.Text;
        string Message   = TextBox5.Text;

        using (SqlConnection connection = connectionManager.GetMembersConnection())
        using (SqlCommand myCommand = new SqlCommand())
        {
            myCommand.Connection  = connection;
            myCommand.CommandText = "INSERT INTO [Contacts] (FirstName, Lastname, Email, PhoneNum, Message) VALUES (@FirstName, @LastName, @Email, @PhoneNum, @Message)";
            myCommand.Parameters.AddWithValue("@FirstName", FirstName);
            myCommand.Parameters.AddWithValue("@LastName",  LastName);
            myCommand.Parameters.AddWithValue("@Email",     Email);
            myCommand.Parameters.AddWithValue("@PhoneNum",  PhoneNum);
            myCommand.Parameters.AddWithValue("@Message",   Message);

            myCommand.ExecuteNonQuery();
        }

        // Ask GPT-4 to draft a personalised reply on behalf of the clinic.
        // HtmlEncode prevents XSS if the AI response contains any HTML characters.
        string aiReply        = OpenAIService.GetContactReply(FirstName, Message);
        LitAIReply.Text       = System.Web.HttpUtility.HtmlEncode(aiReply);
        PanelForm.Visible     = false;
        PanelThankyou.Visible = true;
    }
}
