<%@ Page Title="Appointment Feedback" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="AppointmentFeedback.aspx.cs" Inherits="AppointmentFeedback" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .fb-hero { background:linear-gradient(135deg,#e67e22,#a04000); color:#fff; padding:30px 20px 22px; margin-bottom:28px; border-radius:0 0 6px 6px; }
    .fb-hero h2 { margin:0 0 4px; font-size:26px; }
    .fb-hero p  { margin:0; opacity:.85; font-size:14px; }
    .card { background:#fff; border:1px solid #dde3ea; border-radius:6px; padding:24px; margin-bottom:24px; box-shadow:0 1px 4px rgba(0,0,0,.06); }
    .card h4 { margin-top:0; color:#e67e22; border-bottom:1px solid #eef1f5; padding-bottom:10px; }
    /* Star rating */
    .star-group { display:flex; flex-direction:row-reverse; justify-content:flex-end; margin-bottom:10px; }
    .star-group input[type=radio] { display:none; }
    .star-group label {
        font-size:36px; color:#ccc; cursor:pointer;
        transition:color .15s; padding:0 4px;
    }
    .star-group input:checked ~ label,
    .star-group label:hover,
    .star-group label:hover ~ label { color:#f39c12; }
    .thankyou-box { text-align:center; padding:40px 20px; }
    .thankyou-box .glyphicon { font-size:56px; color:#27ae60; display:block; margin-bottom:12px; }
</style>

<div class="fb-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-star"></span>&nbsp;Share Your Feedback</h2>
        <p>We value your experience. Rate your recent visit and leave a comment — your feedback helps us improve care for every patient.</p>
    </div>
</div>

<div class="container">

    <!-- Feedback form -->
    <asp:Panel ID="PanelForm" runat="server">
        <div class="card">
            <h4><span class="glyphicon glyphicon-pencil"></span> Your Feedback</h4>
            <div class="row">
                <div class="col-sm-4">
                    <div class="form-group">
                        <label>Your Name</label>
                        <asp:TextBox ID="TxtName" runat="server" CssClass="form-control" placeholder="First and last name" />
                    </div>
                </div>
                <div class="col-sm-4">
                    <div class="form-group">
                        <label>Email Address</label>
                        <asp:TextBox ID="TxtEmail" runat="server" CssClass="form-control" placeholder="your@email.com" TextMode="Email" />
                    </div>
                </div>
                <div class="col-sm-4">
                    <div class="form-group">
                        <label>Service Visited</label>
                        <asp:DropDownList ID="DdlService" runat="server" CssClass="form-control">
                            <asp:ListItem Value="">-- Select a service --</asp:ListItem>
                            <asp:ListItem>Cardiology</asp:ListItem>
                            <asp:ListItem>General Practitioner</asp:ListItem>
                            <asp:ListItem>Gynaecology</asp:ListItem>
                            <asp:ListItem>Opticology</asp:ListItem>
                            <asp:ListItem>Paediatrics</asp:ListItem>
                            <asp:ListItem>Radiology</asp:ListItem>
                            <asp:ListItem>Surgery</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </div>

            <!-- Star Rating -->
            <div class="form-group">
                <label>Overall Rating</label><br />
                <div class="star-group">
                    <asp:RadioButton ID="Star5" runat="server" GroupName="Rating" Value="5" /><label for="<%= Star5.ClientID %>">&#9733;</label>
                    <asp:RadioButton ID="Star4" runat="server" GroupName="Rating" Value="4" /><label for="<%= Star4.ClientID %>">&#9733;</label>
                    <asp:RadioButton ID="Star3" runat="server" GroupName="Rating" Value="3" /><label for="<%= Star3.ClientID %>">&#9733;</label>
                    <asp:RadioButton ID="Star2" runat="server" GroupName="Rating" Value="2" /><label for="<%= Star2.ClientID %>">&#9733;</label>
                    <asp:RadioButton ID="Star1" runat="server" GroupName="Rating" Value="1" /><label for="<%= Star1.ClientID %>">&#9733;</label>
                </div>
            </div>

            <div class="form-group">
                <label>Your Comments</label>
                <asp:TextBox ID="TxtComment" runat="server" TextMode="MultiLine" Rows="5"
                    CssClass="form-control"
                    placeholder="Tell us about your experience — what went well, what could be improved..." />
            </div>

            <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
            <br />
            <asp:Button ID="BtnSubmit" runat="server" Text="Submit Feedback"
                CssClass="btn btn-lg" style="background:#e67e22;color:#fff;border:none;"
                OnClick="BtnSubmit_Click" />
        </div>
    </asp:Panel>

    <!-- Thank you panel -->
    <asp:Panel ID="PanelThankyou" runat="server" Visible="false">
        <div class="card">
            <div class="thankyou-box">
                <span class="glyphicon glyphicon-ok-circle"></span>
                <h3>Thank You for Your Feedback!</h3>
                <p class="lead">Your response has been recorded and will be reviewed by our team.</p>
                <div class="alert alert-info" style="max-width:500px;margin:16px auto 0;text-align:left;">
                    <strong>AI Sentiment Analysis:</strong><br />
                    <asp:Literal ID="LitSentimentBadge" runat="server" />
                    &nbsp;<asp:Literal ID="LitSentimentReason" runat="server" />
                </div>
                <div style="margin-top:20px;">
                    <a href="HomePage.aspx" class="btn btn-primary">Return to Home</a>
                    <a href="AppointmentForm.aspx" class="btn btn-default" style="margin-left:8px;">Book Another Appointment</a>
                </div>
            </div>
        </div>
    </asp:Panel>

</div>
</asp:Content>
