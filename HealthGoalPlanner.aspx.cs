using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Health Goal Planner (HealthGoalPlanner.aspx).
///
/// Feature 33 — Health Goal Planner:
///   The patient describes a health goal (e.g. "lose 10kg", "lower blood pressure"),
///   selects their current activity level, and optionally lists existing conditions.
///   GPT-4 generates a structured 4-week action plan with week-by-week steps and
///   a Tips for Success section. Output is HTML-encoded before display.
/// </summary>
public partial class HealthGoalPlanner : System.Web.UI.Page
{
    /// <summary>Standard page lifecycle handler. No initialisation required on first load.</summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Build My 4-Week Plan button click.
    /// Validates the goal field, calls GetHealthGoalPlan, and switches to the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string goal       = TxtGoal.Text.Trim();
        string activity   = DdlActivity.SelectedValue;
        string conditions = TxtConditions.Text.Trim();

        if (string.IsNullOrWhiteSpace(goal))
        {
            LblError.Text    = "Please describe your health goal before generating a plan.";
            LblError.Visible = true;
            return;
        }
        LblError.Visible = false;

        string plan         = OpenAIService.GetHealthGoalPlan(goal, activity, conditions);
        LitGoal.Text        = System.Web.HttpUtility.HtmlEncode(goal);
        LitPlan.Text        = System.Web.HttpUtility.HtmlEncode(plan);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    /// <summary>
    /// Handles the Plan a Different Goal button click.
    /// Clears goal and conditions inputs and returns to the form panel.
    /// Activity level retains its previous value for convenience.
    /// </summary>
    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtGoal.Text        = "";
        TxtConditions.Text  = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
