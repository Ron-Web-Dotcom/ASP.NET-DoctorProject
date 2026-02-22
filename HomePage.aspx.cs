using System;
using System.Web.UI;

public partial class HomePage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string todayKey = "HealthTip_" + DateTime.Now.ToString("yyyy-MM-dd");

        // Use Application cache so the tip is generated once per day, not on every request
        string tip = Application[todayKey] as string;
        if (string.IsNullOrEmpty(tip))
        {
            tip = OpenAIService.GetHealthTip();
            Application[todayKey] = tip;
        }

        LitHealthTip.Text = System.Web.HttpUtility.HtmlEncode(tip);
    }
}
