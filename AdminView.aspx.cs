using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class AdminView : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["FirstName"] == null)
            Response.Redirect("Admin.aspx");
    }

    protected void GridView4_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    protected void BtnRefreshInsights_Click(object sender, EventArgs e)
    {
        string summary = BuildAppointmentSummary();
        string insight = OpenAIService.GetAdminInsights(summary);
        LitInsights.Text = System.Web.HttpUtility.HtmlEncode(insight);
    }

    private string BuildAppointmentSummary()
    {
        var sb = new StringBuilder();
        try
        {
            using (SqlConnection conn = connectionManager.GetMembersConnection())
            {
                // Total appointments
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [Appointments]", conn))
                {
                    int total = (int)cmd.ExecuteScalar();
                    sb.AppendLine("Total appointments: " + total);
                }

                // Breakdown by service
                sb.AppendLine("\nAppointments by service:");
                using (var cmd = new SqlCommand("SELECT Services, COUNT(*) AS Cnt FROM [Appointments] GROUP BY Services ORDER BY Cnt DESC", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        sb.AppendLine("  " + rdr["Services"] + ": " + rdr["Cnt"]);
                }

                // Triage breakdown
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
    /// Colour-codes the AI Triage cell in the Appointments grid using Bootstrap contextual row classes.
    /// Urgent = red, High = orange/warning, Medium = blue/info, Low = green.
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
