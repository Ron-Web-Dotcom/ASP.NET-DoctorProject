using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class AdminView : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void GridView4_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Colour-codes the AI Triage cell in the Appointments grid using Bootstrap contextual row classes.
    /// Urgent = red, High = orange/warning, Medium = blue/info, Low = green.
    /// </summary>
    protected void GridView5_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        // AITriage is the 7th column (index 6)
        string triage = e.Row.Cells[6].Text;

        switch (triage)
        {
            case "Urgent":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xF2, 0xDE, 0xDE);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0xA9, 0x44, 0x42);
                break;
            case "High":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xFC, 0xF8, 0xE3);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0x8A, 0x6D, 0x3B);
                break;
            case "Medium":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xD9, 0xEA, 0xF7);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0x31, 0x70, 0x8F);
                break;
            case "Low":
                e.Row.Cells[6].BackColor = System.Drawing.Color.FromArgb(0xDF, 0xF0, 0xD8);
                e.Row.Cells[6].ForeColor = System.Drawing.Color.FromArgb(0x3C, 0x76, 0x3D);
                break;
        }
    }
}
