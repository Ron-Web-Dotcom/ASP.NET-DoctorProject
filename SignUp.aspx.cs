using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;

public partial class SignUp : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    private static string HashPassword(string password)
    {
        using (var sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        try
        {
            string FirstName = TextBox1.Text;
            string LastName  = TextBox2.Text;
            string Email     = TextBox3.Text;
            string Password  = HashPassword(TextBox4.Text);
            string Month     = DropDownList1.SelectedValue;
            string Day       = DropDownList2.SelectedValue;
            string Year      = DropDownList3.SelectedValue;

            using (SqlConnection connection = connectionManager.GetMembersConnection())
            using (SqlCommand myCommand = new SqlCommand())
            {
                myCommand.Connection  = connection;
                myCommand.CommandText = "INSERT INTO [Users] (FirstName, LastName, Email, Password, Month, Day, Year) VALUES (@FirstName, @LastName, @Email, @Password, @Month, @Day, @Year)";
                myCommand.Parameters.AddWithValue("@FirstName", FirstName);
                myCommand.Parameters.AddWithValue("@LastName",  LastName);
                myCommand.Parameters.AddWithValue("@Email",     Email);
                myCommand.Parameters.AddWithValue("@Password",  Password);
                myCommand.Parameters.AddWithValue("@Month",     Month);
                myCommand.Parameters.AddWithValue("@Day",       Day);
                myCommand.Parameters.AddWithValue("@Year",      Year);

                myCommand.ExecuteNonQuery();
            }

            Response.Redirect("AppointmentForm.aspx");
        }
        catch (SqlException)
        {
            Label1.Text = "An error occurred. Please try again.";
        }
    }
}
