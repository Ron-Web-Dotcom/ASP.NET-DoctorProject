<%@ Page Title="Cancel / Reschedule Appointment" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="CancelAppointment.aspx.cs" Inherits="CancelAppointment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:650px; margin-top:30px;">

  <div class="well">
    <center>
      <h2><span class="glyphicon glyphicon-calendar"></span> Cancel / Reschedule Appointment</h2>
      <p class="text-muted">We understand that plans change. Let us help you reschedule.</p>
    </center>
  </div>

  <!-- ===== Cancellation request form ===== -->
  <asp:Panel ID="PanelForm" runat="server" Visible="true">
    <div class="panel panel-warning">
      <div class="panel-heading">
        <h3 class="panel-title">Your Details</h3>
      </div>
      <div class="panel-body">

        <div class="form-group">
          <label>First Name</label>
          <asp:TextBox ID="TxtFirstName" runat="server" CssClass="form-control" placeholder="Your first name" />
        </div>

        <div class="form-group">
          <label>Service You Booked</label>
          <asp:DropDownList ID="DdlService" runat="server" CssClass="form-control">
            <asp:ListItem>Select a service</asp:ListItem>
            <asp:ListItem>Cardiology</asp:ListItem>
            <asp:ListItem>General Practitioner</asp:ListItem>
            <asp:ListItem>Gynaecology</asp:ListItem>
            <asp:ListItem>Opticology</asp:ListItem>
            <asp:ListItem>Paediatrics</asp:ListItem>
            <asp:ListItem>Radiology</asp:ListItem>
            <asp:ListItem>Surgery</asp:ListItem>
          </asp:DropDownList>
        </div>

        <div class="form-group">
          <label>Reason for Cancellation <span class="text-muted">(optional)</span></label>
          <asp:TextBox ID="TxtReason" runat="server" TextMode="MultiLine" Rows="3"
                       CssClass="form-control" placeholder="e.g. Work commitment, feeling better, etc." />
        </div>

        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
        <br />
        <asp:Button ID="BtnCancel" runat="server" Text="Submit Cancellation Request"
                    CssClass="btn btn-warning btn-lg btn-block" OnClick="BtnCancel_Click" />
      </div>
    </div>
  </asp:Panel>

  <!-- ===== AI rescheduling message result ===== -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-info">
      <div class="panel-heading">
        <h3 class="panel-title">
          <span class="glyphicon glyphicon-info-sign"></span> Your Rescheduling Message
        </h3>
      </div>
      <div class="panel-body">
        <div class="alert alert-info" style="white-space:pre-wrap;">
          <asp:Literal ID="LitRescheduleMsg" runat="server" />
        </div>
        <p>
          <a href="AppointmentForm.aspx" class="btn btn-success">
            <span class="glyphicon glyphicon-calendar"></span> Book a New Appointment
          </a>
          &nbsp;
          <a href="HomePage.aspx" class="btn btn-default">Return to Home</a>
        </p>
      </div>
    </div>
  </asp:Panel>

</div>
</asp:Content>
