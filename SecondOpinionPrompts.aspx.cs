using System;
using System.Web.UI;

/// <summary>
/// Code-behind for the Second Opinion Question Generator (SecondOpinionPrompts.aspx).
///
/// Feature 34 — Second Opinion Question Generator:
///   The patient pastes a diagnosis or treatment plan. GPT-4 generates 8-10
///   intelligent questions grouped into three sections: Understanding the Diagnosis,
///   Treatment Options, and Next Steps &amp; Lifestyle. The result panel includes
///   a Print button. All output is HTML-encoded to prevent XSS from pasted content.
/// </summary>
public partial class SecondOpinionPrompts : System.Web.UI.Page
{
    /// <summary>Standard page lifecycle handler. No initialisation required on first load.</summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Generate My Questions button click.
    /// Validates the diagnosis field, calls GetSecondOpinionQuestions, and shows the result panel.
    /// </summary>
    protected void BtnGenerate_Click(object sender, EventArgs e)
    {
        string diagnosis = TxtDiagnosis.Text.Trim();
        if (string.IsNullOrWhiteSpace(diagnosis))
        {
            LblError.Text    = "Please paste your diagnosis or treatment plan before generating questions.";
            LblError.Visible = true;
            return;
        }
        LblError.Visible = false;

        string questions    = OpenAIService.GetSecondOpinionQuestions(diagnosis);
        LitQuestions.Text   = System.Web.HttpUtility.HtmlEncode(questions);
        PanelForm.Visible   = false;
        PanelResult.Visible = true;
    }

    /// <summary>Clears the diagnosis input and returns the page to the entry form.</summary>
    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtDiagnosis.Text   = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
