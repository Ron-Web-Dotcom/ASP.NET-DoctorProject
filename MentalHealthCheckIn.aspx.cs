using System;
using System.Web.UI;

public partial class MentalHealthCheckIn : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnCheckIn_Click(object sender, EventArgs e)
    {
        string mood    = DdlMood.SelectedValue;
        string sleep   = DdlSleep.SelectedValue;
        string anxiety = DdlAnxiety.SelectedValue;
        string energy  = DdlEnergy.SelectedValue;
        string social  = DdlSocial.SelectedValue;
        string notes   = TxtNotes.Text.Trim();

        string result       = OpenAIService.GetMentalHealthCheckIn(mood, sleep, anxiety, energy, social, notes);
        LitResult.Text      = System.Web.HttpUtility.HtmlEncode(result);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtNotes.Text       = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
