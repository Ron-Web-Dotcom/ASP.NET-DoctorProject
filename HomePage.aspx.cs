using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the clinic home page (HomePage.aspx).
/// On each page load, retrieves a GPT-4 generated daily health tip.
/// The tip is cached in Application state under a date-stamped key
/// (e.g. "HealthTip_2026-03-16") so the same tip is served to all visitors
/// for the entire day, and a fresh tip is generated on the first request of
/// each new calendar day. The rendered tip is HTML-encoded to prevent XSS.
/// </summary>
public partial class HomePage : System.Web.UI.Page
{
    /// <summary>
    /// Retrieves the daily health tip from Application cache, or generates a
    /// new one via GPT-4 if today's entry is absent. Writes the HTML-encoded
    /// result into the <c>LitHealthTip</c> Literal control.
    /// </summary>
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

