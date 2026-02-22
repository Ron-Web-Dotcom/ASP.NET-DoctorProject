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

/// <summary>
/// Code-behind for the patient registration page (SignUp.aspx).
/// Creates a new user account in the Users table. Passwords are hashed
/// with SHA-256 before being stored so plain-text credentials are never
/// persisted to the database.
/// </summary>
public partial class SignUp : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Computes a lowercase hex SHA-256 hash of the supplied plain-text password.
    /// Must be kept consistent with the matching method in SignIn2.aspx.cs so
    /// that login comparisons succeed.
    /// </summary>
    /// <param name="password">The plain-text password chosen by the user.</param>
    /// <returns>A 64-character lowercase hex string representing the SHA-256 digest.</returns>
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

    /// <summary>
    /// Handles the Register button click.
    /// Hashes the chosen password and inserts the new user record into the
    /// Users table using parameterised queries to prevent SQL injection.
    /// On success, redirects the user to the Appointment Form.
    /// On a database error (e.g. duplicate email), displays a generic error message.
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        try
        {
            string FirstName = TextBox1.Text;
            string LastName  = TextBox2.Text;
            string Email     = TextBox3.Text;
            string Password  = HashPassword(TextBox4.Text); // never store plain-text passwords
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
            // A SqlException typically means a constraint violation (e.g. duplicate email)
            Label1.Text = "An error occurred. Please try again.";
        }
    }
}
