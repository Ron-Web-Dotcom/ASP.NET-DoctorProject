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

public partial class SignIn2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            HttpCookie usercookie = Request.Cookies["data1"];
            if (usercookie != null)
            {
                TextBox1.Text = usercookie["emailaddress"].ToString();
                RememberMe.Checked = true;
            }
            else
            {
                TextBox1.Text = "";
                TextBox2.Text = "";
                RememberMe.Checked = false;
            }
        }
    }

    protected void RememberMe_CheckedChanged(object sender, EventArgs e)
    {
        HttpCookie userdata = new HttpCookie("data1");
        userdata["emailaddress"] = TextBox1.Text;
        // Password is intentionally not stored in the cookie
        userdata.Expires = System.DateTime.Now.AddMinutes(10);
        Response.Cookies.Add(userdata);
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
                    Session["FirstName"] = read["FirstName"].ToString();
                    Session["LastName"]  = read["LastName"].ToString();

                    Response.Redirect("ContactForm.aspx");
                }
                else
                {
                    Label1.Text = "Invalid email or password.";
                }
            }
        }
    }
}
