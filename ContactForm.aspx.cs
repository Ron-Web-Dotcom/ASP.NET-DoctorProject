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
/// On submission:
///   1. Feature 5 — Runs GPT-4 sentiment analysis on the patient's message and
///      stores the result (level + reason) in the Contacts table so admin staff
///      can prioritise distressed or urgent enquiries in AdminView.
///   2. Saves the full contact record to the database.
///   3. Feature 4 — Calls OpenAIService.GetContactReply to generate a personalised
///      draft reply which is displayed in a thank-you panel.
/// </summary>
public partial class Form : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Handles the Submit button click.
    ///
    /// Flow:
    ///  1. Run sentiment analysis on the message (does not block submission on failure).
    ///  2. Insert the contact record + sentiment into the Contacts table.
    ///  3. Generate an AI-drafted reply and display it in the thank-you panel.
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        string FirstName = TextBox1.Text.Trim();
        string LastName  = TextBox2.Text.Trim();
        string Email     = TextBox3.Text.Trim();
        string PhoneNum  = TextBox4.Text.Trim();
        string Message   = TextBox5.Text.Trim();

        // Feature 5: Sentiment analysis — determines emotional tone so admin can
        // prioritise distressed or urgent messages without reading every enquiry
        OpenAIService.SentimentResult sentiment = OpenAIService.GetSentiment(Message);

        using (SqlConnection connection = connectionManager.GetMembersConnection())
        using (SqlCommand myCommand = new SqlCommand())
        {
            myCommand.Connection  = connection;
            myCommand.CommandText =
                "INSERT INTO [Contacts] (FirstName, Lastname, Email, PhoneNum, Message, Sentiment, SentimentReason) " +
                "VALUES (@FirstName, @LastName, @Email, @PhoneNum, @Message, @Sentiment, @SentimentReason)";

            myCommand.Parameters.AddWithValue("@FirstName",      FirstName);
            myCommand.Parameters.AddWithValue("@LastName",       LastName);
            myCommand.Parameters.AddWithValue("@Email",          Email);
            myCommand.Parameters.AddWithValue("@PhoneNum",       PhoneNum);
            myCommand.Parameters.AddWithValue("@Message",        Message);
            myCommand.Parameters.AddWithValue("@Sentiment",      sentiment.Level);
            myCommand.Parameters.AddWithValue("@SentimentReason", sentiment.Reason);

            myCommand.ExecuteNonQuery();
        }

        // Feature 4: AI-drafted contact reply — HtmlEncode prevents XSS
        string aiReply        = OpenAIService.GetContactReply(FirstName, Message);
        LitAIReply.Text       = System.Web.HttpUtility.HtmlEncode(aiReply);
        PanelForm.Visible     = false;
        PanelThankyou.Visible = true;
    }
}
