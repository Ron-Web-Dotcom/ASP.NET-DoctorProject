using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

public partial class RegisterationForm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        string FirstName = TextBox1.Text;
        string LastName  = TextBox2.Text;
        string Age       = TextBox3.Text;
        string Services  = DropDownList1.SelectedValue;
        string PhoneNum  = TextBox4.Text;
        string Time      = DropDownList2.SelectedValue;
        string Email     = TextBox8.Text;
        string Address1  = TextBox5.Text;
        string Address2  = TextBox6.Text;
        string City      = TextBox7.Text;
        string Issue     = TextBox9.Text;

        // AI triage: assess the patient's intake before saving
        OpenAIService.TriageResult triage = OpenAIService.GetTriage(
            FirstName, LastName, Age, Services, Issue);

        SqlConnection connection = connectionManager.GetMembersConnection();
        SqlCommand myCommand = new SqlCommand();
        myCommand.Connection = connection;

        string insertSQL =
            "INSERT INTO [Appointments] " +
            "(FirstName, Lastname, Age, Services, PhoneNum, Time, Email, Address1, Address2, City, Issue, AITriage, AINote) " +
            "VALUES " +
            "(@FirstName, @LastName, @Age, @Services, @PhoneNum, @Time, @Email, @Address1, @Address2, @City, @Issue, @AITriage, @AINote)";

        myCommand.CommandText = insertSQL;
        myCommand.Parameters.AddWithValue("@FirstName", FirstName);
        myCommand.Parameters.AddWithValue("@LastName",  LastName);
        myCommand.Parameters.AddWithValue("@Age",       Age);
        myCommand.Parameters.AddWithValue("@Services",  Services);
        myCommand.Parameters.AddWithValue("@PhoneNum",  PhoneNum);
        myCommand.Parameters.AddWithValue("@Time",      Time);
        myCommand.Parameters.AddWithValue("@Email",     Email);
        myCommand.Parameters.AddWithValue("@Address1",  Address1);
        myCommand.Parameters.AddWithValue("@Address2",  Address2);
        myCommand.Parameters.AddWithValue("@City",      City);
        myCommand.Parameters.AddWithValue("@Issue",     Issue);
        myCommand.Parameters.AddWithValue("@AITriage",  triage.Triage);
        myCommand.Parameters.AddWithValue("@AINote",    triage.Note);

        myCommand.ExecuteNonQuery();
    }


    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        TextBox10.Text = Calendar1.SelectedDate.ToLongDateString();
        Calendar1.Visible = false;
    }

    protected void Button3_Click(object sender, EventArgs e)
    {
        Calendar1.Visible = true;
    }

    protected void Calendar1_SelectionChanged1(object sender, EventArgs e)
    {
        TextBox10.Text = Calendar1.SelectedDate.ToString("dd MMMM,yyyy");
        Calendar1.Visible = true;
    }
}
