using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

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
                TextBox2.Text = usercookie["password"].ToString();
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
        userdata["password"] = TextBox2.Text;
        userdata.Expires = System.DateTime.Now.AddMinutes(10);
        Response.Cookies.Add(userdata);
    }



    protected void Button1_Click1(object sender, EventArgs e)
    {

        SqlConnection connection = connectionManager.GetMembersConnection();

        SqlCommand myCommand = new SqlCommand();
        myCommand.Connection = connection;
        string Email = TextBox1.Text;
        string Password = TextBox2.Text;

        string insertSQL = "SELECT * FROM Users WHERE Email=@Email AND password=@Password";
        myCommand.CommandText = insertSQL;
        myCommand.Parameters.AddWithValue("@Email", Email);
        myCommand.Parameters.AddWithValue("@Password", Password);

        SqlDataReader read = myCommand.ExecuteReader();

        if (read.Read())
        {
            Session["FirstName"] = read["FirstName"].ToString();
            Session["LastName"] = read["LastName"].ToString();

            Response.Redirect("ContactForm.aspx");
        }
        else
        {
            Label1.Text = "Invalid email or password.";
        }
    }

}
