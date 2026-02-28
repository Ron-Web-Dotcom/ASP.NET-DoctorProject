using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Code-behind for the Doctor Availability Viewer (DoctorAvailability.aspx).
///
/// Feature 28 — Doctor Availability Viewer:
///   Patient selects a specialty. The page looks up which doctors cover that
///   department and calls GPT-4 to generate a short, patient-friendly summary
///   of each doctor's focus areas. Results are bound to a Repeater.
/// </summary>
public partial class DoctorAvailability : System.Web.UI.Page
{
    // Static doctor roster keyed by specialty.
    // Each entry: Name, Specialty, typical time slots.
    private static readonly Dictionary<string, List<DoctorInfo>> Roster =
        new Dictionary<string, List<DoctorInfo>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Cardiology", new List<DoctorInfo> {
                new DoctorInfo("Dr. Michael Knapton",   "Cardiology",          "9:00 AM – 10:00 AM,11:00 AM – 12:00 PM,2:00 PM – 3:00 PM"),
                new DoctorInfo("Dr. Patrick H Maxwell", "Cardiology",          "10:00 AM – 11:00 AM,3:00 PM – 4:00 PM") }},
            { "General Practitioner", new List<DoctorInfo> {
                new DoctorInfo("Dr. Mike More",         "General Practitioner","9:00 AM – 10:00 AM,11:00 AM – 12:00 PM,2:00 PM – 3:00 PM,4:00 PM – 5:00 PM"),
                new DoctorInfo("Dr. Shirley Pointer",   "General Practitioner","10:00 AM – 11:00 AM,1:00 PM – 2:00 PM,3:00 PM – 4:00 PM") }},
            { "Gynaecology", new List<DoctorInfo> {
                new DoctorInfo("Dr. Ann-Marie Ingle",   "Gynaecology",         "9:00 AM – 10:00 AM,11:00 AM – 12:00 PM,2:00 PM – 3:00 PM"),
                new DoctorInfo("Dr. Evelyn Barker",     "Gynaecology",         "10:00 AM – 11:00 AM,1:00 PM – 2:00 PM") }},
            { "Opticology", new List<DoctorInfo> {
                new DoctorInfo("Dr. Roland Siker",      "Opticology",          "9:00 AM – 10:00 AM,11:00 AM – 12:00 PM,2:00 PM – 3:00 PM,3:00 PM – 4:00 PM") }},
            { "Paediatrics", new List<DoctorInfo> {
                new DoctorInfo("Dr. Sharon Peacock",    "Paediatrics",         "9:00 AM – 10:00 AM,10:00 AM – 11:00 AM,2:00 PM – 3:00 PM") }},
            { "Radiology", new List<DoctorInfo> {
                new DoctorInfo("Dr. Kate Lancaster",    "Radiology",           "9:00 AM – 10:00 AM,11:00 AM – 12:00 PM,1:00 PM – 2:00 PM") }},
            { "Surgery", new List<DoctorInfo> {
                new DoctorInfo("Dr. Jag Ahluwalia",     "Surgery",             "10:00 AM – 11:00 AM,2:00 PM – 3:00 PM,4:00 PM – 5:00 PM") }},
        };

    protected void Page_Load(object sender, EventArgs e) { }

    protected void BtnView_Click(object sender, EventArgs e)
    {
        string specialty = DdlSpecialty.SelectedValue;

        if (string.IsNullOrWhiteSpace(specialty))
        {
            LblError.Text    = "Please select a specialty.";
            LblError.Visible = true;
            return;
        }

        LblError.Visible = false;

        if (!Roster.ContainsKey(specialty))
        {
            LblError.Text    = "No doctors found for the selected specialty.";
            LblError.Visible = true;
            return;
        }

        var doctors = Roster[specialty];

        // Enrich each doctor entry with a GPT-4 focus summary
        var rows = new List<object>();
        foreach (var doc in doctors)
        {
            string summary = OpenAIService.GetDoctorFocusSummary(doc.Name, doc.Specialty);
            rows.Add(new { doc.Name, doc.Specialty, doc.Slots, AISummary = summary });
        }

        LitSpecialty.Text       = System.Web.HttpUtility.HtmlEncode(specialty);
        RptDoctors.DataSource   = rows;
        RptDoctors.DataBind();
        PanelResults.Visible    = true;
    }

    /// <summary>
    /// Converts a comma-separated slot string into HTML list items for the Repeater.
    /// Called from the ASPX inline expression.
    /// </summary>
    public string GetSlotBadges(string slots)
    {
        if (string.IsNullOrWhiteSpace(slots)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (string slot in slots.Split(','))
            sb.AppendFormat("<li>{0}</li>", System.Web.HttpUtility.HtmlEncode(slot.Trim()));
        return sb.ToString();
    }

    // Simple data holder
    private class DoctorInfo
    {
        public string Name     { get; }
        public string Specialty{ get; }
        public string Slots    { get; }
        public DoctorInfo(string name, string specialty, string slots)
        { Name = name; Specialty = specialty; Slots = slots; }
    }
}
