using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Code-behind for the Symptom Diary page (SymptomDiary.aspx).
///
/// Feature 3 — Symptom Diary:
/// Patients log daily symptom entries (date + description) during a browser session.
/// When they click "Analyse", OpenAIService.AnalyseSymptomDiary reviews all entries
/// and returns a trend summary plus an urgency recommendation.
///
/// Entries are stored in Session["SymptomDiaryEntries"] as a JSON array, making
/// them persist across postbacks without requiring a database table.
/// </summary>
public partial class SymptomDiary : System.Web.UI.Page
{
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

    // -----------------------------------------------------------------------
    // Session helpers
    // -----------------------------------------------------------------------

    private List<DiaryEntry> GetEntries()
    {
        if (Session["SymptomDiaryEntries"] == null)
            return new List<DiaryEntry>();
        try
        {
            return Json.Deserialize<List<DiaryEntry>>(Session["SymptomDiaryEntries"].ToString());
        }
        catch { return new List<DiaryEntry>(); }
    }

    private void SaveEntries(List<DiaryEntry> entries)
    {
        Session["SymptomDiaryEntries"] = Json.Serialize(entries);
    }

    // -----------------------------------------------------------------------
    // Page lifecycle
    // -----------------------------------------------------------------------

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            TxtDate.Text = DateTime.Now.ToString("dd MMM yyyy");
            BindDiary();
        }
    }

    private void BindDiary()
    {
        var entries = GetEntries();
        if (entries.Count > 0)
        {
            RptEntries.DataSource = entries;
            RptEntries.DataBind();
            PanelDiary.Visible = true;
        }
        else
        {
            PanelDiary.Visible = false;
        }
    }

    // -----------------------------------------------------------------------
    // Event handlers
    // -----------------------------------------------------------------------

    /// <summary>Adds a new symptom entry to the session diary and refreshes the list.</summary>
    protected void BtnAddEntry_Click(object sender, EventArgs e)
    {
        string date  = TxtDate.Text.Trim();
        string entry = TxtEntry.Text.Trim();

        if (string.IsNullOrWhiteSpace(entry))
        {
            LblEntryError.Text    = "Please describe your symptoms before adding an entry.";
            LblEntryError.Visible = true;
            return;
        }

        LblEntryError.Visible = false;

        var entries = GetEntries();
        entries.Add(new DiaryEntry { Date = date, Entry = entry });
        SaveEntries(entries);

        TxtEntry.Text = string.Empty;
        PanelAnalysis.Visible = false;
        BindDiary();
    }

    /// <summary>
    /// Sends all diary entries to GPT-4 and displays the trend analysis.
    /// </summary>
    protected void BtnAnalyse_Click(object sender, EventArgs e)
    {
        var entries = GetEntries();
        if (entries.Count == 0)
        {
            LblEntryError.Text    = "Add at least one diary entry before analysing.";
            LblEntryError.Visible = true;
            return;
        }

        // Build a plain-text log for GPT-4
        var sb = new System.Text.StringBuilder();
        foreach (var entry in entries)
            sb.AppendLine(entry.Date + ": " + entry.Entry);

        OpenAIService.SymptomDiaryResult result = OpenAIService.AnalyseSymptomDiary(sb.ToString());

        LitSummary.Text        = System.Web.HttpUtility.HtmlEncode(result.Summary);
        LitRecommendation.Text = System.Web.HttpUtility.HtmlEncode(result.Recommendation);
        PanelAnalysis.Visible  = true;
    }

    /// <summary>Clears all diary entries from the session.</summary>
    protected void BtnClearDiary_Click(object sender, EventArgs e)
    {
        Session.Remove("SymptomDiaryEntries");
        PanelAnalysis.Visible = false;
        BindDiary();
    }

    // -----------------------------------------------------------------------
    // Helper model
    // -----------------------------------------------------------------------

    public class DiaryEntry
    {
        public string Date  { get; set; }
        public string Entry { get; set; }
    }
}
