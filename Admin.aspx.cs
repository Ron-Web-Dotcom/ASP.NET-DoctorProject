using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

/// <summary>
/// Code-behind for the Admin login page (Admin.aspx).
/// Authenticates clinic administrators against the Admins table and
/// establishes a named session upon success.
/// </summary>
public partial class admin : System.Web.UI.Page
{
    /// <summary>
    /// On first load, restores a saved email address from the Remember Me cookie
    /// if one exists, so the admin does not have to retype it.
    /// The password is never pre-filled from a cookie for security reasons.
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
    /// Handles the Login button click.
    /// Looks up the supplied email and password in the Admins table using
    /// parameterised queries to prevent SQL injection.
    /// On success, stores the admin's name in session and redirects to
    /// AdminView.aspx. On failure, displays an error label.
    /// </summary>
    protected void Button1_Click(object sender, EventArgs e)
    {
        string Email    = TextBox1.Text;
        string Password = TextBox2.Text;

        using (SqlConnection connection = connectionManager.GetMembersConnection())
        using (SqlCommand myCommand = new SqlCommand("SELECT * FROM Admins WHERE Email=@Email AND password=@Password", connection))
        {
            myCommand.Parameters.AddWithValue("@Email",    Email);
            myCommand.Parameters.AddWithValue("@Password", Password);

            using (SqlDataReader read = myCommand.ExecuteReader())
            {
                if (read.Read())
                {
                    // Store admin identity in session for use across protected pages
                    Session["FirstName"] = read["FirstName"].ToString();
                    Session["LastName"]  = read["LastName"].ToString();

                    Response.Redirect("AdminView.aspx");
                }
                else
                {
                    Label1.Text = "Invalid email or password.";
                }
            }
        }
    }

    /// <summary>
    /// Handles the Remember Me checkbox toggle.
    /// Saves only the email address in a short-lived cookie (10 minutes).
    /// The password is intentionally excluded from the cookie to prevent
    /// credential exposure if the cookie is intercepted or inspected.
    /// </summary>
    protected void RememberMe_CheckedChanged(object sender, EventArgs e)
    {
        HttpCookie userdata = new HttpCookie("data1");
        userdata["emailaddress"] = TextBox1.Text;
        // Password is intentionally not stored in the cookie
        userdata.Expires = System.DateTime.Now.AddMinutes(10);
        Response.Cookies.Add(userdata);
    }
}
