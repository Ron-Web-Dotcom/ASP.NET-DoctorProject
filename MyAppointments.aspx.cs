using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Code-behind for the Patient Dashboard (MyAppointments.aspx).
///
/// Feature — Patient Appointment Dashboard:
///   Displays all appointments linked to the logged-in patient's email address.
///   Each row shows the service, time slot, AI triage level, AI pre-assessment note,
///   and no-show risk. Patients can request an AI preparation tip for any appointment
///   with a single button click.
///
/// Session guard: redirects to SignIn2.aspx if the user is not logged in.
/// </summary>
public partial class MyAppointments : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Session guard — redirect to login if the patient is not authenticated
        if (Session["Email"] == null || string.IsNullOrWhiteSpace(Session["Email"].ToString()))
        {
            Response.Redirect("SignIn2.aspx");
            return;
        }

        if (!IsPostBack)
        {
            LitName.Text = System.Web.HttpUtility.HtmlEncode(
                Session["FirstName"] + " " + Session["LastName"]);

            LoadAppointments();
        }
    }

    /// <summary>
    /// Queries the Appointments table for all rows where Email matches the
    /// session email, then binds the result to the Repeater.
    /// </summary>
    private void LoadAppointments()
    {
        string email = Session["Email"].ToString();

        using (SqlConnection conn = connectionManager.GetMembersConnection())
        using (SqlCommand cmd = new SqlCommand(
            "SELECT FirstName, Lastname, Services, Time, AITriage, AINote, NoShowRisk " +
            "FROM [Appointments] WHERE Email = @Email " +
            "ORDER BY (SELECT NULL)", conn))
        {
            cmd.Parameters.AddWithValue("@Email", email);

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    PanelNoAppts.Visible = true;
                    PanelGrid.Visible    = false;
                }
                else
                {
                    PanelGrid.Visible    = true;
                    PanelNoAppts.Visible = false;
                    RptAppointments.DataSource = dt;
                    RptAppointments.DataBind();
                }
            }
        }
    }

    /// <summary>
    /// Handles the "Get AI Tip" button inside the Repeater.
    /// Calls GPT-4 to generate a personalised preparation tip for the selected
    /// appointment (service + time slot passed via CommandArgument).
    /// </summary>
    protected void RptAppointments_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "GetTip") return;

        string arg      = e.CommandArgument.ToString();
        string[] parts  = arg.Split('|');
        string service  = parts.Length > 0 ? parts[0] : "";
        string timeSlot = parts.Length > 1 ? parts[1] : "";

        string tip = OpenAIService.GetAppointmentPreparationTip(service, timeSlot);

        LitPreparationTip.Text = System.Web.HttpUtility.HtmlEncode(tip);
        PanelTip.Visible = true;

        // Re-bind the repeater so the page renders correctly after postback
        LoadAppointments();
    }

    /// <summary>
    /// Returns a colour-coded HTML badge for the given triage level.
    /// Safe to call from the ASPX inline expression <%# GetTriageBadge(...) %>.
    /// </summary>
    public string GetTriageBadge(string triage)
    {
        if (string.IsNullOrWhiteSpace(triage)) return "<span>—</span>";
        switch (triage.Trim().ToLower())
        {
            case "urgent": return "<span class='triage-urgent'>Urgent</span>";
            case "high":   return "<span class='triage-high'>High</span>";
            case "medium": return "<span class='triage-medium'>Medium</span>";
            case "low":    return "<span class='triage-low'>Low</span>";
            default:       return "<span>" + System.Web.HttpUtility.HtmlEncode(triage) + "</span>";
        }
    }

    /// <summary>
    /// Returns a colour-coded HTML badge for the given no-show risk level.
    /// Safe to call from the ASPX inline expression <%# GetRiskBadge(...) %>.
    /// </summary>
    public string GetRiskBadge(string risk)
    {
        if (string.IsNullOrWhiteSpace(risk)) return "<span>—</span>";
        switch (risk.Trim().ToLower())
        {
            case "high":   return "<span class='risk-high'>High</span>";
            case "medium": return "<span class='risk-medium'>Medium</span>";
            case "low":    return "<span class='risk-low'>Low</span>";
            default:       return "<span>" + System.Web.HttpUtility.HtmlEncode(risk) + "</span>";
        }
    }
}
