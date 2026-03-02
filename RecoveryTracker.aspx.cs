using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class RecoveryTracker : System.Web.UI.Page
{
    private const string SessionKey = "RecoveryEntries";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            BindEntries();
    }

    protected void BtnAddEntry_Click(object sender, EventArgs e)
    {
        string date      = TxtDate.Text.Trim();
        string note      = TxtNote.Text.Trim();
        string procedure = TxtProcedure.Text.Trim();

        if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(note))
        {
            LblError.Text    = "Please enter both a date and a recovery note.";
            LblError.Visible = true;
            return;
        }
        LblError.Visible = false;

        var entries = GetEntries();
        entries.Add(new RecoveryEntry { Date = date, Note = note, Procedure = procedure });
        Session[SessionKey] = entries;

        TxtDate.Text = "";
        TxtNote.Text = "";
        BindEntries();
        PanelResult.Visible = false;
    }

    protected void BtnClear_Click(object sender, EventArgs e)
    {
        Session.Remove(SessionKey);
        TxtProcedure.Text   = "";
        TxtDate.Text        = "";
        TxtNote.Text        = "";
        PanelResult.Visible = false;
        BindEntries();
    }

    protected void BtnAnalyse_Click(object sender, EventArgs e)
    {
        var entries = GetEntries();
        if (entries.Count == 0) return;

        string procedure = TxtProcedure.Text.Trim();
        if (string.IsNullOrWhiteSpace(procedure) && entries.Count > 0)
            procedure = entries[0].Procedure;
        if (string.IsNullOrWhiteSpace(procedure)) procedure = "the procedure";

        var sb = new System.Text.StringBuilder();
        foreach (var entry in entries)
            sb.AppendLine(entry.Date + ": " + entry.Note);

        string result = OpenAIService.AnalyseRecovery(sb.ToString(), procedure);

        // Determine recommendation badge
        string badge;
        if (result.IndexOf("contact your doctor promptly", StringComparison.OrdinalIgnoreCase) >= 0)
            badge = "<div class='rec-urgent'><span class='glyphicon glyphicon-warning-sign'></span> Please contact your doctor promptly</div>";
        else if (result.IndexOf("contact your care team", StringComparison.OrdinalIgnoreCase) >= 0)
            badge = "<div class='rec-contact'><span class='glyphicon glyphicon-exclamation-sign'></span> Consider contacting your care team</div>";
        else
            badge = "<div class='rec-on-track'><span class='glyphicon glyphicon-ok-circle'></span> Recovery appears on track</div>";

        LitRecommendBadge.Text = badge;
        LitResult.Text         = System.Web.HttpUtility.HtmlEncode(result);
        PanelResult.Visible    = true;
    }

    private void BindEntries()
    {
        var entries = GetEntries();
        LitCount.Text          = entries.Count.ToString();
        PanelNoEntries.Visible = entries.Count == 0;
        RptEntries.DataSource  = entries;
        RptEntries.DataBind();
        PanelAnalyseBtn.Visible = entries.Count >= 2;
    }

    private List<RecoveryEntry> GetEntries()
    {
        return Session[SessionKey] as List<RecoveryEntry> ?? new List<RecoveryEntry>();
    }

    public class RecoveryEntry
    {
        public string Date      { get; set; }
        public string Note      { get; set; }
        public string Procedure { get; set; }
    }
}
