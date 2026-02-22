<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="AppointmentForm.aspx.cs" Inherits="RegisterationForm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
     <div id="banner">
     <div class="row">
    <div class="large-12 large-columns centered">

    <div id="page_masthead_block">
         <div class="large-12 small-12">
         
   <center><img class="page_masthead_image" src="images/appointment-banner1.jpg" alt="Opticology" width="900" height="250"></center>
    </div>
  </div>
   </div>
    </div>
</div>
     <div class="well">
<center><h1><block>Portmore Medical Center Appointment Page</block></h1></center>
        </div>
         <div class="container">

<!-- Confirmation panel — shown after successful booking -->
<asp:Panel ID="PanelConfirmation" runat="server" Visible="false">
  <div class="panel panel-success" style="margin:20px 0;">
    <div class="panel-heading"><h3 class="panel-title">Appointment Confirmed!</h3></div>
    <div class="panel-body">
      <p>Thank you, <strong><asp:Label ID="LblConfirmName" runat="server" /></strong>!
         Your <strong><asp:Label ID="LblConfirmService" runat="server" /></strong> appointment
         at <strong><asp:Label ID="LblConfirmTime" runat="server" /></strong> has been booked.</p>
      <hr />
      <h4><span class="glyphicon glyphicon-heart"></span> Personalised Wellness Tips</h4>
      <p style="white-space:pre-line;"><asp:Literal ID="LitWellnessTips" runat="server" /></p>
      <a href="AppointmentForm.aspx" class="btn btn-default">Book Another Appointment</a>
      &nbsp;
      <a href="HomePage.aspx" class="btn btn-primary">Return to Home</a>
    </div>
  </div>
</asp:Panel>

<!-- Main booking form panel -->
<asp:Panel ID="PanelForm" runat="server" Visible="true">
        <table class="nav-justified">
                 <tr>
                     <td style="width: 396px">FirstName :<asp:TextBox ID="TextBox1" runat="server" Width="248px"></asp:TextBox>
                     </td>
                     <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Select Service:
                         <asp:DropDownList ID="DropDownList1" runat="server" Height="16px" Width="166px">
                             <asp:ListItem>Services Offer</asp:ListItem>
                             <asp:ListItem>Cardiology</asp:ListItem>
                             <asp:ListItem>General Practitioner</asp:ListItem>
                             <asp:ListItem>Gynaecology</asp:ListItem>
                             <asp:ListItem>Opticology</asp:ListItem>
                             <asp:ListItem>Paediatrician</asp:ListItem>
                             <asp:ListItem>Radiology</asp:ListItem>
                             <asp:ListItem>Surgeon</asp:ListItem>
                         </asp:DropDownList>
                     </td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px; height: 20px"></td>
                     <td style="height: 20px"></td>
                 </tr>
                 <tr>
                     <td style="width: 396px">LastName:<asp:TextBox ID="TextBox2" runat="server" Width="255px"></asp:TextBox>
                     </td>
                     <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Select Time:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                         <asp:DropDownList ID="DropDownList2" runat="server" Height="16px" Width="166px">
                             <asp:ListItem>Time Offer </asp:ListItem>
                             <asp:ListItem>8am to9am</asp:ListItem>
                             <asp:ListItem>9amto10am</asp:ListItem>
                             <asp:ListItem>11am to12pm</asp:ListItem>
                             <asp:ListItem>12pm to 1pm</asp:ListItem>
                             <asp:ListItem>1pm to 2pm</asp:ListItem>
                             <asp:ListItem>2pm to 3pm</asp:ListItem>
                             <asp:ListItem>3pm to 4pm</asp:ListItem>
                             <asp:ListItem>4pm to 5pm</asp:ListItem>
                             <asp:ListItem>5pm to 6pm</asp:ListItem>
                             <asp:ListItem>6pm to 7pm</asp:ListItem>
                             <asp:ListItem>7pm to 8pm</asp:ListItem>
                             <asp:ListItem>8pm to 9pm</asp:ListItem>
                             <asp:ListItem>9pm to 10pm</asp:ListItem>
                         </asp:DropDownList>
                     </td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Age:&nbsp;
                         <asp:TextBox ID="TextBox3" runat="server" Width="93px"></asp:TextBox>
                     </td>
                     <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Phone Number :
                         <asp:TextBox ID="TextBox4" runat="server" Width="221px"></asp:TextBox>
                     </td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px">Adresss:
                         <asp:TextBox ID="TextBox5" runat="server" Width="253px"></asp:TextBox>
                     </td>
                     <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Email Address:<asp:TextBox ID="TextBox8" runat="server" Width="259px"></asp:TextBox>
                     </td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;<asp:TextBox ID="TextBox6" runat="server" Width="255px"></asp:TextBox>
                     </td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;<asp:TextBox ID="TextBox7" runat="server" CssClass="col-xs-offset-0" Width="252px"></asp:TextBox>
                     </td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px"><div>
        Appointment Calender <asp:TextBox ID="TextBox10" runat="server" Width="151px"></asp:TextBox><asp:Button ID="Button3" runat="server" Text="Calender" OnClick="Button3_Click" />
                        <center><asp:Calendar ID="Calendar1" runat="server" BackColor="White" BorderColor="#999999" CellPadding="4" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt" ForeColor="Black" Height="180px" Width="200px" OnSelectionChanged="Calendar1_SelectionChanged1">
                             <DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
                             <NextPrevStyle VerticalAlign="Bottom" />
                             <OtherMonthDayStyle ForeColor="#808080" />
                             <SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
                             <SelectorStyle BackColor="#CCCCCC" />
                             <TitleStyle BackColor="#999999" BorderColor="Black" Font-Bold="True" />
                             <TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
                             <WeekendDayStyle BackColor="#FFFFCC" />
                         </asp:Calendar></center>
                     </div>
               </td>
                     <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Reason For Appointment<asp:TextBox ID="TextBox9" runat="server" Height="82px" Width="279px"></asp:TextBox>
                     </td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td style="width: 396px">&nbsp;</td>
                     <td>&nbsp;</td>
                 </tr>
                 <tr>
                     <td colspan="2">
                         <asp:Label ID="LblDuplicate" runat="server" Visible="false"
                             CssClass="alert alert-warning" style="display:block;margin-top:10px;" />
                     </td>
                 </tr>
                 <tr>
                     <td style="width: 396px">
                         <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Submit" Width="135px" />
                     </td>
                     <td>&nbsp;</td>
                 </tr>
             </table>
</asp:Panel><!-- /PanelForm -->

  <footer>
        <div class="container">
            <p class="pull-right"><a href="#">Go Back Upht"><a href="#">Go Back Up</a></p>
            <p class="pull-down">&copy; 2011 MedicalCenter.com &middot;<a href="HomePage.aspx">Home</a>&middot;<a href="#">About Us</a>&middot;<a href="#">Service</a>&middot;<a href="#">Bio</a></p>
        </div>
    </footer>
     <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
</asp:Content>

