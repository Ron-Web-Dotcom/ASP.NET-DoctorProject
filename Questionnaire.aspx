<%@ Page Title="Pre-Appointment Questionnaire" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Questionnaire.aspx.cs" Inherits="Questionnaire" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:700px; margin-top:30px;">

  <div class="well">
    <center><h2>Pre-Appointment Questionnaire</h2>
    <p class="text-muted">Help your doctor prepare by answering a few short questions before your visit.</p></center>
  </div>

  <!-- ===== Questionnaire form panel ===== -->
  <asp:Panel ID="PanelQuestionnaire" runat="server" Visible="true">
    <div class="panel panel-primary">
      <div class="panel-heading">
        <h3 class="panel-title"><span class="glyphicon glyphicon-list-alt"></span> Pre-Screening Questions</h3>
      </div>
      <div class="panel-body">

        <div class="form-group">
          <label>Which service are you booking for?</label>
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
          <label>How long have you been experiencing your symptoms?</label>
          <asp:DropDownList ID="DdlDuration" runat="server" CssClass="form-control">
            <asp:ListItem>Select duration</asp:ListItem>
            <asp:ListItem>Less than 24 hours</asp:ListItem>
            <asp:ListItem>1–3 days</asp:ListItem>
            <asp:ListItem>4–7 days</asp:ListItem>
            <asp:ListItem>1–2 weeks</asp:ListItem>
            <asp:ListItem>More than 2 weeks</asp:ListItem>
            <asp:ListItem>More than a month</asp:ListItem>
            <asp:ListItem>Ongoing / chronic</asp:ListItem>
          </asp:DropDownList>
        </div>

        <div class="form-group">
          <label>How would you rate the severity of your symptoms? (1 = mild, 10 = severe)</label>
          <asp:DropDownList ID="DdlSeverity" runat="server" CssClass="form-control">
            <asp:ListItem>Select severity</asp:ListItem>
            <asp:ListItem>1 – Very mild</asp:ListItem>
            <asp:ListItem>2</asp:ListItem>
            <asp:ListItem>3</asp:ListItem>
            <asp:ListItem>4</asp:ListItem>
            <asp:ListItem>5 – Moderate</asp:ListItem>
            <asp:ListItem>6</asp:ListItem>
            <asp:ListItem>7</asp:ListItem>
            <asp:ListItem>8</asp:ListItem>
            <asp:ListItem>9</asp:ListItem>
            <asp:ListItem>10 – Very severe</asp:ListItem>
          </asp:DropDownList>
        </div>

        <div class="form-group">
          <label>Are you currently taking any medications? (List them or write "None")</label>
          <asp:TextBox ID="TxtMedications" runat="server" CssClass="form-control"
                       placeholder="e.g. Aspirin 75mg, Metformin 500mg / None" />
        </div>

        <div class="form-group">
          <label>Do you have any known allergies? (medicines, foods, etc., or write "None")</label>
          <asp:TextBox ID="TxtAllergies" runat="server" CssClass="form-control"
                       placeholder="e.g. Penicillin, shellfish / None" />
        </div>

        <div class="form-group">
          <label>Is there anything else you would like your doctor to know before your appointment?</label>
          <asp:TextBox ID="TxtAdditional" runat="server" TextMode="MultiLine" Rows="4"
                       CssClass="form-control" placeholder="Optional — any other relevant information" />
        </div>

        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
        <br />
        <asp:Button ID="BtnAnalyse" runat="server" Text="Analyse &amp; Continue to Booking"
                    CssClass="btn btn-success btn-lg btn-block" OnClick="BtnAnalyse_Click" />
      </div>
    </div>
  </asp:Panel>

  <!-- ===== Results panel — shown after AI analysis ===== -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-success">
      <div class="panel-heading">
        <h3 class="panel-title"><span class="glyphicon glyphicon-ok-circle"></span> Questionnaire Complete</h3>
      </div>
      <div class="panel-body">
        <p>Thank you. Your pre-screening summary has been prepared and will be included with your booking.
           Your doctor will review it before your appointment.</p>
        <div class="alert alert-info">
          <strong>Your Summary:</strong><br />
          <asp:Literal ID="LitSummary" runat="server" />
        </div>
        <a href="AppointmentForm.aspx" class="btn btn-primary btn-lg">
          <span class="glyphicon glyphicon-calendar"></span>
          Continue to Appointment Form
        </a>
      </div>
    </div>
  </asp:Panel>

</div>
</asp:Content>
