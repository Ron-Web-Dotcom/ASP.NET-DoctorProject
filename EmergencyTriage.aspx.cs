using System;
using System.Web.UI;

public partial class EmergencyTriage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnTriage_Click(object sender, EventArgs e)
    {
        string symptoms = TxtSymptoms.Text.Trim();
        if (string.IsNullOrWhiteSpace(symptoms))
        {
            LblError.Text    = "Please describe your symptoms before submitting.";
            LblError.Visible = true;
            return;
        }
        LblError.Visible = false;

        string result = OpenAIService.GetEmergencyTriage(symptoms);

        // Extract the first line to determine the level badge
        string level = "";
        if (result.IndexOf("EMERGENCY", StringComparison.OrdinalIgnoreCase) >= 0 &&
            result.IndexOf("Level: EMERGENCY", StringComparison.OrdinalIgnoreCase) >= 0)
            level = "emergency";
        else if (result.IndexOf("Level: URGENT", StringComparison.OrdinalIgnoreCase) >= 0)
            level = "urgent";
        else if (result.IndexOf("Level: APPOINTMENT", StringComparison.OrdinalIgnoreCase) >= 0)
            level = "appointment";
        else
            level = "selfcare";

        string[] badges = {
            "<div class='level-emergency'><span class='glyphicon glyphicon-warning-sign'></span> EMERGENCY — Call 999 immediately</div>",
            "<div class='level-urgent'><span class='glyphicon glyphicon-exclamation-sign'></span> URGENT — Visit A&amp;E or urgent care today</div>",
            "<div class='level-appointment'><span class='glyphicon glyphicon-calendar'></span> APPOINTMENT — Book with your GP or specialist</div>",
            "<div class='level-selfcare'><span class='glyphicon glyphicon-home'></span> SELF-CARE — Manageable at home</div>"
        };

        switch (level)
        {
            case "emergency":   LitLevelBadge.Text = badges[0]; break;
            case "urgent":      LitLevelBadge.Text = badges[1]; break;
            case "appointment": LitLevelBadge.Text = badges[2]; break;
            default:            LitLevelBadge.Text = badges[3]; break;
        }

        LitResult.Text      = System.Web.HttpUtility.HtmlEncode(result);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtSymptoms.Text    = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
