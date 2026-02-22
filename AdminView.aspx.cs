using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Code-behind for the Admin Dashboard page (AdminView.aspx).
/// Displays appointment data in a GridView, colour-codes rows by AI triage
/// urgency level, and provides an AI-powered operational insights panel
/// that summarises appointment statistics on demand.
/// Access is restricted to authenticated administrators via session guard.
/// </summary>
public partial class AdminView : System.Web.UI.Page
{
    /// <summary>
    /// Enforces authentication on every request.
    /// If the admin session is absent the visitor is redirected to the
    /// login page (Admin.aspx), preventing unauthorised access to the dashboard.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["FirstName"] == null)
            Response.Redirect("Admin.aspx");
    }

    protected void GridView4_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// Handles the Refresh Insights button click.
    /// Builds a plain-text summary of current appointment statistics from the
    /// database and passes it to OpenAIService.GetAdminInsights to generate
    /// a concise AI-written operational analysis for the admin team.
    /// The result is HTML-encoded before rendering to prevent XSS.
    /// </summary>
    protected void BtnRefreshInsights_Click(object sender, EventArgs e)
    {
        string summary = BuildAppointmentSummary();
        string insight = OpenAIService.GetAdminInsights(summary);
        LitInsights.Text = System.Web.HttpUtility.HtmlEncode(insight);
    }

    /// <summary>
    /// Queries the Appointments table to produce a plain-text statistical summary
    /// covering total appointment count, a breakdown by service, and a breakdown
    /// by AI triage level. This summary is sent to GPT-4 for analysis.
    /// Returns a partial summary with an error note if the database query fails.
    /// </summary>
    /// <returns>A multi-line string containing appointment statistics.</returns>
    private string BuildAppointmentSummary()
    {
        var sb = new StringBuilder();
        try
        {
            using (SqlConnection conn = connectionManager.GetMembersConnection())
            {
                // Total appointment count
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [Appointments]", conn))
                {
                    int total = (int)cmd.ExecuteScalar();
                    sb.AppendLine("Total appointments: " + total);
                }

                // Per-service breakdown, most popular first
                sb.AppendLine("\nAppointments by service:");
                using (var cmd = new SqlCommand("SELECT Services, COUNT(*) AS Cnt FROM [Appointments] GROUP BY Services ORDER BY Cnt DESC", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        sb.AppendLine("  " + rdr["Services"] + ": " + rdr["Cnt"]);
                }

                // AI triage level breakdown (excludes rows with no triage value)
                sb.AppendLine("\nAI Triage breakdown:");
                using (var cmd = new SqlCommand("SELECT AITriage, COUNT(*) AS Cnt FROM [Appointments] WHERE AITriage IS NOT NULL GROUP BY AITriage ORDER BY Cnt DESC", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        sb.AppendLine("  " + rdr["AITriage"] + ": " + rdr["Cnt"]);
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("(Could not load full data: " + ex.Message + ")");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Colour-codes the AI Triage cell (column index 6) of each data row in the
    /// Appointments GridView using Bootstrap-compatible contextual colours:
    ///   Urgent  — red background / dark-red text
    ///   High    — amber background / dark-amber text
    ///   Medium  — blue background / dark-blue text
    ///   Low     — green background / dark-green text
    /// Non-data rows (header, footer) are skipped.
    /// </summary>
    protected void GridView5_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        // AITriage is the 7th column (index 6)
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
    }
}
