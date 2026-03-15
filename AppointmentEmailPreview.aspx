<%@ Page Title="Appointment Email Preview" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="AppointmentEmailPreview.aspx.cs" Inherits="AppointmentEmailPreview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:760px;margin-top:30px;margin-bottom:40px;">

  <h2><span class="glyphicon glyphicon-envelope"></span> Appointment Confirmation Email Preview</h2>
  <p class="text-muted">
    Fill in your appointment details below and GPT-4 will draft a personalised confirmation email
    you can copy and send to yourself or a carer.
  </p>

  <div class="panel panel-default">
    <div class="panel-body">
      <div class="row">
        <div class="col-sm-6">
          <div class="form-group">
            <label for="TxtPatientName">Your Name</label>
            <asp:TextBox ID="TxtPatientName" runat="server" CssClass="form-control"
                placeholder="e.g. Maria Brown" />
          </div>
        </div>
        <div class="col-sm-6">
          <div class="form-group">
            <label for="TxtAppointmentDate">Appointment Date</label>
            <asp:TextBox ID="TxtAppointmentDate" runat="server" CssClass="form-control"
                placeholder="e.g. 20 March 2026" />
          </div>
        </div>
      </div>
      <div class="row">
        <div class="col-sm-6">
          <div class="form-group">
            <label for="DdlService">Service</label>
            <asp:DropDownList ID="DdlService" runat="server" CssClass="form-control">
              <asp:ListItem>Cardiology</asp:ListItem>
              <asp:ListItem>General Practitioner</asp:ListItem>
              <asp:ListItem>Gynaecology</asp:ListItem>
              <asp:ListItem>Opticology</asp:ListItem>
              <asp:ListItem>Paediatrician</asp:ListItem>
              <asp:ListItem>Radiology</asp:ListItem>
              <asp:ListItem>Surgeon</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
        <div class="col-sm-6">
          <div class="form-group">
            <label for="DdlTimeSlot">Time Slot</label>
            <asp:DropDownList ID="DdlTimeSlot" runat="server" CssClass="form-control">
              <asp:ListItem>8:00 AM – 9:00 AM</asp:ListItem>
              <asp:ListItem>9:00 AM – 10:00 AM</asp:ListItem>
              <asp:ListItem>11:00 AM – 12:00 PM</asp:ListItem>
              <asp:ListItem>12:00 PM – 1:00 PM</asp:ListItem>
              <asp:ListItem>1:00 PM – 2:00 PM</asp:ListItem>
              <asp:ListItem>2:00 PM – 3:00 PM</asp:ListItem>
              <asp:ListItem>3:00 PM – 4:00 PM</asp:ListItem>
              <asp:ListItem>4:00 PM – 5:00 PM</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
      </div>

      <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />

      <asp:Button ID="BtnGenerate" runat="server" Text="Generate Email Draft"
          CssClass="btn btn-primary" OnClick="BtnGenerate_Click" />
    </div>
  </div>

  <!-- Result panel -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-success">
      <div class="panel-heading">
        <h4 class="panel-title">
          <span class="glyphicon glyphicon-ok"></span> Your Confirmation Email Draft
          <button type="button" class="btn btn-xs btn-default pull-right" onclick="copyEmail()">
            <span class="glyphicon glyphicon-copy"></span> Copy
          </button>
        </h4>
      </div>
      <div class="panel-body">
        <pre id="emailDraft" style="background:#fff;border:none;font-family:inherit;white-space:pre-wrap;font-size:14px;">
          <asp:Literal ID="LitEmail" runat="server" />
        </pre>
      </div>
    </div>
    <p class="text-muted" style="font-size:12px;">
      <span class="glyphicon glyphicon-info-sign"></span>
      This email was drafted by GPT-4 based on the details you provided.
      Please review and personalise before sending.
    </p>
  </asp:Panel>

</div>

<script type="text/javascript">
function copyEmail() {
    var text = document.getElementById('emailDraft').innerText;
    if (navigator.clipboard) {
        navigator.clipboard.writeText(text);
        alert('Email draft copied to clipboard!');
    }
}
</script>
</asp:Content>
