using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Code-behind for the Post-Treatment Recovery Tracker (RecoveryTracker.aspx).
///
/// Feature 35 — Post-Treatment Recovery Tracker:
///   Patients log daily free-text recovery notes after a procedure. Entries are
///   stored in ASP.NET Session for the browser session lifetime.
///   Once 2+ entries exist the Analyse button appears. GPT-4 returns a recovery
///   trend, positive signs, areas to watch, and a recommendation phrase which
///   drives a colour-coded badge (green / amber / red).
///   All output is HTML-encoded. The disclaimer reminds patients this is not
///   a clinical assessment.
/// </summary>
public partial class RecoveryTracker : System.Web.UI.Page
{
    /// <summary>Session key used to persist recovery log entries across postbacks.</summary>
    private const string SessionKey = "RecoveryEntries";

    /// <summary>
    /// On first load, binds any existing session entries to the Repeater so
    /// entries survive navigation within the same session.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            BindEntries();
    }

    /// <summary>
    /// Appends a new entry to the session list after validating date and note fields.
    /// Clears date and note inputs but retains the procedure field for subsequent entries.
    /// </summary>
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

    /// <summary>Removes all recovery entries from session and resets all form fields.</summary>
    protected void BtnClear_Click(object sender, EventArgs e)
    {
        Session.Remove(SessionKey);
        TxtProcedure.Text   = "";
        TxtDate.Text        = "";
        TxtNote.Text        = "";
        PanelResult.Visible = false;
        BindEntries();
    }

    /// <summary>
    /// Sends all recovery entries to GPT-4 and renders the trend analysis result.
    /// Resolves the procedure name from the form field, the first entry, or a generic
    /// fallback. Scans the GPT-4 response for the recommendation phrase to select the
    /// appropriate colour-coded badge (red / amber / green).
    /// </summary>
    protected void BtnAnalyse_Click(object sender, EventArgs e)
    {
        var entries = GetEntries();
        if (entries.Count == 0) return;

        // Resolve procedure name: form field → first entry's stored procedure → fallback
        string procedure = TxtProcedure.Text.Trim();
        if (string.IsNullOrWhiteSpace(procedure))
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

    /// <summary>
    /// Refreshes the Repeater, entry count badge, and visibility of the no-entries
    /// placeholder and Analyse button. The Analyse button only appears with 2+ entries
    /// so GPT-4 has sufficient data to identify a trend.
    /// </summary>
    private void BindEntries()
    {
        var entries = GetEntries();
        LitCount.Text          = entries.Count.ToString();
        PanelNoEntries.Visible = entries.Count == 0;
        RptEntries.DataSource  = entries;
        RptEntries.DataBind();
        PanelAnalyseBtn.Visible = entries.Count >= 2;
    }

    /// <summary>
    /// Retrieves the current recovery log from session.
    /// Returns an empty list when no entries have been added yet.
    /// </summary>
    private List<RecoveryEntry> GetEntries()
    {
        return Session[SessionKey] as List<RecoveryEntry> ?? new List<RecoveryEntry>();
    }

    /// <summary>Represents a single dated entry in the patient's recovery log.</summary>
    public class RecoveryEntry
    {
        /// <summary>Patient-supplied date label (e.g. "Day 3 post-op" or "02 Mar 2026").</summary>
        public string Date      { get; set; }

        /// <summary>Free-text description of how the patient is feeling on this date.</summary>
        public string Note      { get; set; }

        /// <summary>The procedure being tracked; stored on first entry and used as GPT-4 context.</summary>
        public string Procedure { get; set; }
    }
}
