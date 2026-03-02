using System;
using System.Web.UI;

public partial class SecondOpinionPrompts : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

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

    protected void BtnReset_Click(object sender, EventArgs e)
    {
        TxtDiagnosis.Text   = "";
        PanelForm.Visible   = true;
        PanelResult.Visible = false;
    }
}
