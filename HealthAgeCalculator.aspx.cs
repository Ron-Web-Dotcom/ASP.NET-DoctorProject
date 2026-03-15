using System;
using System.Web.UI;

/// <summary>
/// Code-behind for HealthAgeCalculator.aspx.
/// Collects six lifestyle indicators from the patient and calls
/// <see cref="OpenAIService.GetHealthAge"/> to produce a GPT-4 health age
/// estimate with personalised improvement tips.
/// </summary>
public partial class HealthAgeCalculator : Page
{
    /// <summary>
    /// Standard page load handler. No data-binding required on load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e) { }

    /// <summary>
    /// Handles the Calculate My Health Age button click.
    /// Validates that an actual age has been entered, passes all lifestyle
    /// inputs to GPT-4, HTML-encodes the result for XSS safety, and
    /// reveals the result panel.
    /// </summary>
    protected void BtnCalculate_Click(object sender, EventArgs e)
    {
        string age = TxtAge.Text.Trim();

        if (string.IsNullOrWhiteSpace(age))
        {
            LblError.Text    = "Please enter your actual age to continue.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        string smoking  = DdlSmoking.SelectedValue;
        string exercise = DdlExercise.SelectedValue;
        string diet     = DdlDiet.SelectedValue;
        string sleep    = DdlSleep.SelectedValue;
        string stress   = DdlStress.SelectedValue;

        string result = OpenAIService.GetHealthAge(age, smoking, exercise, diet, sleep, stress)
                        ?? "Unable to calculate your health age at this time. Please try again later.";

        LitResult.Text      = System.Web.HttpUtility.HtmlEncode(result);
        PanelResult.Visible = true;
    }
}
