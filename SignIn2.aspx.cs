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
/// Code-behind for the patient Sign In page (SignIn2.aspx).
/// Authenticates registered users against the Users table using a
/// SHA-256 hashed password comparison, then redirects to the Contact Form.
/// </summary>
public partial class SignIn2 : System.Web.UI.Page
{
    /// <summary>
    /// On first load, restores a saved email address from the Remember Me cookie
    /// if one exists. The password field is never pre-filled for security reasons.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            HttpCookie usercookie = Request.Cookies["data1"];
            if (usercookie != null)
            {
                // Restore only the email; never restore a password from a cookie
                TextBox1.Text      = usercookie["emailaddress"].ToString();
                RememberMe.Checked = true;
            }
            else
            {
                TextBox1.Text      = "";
                TextBox2.Text      = "";
                RememberMe.Checked = false;
            }
        }
    }

    /// <summary>
    /// Handles the Remember Me checkbox toggle.
    /// Saves only the email address in a short-lived cookie (10 minutes).
    /// The password is intentionally excluded to prevent credential exposure.
    /// </summary>
    protected void RememberMe_CheckedChanged(object sender, EventArgs e)
    {
        HttpCookie userdata = new HttpCookie("data1");
        userdata["emailaddress"] = TextBox1.Text;
        // Password is intentionally not stored in the cookie
        userdata.Expires = System.DateTime.Now.AddMinutes(10);
        Response.Cookies.Add(userdata);
    }

    /// <summary>
    /// Computes a lowercase hex SHA-256 hash of the supplied plain-text password.
    /// This must match the hashing applied when the password was stored during sign-up.
    /// </summary>
    /// <param name="password">The plain-text password entered by the user.</param>
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
    /// Handles the Sign In button click.
    /// Hashes the entered password and compares it against the stored hash in the
    /// Users table using a parameterised query to prevent SQL injection.
    /// On success, saves the user's name in session and redirects to ContactForm.aspx.
    /// On failure, displays an error label.
    /// </summary>
    protected void Button1_Click1(object sender, EventArgs e)
    {
        string Email          = TextBox1.Text;
        string HashedPassword = HashPassword(TextBox2.Text);

        using (SqlConnection connection = connectionManager.GetMembersConnection())
        using (SqlCommand myCommand = new SqlCommand("SELECT * FROM Users WHERE Email=@Email AND password=@Password", connection))
        {
            myCommand.Parameters.AddWithValue("@Email",    Email);
            myCommand.Parameters.AddWithValue("@Password", HashedPassword);

            using (SqlDataReader read = myCommand.ExecuteReader())
            {
                if (read.Read())
                {
                    // Store user identity in session for use on subsequent pages
                    Session["FirstName"] = read["FirstName"].ToString();
                    Session["LastName"]  = read["LastName"].ToString();
                    Session["Email"]     = Email;

                    Response.Redirect("MyAppointments.aspx");
                }
                else
                {
                    Label1.Text = "Invalid email or password.";
                }
            }
        }
    }
}
