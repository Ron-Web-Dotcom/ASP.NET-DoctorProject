using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the AI Diet &amp; Lifestyle Planner (LifestylePlanner.aspx).
///
/// Feature 26 — AI Diet &amp; Lifestyle Planner:
///   Patient selects their upcoming specialty and optionally provides their age
///   and any other conditions. GPT-4 generates a personalised plan covering
///   diet, exercise, and general lifestyle tips for that specialty.
/// </summary>
public partial class LifestylePlanner : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string specialty   = DdlSpecialty.SelectedValue;
        string age         = TxtAge.Text.Trim();
        string conditions  = TxtConditions.Text.Trim();

        if (string.IsNullOrWhiteSpace(specialty))
        {
            LblError.Text    = "Please select a specialty before generating your plan.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string plan = OpenAIService.GetLifestylePlan(specialty, age, conditions);

        LitSpecialty.Text = System.Web.HttpUtility.HtmlEncode(specialty);
        LitPlan.Text      = System.Web.HttpUtility.HtmlEncode(plan);
        PanelResult.Visible = true;
    }
}
