<%@ Page Title="Medication Interaction Checker" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="MedicationChecker.aspx.cs" Inherits="MedicationChecker" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:680px; margin-top:30px;">

  <div class="well">
    <center>
      <h2><span class="glyphicon glyphicon-list-alt"></span> Medication Interaction Checker</h2>
      <p class="text-muted">Enter your current medications and GPT-4 will flag any known interactions — always discuss results with your doctor.</p>
    </center>
  </div>

  <asp:Panel ID="PanelForm" runat="server" Visible="true">
    <div class="panel panel-primary">
      <div class="panel-heading"><h3 class="panel-title">Your Medications</h3></div>
      <div class="panel-body">

        <div class="form-group">
          <label>List your current medications</label>
          <p class="text-muted" style="font-size:12px;">Enter each medication on a new line, or separate with commas. Include dosage if known (e.g. Aspirin 75mg).</p>
          <asp:TextBox ID="TxtMedications" runat="server" TextMode="MultiLine" Rows="6"
                       CssClass="form-control"
                       placeholder="e.g.&#10;Aspirin 75mg&#10;Metformin 500mg&#10;Lisinopril 10mg" />
        </div>

        <div class="alert alert-warning" style="font-size:12px;">
          <strong>Disclaimer:</strong> This tool provides general educational information only and is
          <strong>not</strong> a substitute for advice from your doctor, pharmacist, or other qualified
          healthcare professional. Always consult a professional before making any changes to your medication.
        </div>

        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
        <asp:Button ID="BtnCheck" runat="server" Text="Check Interactions"
                    CssClass="btn btn-danger btn-lg btn-block" OnClick="BtnCheck_Click" />
      </div>
    </div>
  </asp:Panel>

  <!-- Results -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-warning">
      <div class="panel-heading">
        <h3 class="panel-title"><span class="glyphicon glyphicon-exclamation-sign"></span> Interaction Report</h3>
      </div>
      <div class="panel-body">
        <div class="well" style="white-space:pre-wrap; background:#fff9f0;">
          <asp:Literal ID="LitResult" runat="server" />
        </div>
        <asp:Button ID="BtnCheckAnother" runat="server" Text="Check Another List"
                    CssClass="btn btn-default" OnClick="BtnCheckAnother_Click" />
        &nbsp;
        <a href="AppointmentForm.aspx" class="btn btn-primary">Book an Appointment</a>
      </div>
    </div>
  </asp:Panel>

</div>
</asp:Content>
