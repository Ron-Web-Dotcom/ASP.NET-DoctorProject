using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

public partial class AdminSignUpaspx : System.Web.UI.Page
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
        string TypeOFUser = DropDownList1.SelectedValue;
        string Email = TextBox3.Text;
        string Password = TextBox4.Text;
        string Confirm = TextBox5.Text;

        string insertSQL = "INSERT INTO [Admins] (FirstName, LastName, TypeOFUser, Email, Password, Confirm) VALUES (@FirstName, @LastName, @TypeOFUser, @Email, @Password, @Confirm)";
        myCommand.CommandText = insertSQL;
        myCommand.Parameters.AddWithValue("@FirstName", FirstName);
        myCommand.Parameters.AddWithValue("@LastName", LastName);
        myCommand.Parameters.AddWithValue("@TypeOFUser", TypeOFUser);
        myCommand.Parameters.AddWithValue("@Email", Email);
        myCommand.Parameters.AddWithValue("@Password", Password);
        myCommand.Parameters.AddWithValue("@Confirm", Confirm);

        myCommand.ExecuteNonQuery();
    }
}
