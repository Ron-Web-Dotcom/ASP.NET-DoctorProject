using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

public partial class Form : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        SqlConnection connection = connectionManager.GetMembersConnection();

        SqlCommand myCommand = new SqlCommand();
        myCommand.Connection = connection;
        string FirstName = TextBox1.Text;
        string LastName = TextBox2.Text;
        string Email = TextBox3.Text;
        string PhoneNum = TextBox4.Text;
        string Message = TextBox5.Text;

        string insertSQL = "INSERT INTO [Contacts] (FirstName, Lastname, Email, PhoneNum, Message) VALUES (@FirstName, @LastName, @Email, @PhoneNum, @Message)";
        myCommand.CommandText = insertSQL;
        myCommand.Parameters.AddWithValue("@FirstName", FirstName);
        myCommand.Parameters.AddWithValue("@LastName", LastName);
        myCommand.Parameters.AddWithValue("@Email", Email);
        myCommand.Parameters.AddWithValue("@PhoneNum", PhoneNum);
        myCommand.Parameters.AddWithValue("@Message", Message);

        myCommand.ExecuteNonQuery();

        // Generate AI-drafted reply and show thank-you panel
        string aiReply = OpenAIService.GetContactReply(FirstName, Message);
        LitAIReply.Text       = System.Web.HttpUtility.HtmlEncode(aiReply);
        PanelForm.Visible     = false;
        PanelThankyou.Visible = true;
    }
}
