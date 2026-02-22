using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Code-behind for the Admin Dashboard (AdminView.aspx).
/// Implements the following AI features for admin users:
///   Feature 3  — Triage escalation alert: highlights when 3+ Urgent appointments exist.
///   Feature 5  — Sentiment badges on contact messages (colour-coded in GridView).
///   Feature 6  — Follow-up email drafter: generates a patient follow-up on demand.
///   Feature 7  — No-show risk badges on appointment rows.
///   Feature 8  — Doctor's note simplifier: rewrites clinical notes in plain English.
///   Feature 10 — AI Dashboard Insights panel (appointment statistics summary).
/// Access is restricted to authenticated admins via session guard.
/// </summary>
public partial class AdminView : System.Web.UI.Page
{
    // -----------------------------------------------------------------------
    // Page lifecycle
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enforces authentication and checks triage escalation on every request.
    /// Redirects to Admin.aspx if the admin session is absent.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["FirstName"] == null)
            Response.Redirect("Admin.aspx");

        if (!IsPostBack)
            CheckTriageEscalation();
    }

    // -----------------------------------------------------------------------
    // Feature 10: Triage escalation alert
    // -----------------------------------------------------------------------

    /// <summary>
    /// Counts Urgent-level appointments in the database.
    /// If 3 or more are found, shows a prominent red alert banner at the top of
    /// the dashboard so admin staff are immediately aware and can prioritise.
    /// </summary>
    private void CheckTriageEscalation()
    {
        try
        {
            using (SqlConnection conn = connectionManager.GetMembersConnection())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM [Appointments] WHERE AITriage = 'Urgent'", conn))
            {
                int urgentCount = (int)cmd.ExecuteScalar();
                if (urgentCount >= 3)
                {
                    PanelEscalation.Visible = true;
                    LblEscalation.Text =
                        "<strong>Triage Escalation Alert:</strong> There are currently <strong>" +
                        urgentCount + " Urgent</strong> appointment(s) requiring immediate attention. " +
                        "Please review the Appointments grid below.";
                }
                else
                {
                    PanelEscalation.Visible = false;
                }
            }
        }
        catch
        {
            PanelEscalation.Visible = false;
        }
    }

    // -----------------------------------------------------------------------
    // Feature 10: AI Dashboard Insights
    // -----------------------------------------------------------------------

    /// <summary>
    /// Handles the Refresh Insights button click.
    /// Builds a statistical summary of appointments and asks GPT-4 to produce
    /// a concise operational analysis. Result is HTML-encoded before rendering.
    /// </summary>
    protected void BtnRefreshInsights_Click(object sender, EventArgs e)
    {
        string summary = BuildAppointmentSummary();
        string insight = OpenAIService.GetAdminInsights(summary);
        LitInsights.Text = System.Web.HttpUtility.HtmlEncode(insight);
    }

    /// <summary>
    /// Queries the Appointments table to produce a plain-text statistical summary
    /// covering total count, per-service breakdown, and per-triage-level breakdown.
    /// </summary>
    private string BuildAppointmentSummary()
    {
        var sb = new StringBuilder();
        try
        {
            using (SqlConnection conn = connectionManager.GetMembersConnection())
            {
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [Appointments]", conn))
                    sb.AppendLine("Total appointments: " + cmd.ExecuteScalar());

                sb.AppendLine("\nAppointments by service:");
                using (var cmd = new SqlCommand(
                    "SELECT Services, COUNT(*) AS Cnt FROM [Appointments] GROUP BY Services ORDER BY Cnt DESC", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        sb.AppendLine("  " + rdr["Services"] + ": " + rdr["Cnt"]);
                }

                sb.AppendLine("\nAI Triage breakdown:");
                using (var cmd = new SqlCommand(
                    "SELECT AITriage, COUNT(*) AS Cnt FROM [Appointments] WHERE AITriage IS NOT NULL GROUP BY AITriage ORDER BY Cnt DESC", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        sb.AppendLine("  " + rdr["AITriage"] + ": " + rdr["Cnt"]);
                }

                sb.AppendLine("\nNo-Show Risk breakdown:");
                using (var cmd = new SqlCommand(
                    "SELECT NoShowRisk, COUNT(*) AS Cnt FROM [Appointments] WHERE NoShowRisk IS NOT NULL GROUP BY NoShowRisk ORDER BY Cnt DESC", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        sb.AppendLine("  " + rdr["NoShowRisk"] + ": " + rdr["Cnt"]);
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("(Could not load full data: " + ex.Message + ")");
        }
        return sb.ToString();
    }

    // -----------------------------------------------------------------------
    // Feature 6: Follow-up email drafter
    // -----------------------------------------------------------------------

    /// <summary>
    /// Handles the Generate Follow-Up button click.
    /// Reads the patient name, service, and AI note from the hidden fields (set by
    /// the row's select button) and calls GPT-4 to draft a personalised follow-up email.
    /// The draft is displayed in a result panel for the admin to copy and send.
    /// </summary>
    protected void BtnGenerateFollowUp_Click(object sender, EventArgs e)
    {
        string firstName = TxtFollowUpFirst.Text.Trim();
        string service   = TxtFollowUpService.Text.Trim();
        string aiNote    = TxtFollowUpNote.Text.Trim();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            LitFollowUp.Text      = "Please enter the patient's first name.";
            PanelFollowUp.Visible = true;
            return;
        }

        string draft          = OpenAIService.GetFollowUpEmail(firstName, service, aiNote);
        LitFollowUp.Text      = System.Web.HttpUtility.HtmlEncode(draft);
        PanelFollowUp.Visible = true;
    }

    // -----------------------------------------------------------------------
    // Feature 8: Doctor's note simplifier
    // -----------------------------------------------------------------------

    /// <summary>
    /// Handles the Simplify Note button click.
    /// Takes the clinical text pasted into TxtClinicalNote and returns a
    /// plain-English rewrite suitable for sharing with the patient.
    /// </summary>
    protected void BtnSimplifyNote_Click(object sender, EventArgs e)
    {
        string raw = TxtClinicalNote.Text.Trim();
        if (string.IsNullOrWhiteSpace(raw))
        {
            LitSimplifiedNote.Text      = "Please paste a clinical note above.";
            PanelSimplifiedNote.Visible = true;
            return;
        }

        string plain                = OpenAIService.SimplifyNote(raw);
        LitSimplifiedNote.Text      = System.Web.HttpUtility.HtmlEncode(plain);
        PanelSimplifiedNote.Visible = true;
    }

    // -----------------------------------------------------------------------
    // GridView: Appointments (triage + no-show colour coding)
    // -----------------------------------------------------------------------

    protected void GridView4_SelectedIndexChanged(object sender, EventArgs e) { }


    /// <summary>
    /// Colour-codes the AI Triage cell (col 6) and No-Show Risk cell (col 7) of
    /// each data row in the Appointments GridView.
    /// Also populates the hidden fields used by the Follow-Up Email drafter when
    /// a row is selected.
    ///
    /// Triage colours:
    ///   Urgent  — red    | High — amber | Medium — blue | Low — green
    ///
    /// No-Show Risk colours:
    ///   High — orange | Medium — yellow | Low — light green
    /// </summary>
    protected void GridView5_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        // Column 6 = AITriage
        string triage = e.Row.Cells[6].Text;
        switch (triage)
        {
            case "Urgent":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xF2, 0xDE, 0xDE);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0xA9, 0x44, 0x42);
                break;
            case "High":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xFC, 0xF8, 0xE3);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0x8A, 0x6D, 0x3B);
                break;
            case "Medium":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xD9, 0xEA, 0xF7);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0x31, 0x70, 0x8F);
                break;
            case "Low":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xDF, 0xF0, 0xD8);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0x3C, 0x76, 0x3D);
                break;
        }

        // Column 9 = NoShowRisk (added to GridView5 columns in markup)
        if (e.Row.Cells.Count > 9)
        {
            string risk = e.Row.Cells[9].Text;
            switch (risk)
            {
                case "High":
                    e.Row.Cells[9].BackColor = System.Drawing.Color.FromArgb(0xFF, 0xE0, 0xB2);
                    e.Row.Cells[9].ForeColor = System.Drawing.Color.FromArgb(0xE6, 0x51, 0x00);
                    break;
                case "Medium":
                    e.Row.Cells[9].BackColor = System.Drawing.Color.FromArgb(0xFF, 0xF9, 0xC4);
                    e.Row.Cells[9].ForeColor = System.Drawing.Color.FromArgb(0xF5, 0x7F, 0x17);
                    break;
                case "Low":
                    e.Row.Cells[9].BackColor = System.Drawing.Color.FromArgb(0xE8, 0xF5, 0xE9);
                    e.Row.Cells[9].ForeColor = System.Drawing.Color.FromArgb(0x2E, 0x7D, 0x32);
                    break;
            }
        }
    }

    // -----------------------------------------------------------------------
    // GridView: Contacts (sentiment colour coding)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Colour-codes the Sentiment cell in the Contacts GridView.
    ///   Urgent     — red    | Distressed — orange
    ///   Neutral    — grey   | Positive   — green
    /// </summary>
    protected void GridView4_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        // Sentiment is the last bound column in GridView4
        int lastCol = e.Row.Cells.Count - 1;
        if (lastCol < 0) return;

        string sentiment = e.Row.Cells[lastCol].Text;
        switch (sentiment)
        {
            case "Urgent":
                e.Row.Cells[lastCol].BackColor = System.Drawing.Color.FromArgb(0xF2, 0xDE, 0xDE);
                e.Row.Cells[lastCol].ForeColor = System.Drawing.Color.FromArgb(0xA9, 0x44, 0x42);
                break;
            case "Distressed":
                e.Row.Cells[lastCol].BackColor = System.Drawing.Color.FromArgb(0xFF, 0xE0, 0xB2);
                e.Row.Cells[lastCol].ForeColor = System.Drawing.Color.FromArgb(0xE6, 0x51, 0x00);
                break;
            case "Positive":
                e.Row.Cells[lastCol].BackColor = System.Drawing.Color.FromArgb(0xDF, 0xF0, 0xD8);
                e.Row.Cells[lastCol].ForeColor = System.Drawing.Color.FromArgb(0x3C, 0x76, 0x3D);
                break;
        }
    }
}
