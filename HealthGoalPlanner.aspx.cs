using System;
using System.Web.UI;

public partial class HealthGoalPlanner : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

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

    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtGoal.Text        = "";
        TxtConditions.Text  = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
