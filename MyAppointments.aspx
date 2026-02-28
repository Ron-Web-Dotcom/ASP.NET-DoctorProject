<%@ Page Title="My Appointments" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="MyAppointments.aspx.cs" Inherits="MyAppointments" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<style type="text/css">
    .dashboard-header {
        background: linear-gradient(135deg, #2c6fad 0%, #1a4a7a 100%);
        color: #fff;
        padding: 30px 20px 20px 20px;
        margin-bottom: 28px;
        border-radius: 0 0 6px 6px;
    }
    .dashboard-header h2 { margin: 0 0 4px 0; font-size: 26px; }
    .dashboard-header p  { margin: 0; opacity: .85; font-size: 14px; }
    .section-card {
        background: #fff;
        border: 1px solid #dde3ea;
        border-radius: 6px;
        padding: 22px 24px;
        margin-bottom: 24px;
        box-shadow: 0 1px 4px rgba(0,0,0,.06);
    }
    .section-card h4 {
        margin-top: 0;
        color: #2c6fad;
        border-bottom: 1px solid #eef1f5;
        padding-bottom: 10px;
        margin-bottom: 16px;
    }
    .triage-urgent  { background:#F2DEDE; color:#A94442; padding:3px 8px; border-radius:4px; font-weight:bold; }
    .triage-high    { background:#FCF8E3; color:#8A6D3B; padding:3px 8px; border-radius:4px; font-weight:bold; }
    .triage-medium  { background:#D9EDF7; color:#31708F; padding:3px 8px; border-radius:4px; font-weight:bold; }
    .triage-low     { background:#DFF0D8; color:#3C763D; padding:3px 8px; border-radius:4px; font-weight:bold; }
    .risk-high      { background:#FFEBCC; color:#E65100; padding:3px 8px; border-radius:4px; }
    .risk-medium    { background:#FFF9C4; color:#7B6200; padding:3px 8px; border-radius:4px; }
    .risk-low       { background:#E8F5E9; color:#2E7D32; padding:3px 8px; border-radius:4px; }
    .tip-box {
        background:#F0F7FF;
        border-left:4px solid #2c6fad;
        padding:10px 14px;
        border-radius:0 4px 4px 0;
        font-size:13px;
        margin-top:6px;
        display:none;
    }
    .btn-tip { font-size:12px; padding:2px 10px; }
    .no-appts { text-align:center; padding:40px 20px; color:#888; }
    .no-appts .glyphicon { font-size:48px; display:block; margin-bottom:14px; }
    .signout-link { float:right; margin-top:6px; color:#fff; opacity:.8; font-size:13px; }
    .signout-link:hover { opacity:1; color:#fff; }
    table.appt-table { width:100%; border-collapse:collapse; font-size:13.5px; }
    table.appt-table th { background:#f4f7fb; color:#2c6fad; border-bottom:2px solid #dde3ea; padding:10px 12px; text-align:left; }
    table.appt-table td { padding:10px 12px; border-bottom:1px solid #eef1f5; vertical-align:top; }
    table.appt-table tr:last-child td { border-bottom:none; }
    table.appt-table tr:hover td { background:#fafcff; }
    .ai-note-text { color:#555; font-style:italic; font-size:12px; max-width:220px; }
</style>

<div class="dashboard-header">
    <div class="container">
        <asp:HyperLink ID="LnkSignOut" runat="server" CssClass="signout-link"
            NavigateUrl="SignIn2.aspx">
            <span class="glyphicon glyphicon-log-out"></span> Sign Out
        </asp:HyperLink>
        <h2>
            <span class="glyphicon glyphicon-calendar"></span>&nbsp;
            My Appointments
        </h2>
        <p>Welcome, <strong><asp:Literal ID="LitName" runat="server" /></strong> &mdash; here are all your bookings at Portmore Medical Center.</p>
    </div>
</div>

<div class="container">

    <!-- Action bar -->
    <div class="section-card" style="padding:14px 20px;">
        <a href="AppointmentForm.aspx" class="btn btn-primary">
            <span class="glyphicon glyphicon-plus"></span> Book New Appointment
        </a>
        <a href="Questionnaire.aspx" class="btn btn-default" style="margin-left:8px;">
            <span class="glyphicon glyphicon-list-alt"></span> Pre-Appointment Questionnaire
        </a>
        <a href="MedicationChecker.aspx" class="btn btn-default" style="margin-left:8px;">
            <span class="glyphicon glyphicon-plus-sign"></span> Medication Checker
        </a>
        <a href="SymptomDiary.aspx" class="btn btn-default" style="margin-left:8px;">
            <span class="glyphicon glyphicon-pencil"></span> Symptom Diary
        </a>
    </div>

    <!-- Appointments table -->
    <div class="section-card">
        <h4><span class="glyphicon glyphicon-th-list"></span> Your Bookings</h4>

        <!-- No appointments message (shown when grid is empty) -->
        <asp:Panel ID="PanelNoAppts" runat="server" Visible="false">
            <div class="no-appts">
                <span class="glyphicon glyphicon-calendar"></span>
                <p><strong>No appointments found.</strong></p>
                <p>You have not booked any appointments yet, or your bookings may be under a different email address.</p>
                <a href="AppointmentForm.aspx" class="btn btn-primary">Book Your First Appointment</a>
            </div>
        </asp:Panel>

        <!-- Appointments grid -->
        <asp:Panel ID="PanelGrid" runat="server" Visible="false">
            <div style="overflow-x:auto;">
                <asp:Repeater ID="RptAppointments" runat="server"
                    OnItemCommand="RptAppointments_ItemCommand">
                    <HeaderTemplate>
                        <table class="appt-table">
                            <thead>
                                <tr>
                                    <th>Service</th>
                                    <th>Time Slot</th>
                                    <th>Triage</th>
                                    <th>AI Pre-Assessment Note</th>
                                    <th>No-Show Risk</th>
                                    <th>AI Prep Tip</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <strong><%# System.Web.HttpUtility.HtmlEncode(Eval("Services").ToString()) %></strong><br />
                                <span style="color:#888;font-size:12px;">
                                    <%# System.Web.HttpUtility.HtmlEncode(
                                        Eval("FirstName").ToString() + " " + Eval("Lastname").ToString()) %>
                                </span>
                            </td>
                            <td><%# System.Web.HttpUtility.HtmlEncode(Eval("Time").ToString()) %></td>
                            <td>
                                <%# GetTriageBadge(Eval("AITriage").ToString()) %>
                            </td>
                            <td>
                                <span class="ai-note-text">
                                    <%# System.Web.HttpUtility.HtmlEncode(
                                        string.IsNullOrWhiteSpace(Eval("AINote").ToString())
                                            ? "—" : Eval("AINote").ToString()) %>
                                </span>
                            </td>
                            <td>
                                <%# GetRiskBadge(Eval("NoShowRisk").ToString()) %>
                            </td>
                            <td>
                                <asp:Button ID="BtnTip" runat="server"
                                    Text="Get AI Tip"
                                    CssClass="btn btn-info btn-tip"
                                    CommandName="GetTip"
                                    CommandArgument='<%# Eval("Services").ToString() + "|" + Eval("Time").ToString() %>' />
                                <div id='tip_<%# Container.ItemIndex %>' class="tip-box">
                                    <asp:Literal ID="LitTip" runat="server" />
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
    </div>

    <!-- AI Preparation Tip result (shown after button click) -->
    <asp:Panel ID="PanelTip" runat="server" Visible="false" CssClass="section-card">
        <h4><span class="glyphicon glyphicon-flash"></span> AI Preparation Tip</h4>
        <div class="alert alert-info" style="margin-bottom:0;">
            <span class="glyphicon glyphicon-info-sign"></span>&nbsp;
            <asp:Literal ID="LitPreparationTip" runat="server" />
        </div>
    </asp:Panel>

    <!-- Quick links -->
    <div class="row">
        <div class="col-sm-4">
            <div class="section-card" style="text-align:center;">
                <span class="glyphicon glyphicon-remove-circle" style="font-size:32px;color:#c0392b;"></span>
                <h5 style="margin:10px 0 6px;">Need to Cancel?</h5>
                <p style="font-size:12px;color:#777;margin-bottom:12px;">
                    Use our cancellation assistant to get a rescheduling message and tips for rebooking.
                </p>
                <a href="CancelAppointment.aspx" class="btn btn-danger btn-sm">Cancel / Reschedule</a>
            </div>
        </div>
        <div class="col-sm-4">
            <div class="section-card" style="text-align:center;">
                <span class="glyphicon glyphicon-comment" style="font-size:32px;color:#2980b9;"></span>
                <h5 style="margin:10px 0 6px;">Have Questions?</h5>
                <p style="font-size:12px;color:#777;margin-bottom:12px;">
                    Chat with our AI health assistant or check for medication interactions.
                </p>
                <a href="AIChatAssistant.aspx" class="btn btn-info btn-sm">AI Chat Assistant</a>
            </div>
        </div>
        <div class="col-sm-4">
            <div class="section-card" style="text-align:center;">
                <span class="glyphicon glyphicon-envelope" style="font-size:32px;color:#27ae60;"></span>
                <h5 style="margin:10px 0 6px;">Contact Us</h5>
                <p style="font-size:12px;color:#777;margin-bottom:12px;">
                    Send a message to our team. We analyse every enquiry and respond promptly.
                </p>
                <a href="ContactForm.aspx" class="btn btn-success btn-sm">Send a Message</a>
            </div>
        </div>
    </div>

</div><!-- /container -->
</asp:Content>
