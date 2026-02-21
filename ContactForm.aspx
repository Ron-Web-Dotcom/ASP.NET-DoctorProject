<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ContactForm.aspx.cs" Inherits="Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="banner">
     <div class="row">
    <div class="large-12 large-columns centered">

    <div id="page_masthead_block">
         <div class="large-12 small-12">
         
   <center><img class="page_masthead_image" src="images/imagrrrrrres.jpg"alt="Contact" width="900" height="250"></center>
    </div>
  </div>
   </div>
    </div>
</div>
     <div class="well">
         <center><h1><block>Contact Form for Portmore Medical Center</block></h1></center>
        <div class="container text-center">
            <p>Please use the form below to get in touch with us if you have any general questions or any comments or suggestions about this website. Please DO NOT use this form to request or send any kind of medical information.</p>
            </div>
         </div>
    <!-- Shown after successful submission -->
    <asp:Panel ID="PanelThankyou" runat="server" Visible="false" style="max-width:640px;margin:30px auto;">
        <div class="alert alert-success">
            <h4><span class="glyphicon glyphicon-ok-circle"></span> Thank you for contacting us!</h4>
            <p>Your message has been received. Here is a draft reply our team will be sending you:</p>
        </div>
        <div class="panel panel-default">
            <div class="panel-heading"><strong><span class="glyphicon glyphicon-comment"></span> AI-Drafted Reply</strong></div>
            <div class="panel-body" style="white-space:pre-wrap;">
                <asp:Literal ID="LitAIReply" runat="server" />
            </div>
        </div>
        <a href="HomePage.aspx" class="btn btn-primary">Return to Home</a>
    </asp:Panel>

    <!-- Contact form (hidden after submit) -->
    <asp:Panel ID="PanelForm" runat="server">
          <table class="nav-justified">
                <tr>
                    <td style="width: 429px">FirstName :<asp:TextBox ID="TextBox1" runat="server" Width="257px"></asp:TextBox>
                    </td>
                    <td>Email:<asp:TextBox ID="TextBox3" runat="server" Width="257px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="width: 429px">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td style="width: 429px">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td style="width: 429px">LastName:<asp:TextBox ID="TextBox2" runat="server" Width="257px"></asp:TextBox>
                    </td>
                    <td>Phone Number
                        <asp:TextBox ID="TextBox4" runat="server" Height="22px" Width="257px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="width: 429px">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td style="width: 429px">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
                <tr>
                    <td style="width: 429px">
                        <asp:Button ID="Button1" runat="server" Text="Button" Width="84px" OnClick="Button1_Click" />
&nbsp;<asp:Button ID="Button2" runat="server" Text="Button" Width="84px" />
                    </td>
                    <td>Message :<asp:TextBox ID="TextBox5" runat="server" Height="110px" Width="247px"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td style="width: 429px">&nbsp;</td>
                    <td>&nbsp;</td>
                </tr>
            </table>
    <footer>
        <p class="pull-right"><a href="#">Go Back Up</a></p>
            <p class="pull-down">&copy; 2011 MedicalCenter.com &middot;<a href="HomePage.aspx">Home</a>&middot;<a href="#">About Us</a>&middot;<a href="#">Service</a>&middot;<a href="#">Bio</a></p>
        </div>
    </footer>
    </asp:Panel><%-- close PanelForm --%>
     <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>  
 
</asp:Content>

