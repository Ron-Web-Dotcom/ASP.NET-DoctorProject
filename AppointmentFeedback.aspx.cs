using System;
using System.Data.SqlClient;
using System.Web.UI;

/// <summary>
/// Code-behind for the Post-Appointment Feedback page (AppointmentFeedback.aspx).
///
/// Feature — Post-Appointment Feedback:
///   Patients submit a star rating (1–5) and a free-text comment after their visit.
///   GPT-4 analyses the comment and assigns a sentiment level (Positive / Neutral /
///   Distressed / Urgent). The name, email, service, rating, comment, sentiment,
///   and sentiment reason are persisted to the Feedback table for admin review.
///
///   Requires the Feedback table — run App_Data/FeedbackSchema.sql once.
/// </summary>
public partial class AppointmentFeedback : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnSubmit_Click(object sender, EventArgs e)
    {
        string name    = TxtName.Text.Trim();
        string email   = TxtEmail.Text.Trim();
        string service = DdlService.SelectedValue;
        string comment = TxtComment.Text.Trim();
        int rating     = GetSelectedRating();

        // Basic validation
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(comment) ||
            string.IsNullOrWhiteSpace(service) || rating == 0)
        {
            LblError.Text    = "Please fill in your name, select a service, choose a star rating, and leave a comment.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        // AI sentiment analysis on the comment
        var sentimentResult = OpenAIService.GetSentiment(comment);
        string sentiment       = sentimentResult != null ? sentimentResult.Level  : "Neutral";
        string sentimentReason = sentimentResult != null ? sentimentResult.Reason : "";

        // Persist to database
        SaveFeedback(name, email, service, rating, comment, sentiment, sentimentReason);

        // Show thank-you panel
        LitSentimentBadge.Text  = GetSentimentBadge(sentiment);
        LitSentimentReason.Text = System.Web.HttpUtility.HtmlEncode(sentimentReason);
        PanelForm.Visible       = false;
        PanelThankyou.Visible   = true;
    }

    /// <summary>
    /// Reads the selected RadioButton value from the star group.
    /// Returns 0 if none selected.
    /// </summary>
    private int GetSelectedRating()
    {
        if (Star5.Checked) return 5;
        if (Star4.Checked) return 4;
        if (Star3.Checked) return 3;
        if (Star2.Checked) return 2;
        if (Star1.Checked) return 1;
        return 0;
    }

    /// <summary>
    /// Inserts the feedback record into the Feedback table using a parameterised query.
    /// Silently swallows DB errors so the thank-you screen still shows.
    /// </summary>
    private void SaveFeedback(string name, string email, string service,
        int rating, string comment, string sentiment, string sentimentReason)
    {
        try
        {
            using (SqlConnection conn = connectionManager.GetMembersConnection())
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO [Feedback] (PatientName, Email, Service, Rating, Comment, Sentiment, SentimentReason, SubmittedAt) " +
                "VALUES (@Name, @Email, @Service, @Rating, @Comment, @Sentiment, @SentimentReason, GETDATE())", conn))
            {
                cmd.Parameters.AddWithValue("@Name",            name);
                cmd.Parameters.AddWithValue("@Email",           email);
                cmd.Parameters.AddWithValue("@Service",         service);
                cmd.Parameters.AddWithValue("@Rating",          rating);
                cmd.Parameters.AddWithValue("@Comment",         comment);
                cmd.Parameters.AddWithValue("@Sentiment",       sentiment);
                cmd.Parameters.AddWithValue("@SentimentReason", sentimentReason);
                cmd.ExecuteNonQuery();
            }
        }
        catch { /* graceful degradation — thank-you screen still shown */ }
    }

    /// <summary>Returns a colour-coded HTML badge for the sentiment level.</summary>
    private string GetSentimentBadge(string sentiment)
    {
        switch ((sentiment ?? "").Trim().ToLower())
        {
            case "urgent":     return "<span style='background:#F2DEDE;color:#A94442;padding:2px 8px;border-radius:4px;font-weight:bold;'>Urgent</span>";
            case "distressed": return "<span style='background:#FFE0B2;color:#E65100;padding:2px 8px;border-radius:4px;font-weight:bold;'>Distressed</span>";
            case "positive":   return "<span style='background:#DFF0D8;color:#3C763D;padding:2px 8px;border-radius:4px;font-weight:bold;'>Positive</span>";
            default:           return "<span style='background:#F5F5F5;color:#555;padding:2px 8px;border-radius:4px;'>Neutral</span>";
        }
    }
}
