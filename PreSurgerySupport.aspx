<%@ Page Title="Pre-Surgery Anxiety Support" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="PreSurgerySupport.aspx.cs" Inherits="PreSurgerySupport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:760px;margin-top:30px;margin-bottom:40px;">

  <h2><span class="glyphicon glyphicon-heart"></span> Pre-Surgery Anxiety Support</h2>
  <p class="text-muted">
    Feeling nervous before a procedure is completely normal. Tell us what you are facing and
    GPT-4 will provide compassionate, evidence-based support and practical preparation guidance.
  </p>

  <div class="alert alert-info">
    <span class="glyphicon glyphicon-info-sign"></span>
    <strong>Please note:</strong> This tool provides emotional support and general preparation guidance only.
    All medical questions should be directed to your clinical team who are best placed to advise you.
  </div>

  <div class="panel panel-default">
    <div class="panel-body">
      <div class="form-group">
        <label for="DdlProcedure">Type of Surgery or Procedure</label>
        <asp:DropDownList ID="DdlProcedure" runat="server" CssClass="form-control">
          <asp:ListItem>General Surgery</asp:ListItem>
          <asp:ListItem>Cardiac Surgery</asp:ListItem>
          <asp:ListItem>Gynaecological Procedure</asp:ListItem>
          <asp:ListItem>Orthopaedic Surgery</asp:ListItem>
          <asp:ListItem>Eye Surgery / Opticology Procedure</asp:ListItem>
          <asp:ListItem>Diagnostic Radiology Procedure (MRI / CT / X-ray)</asp:ListItem>
          <asp:ListItem>Endoscopy / Colonoscopy</asp:ListItem>
          <asp:ListItem>Minor Surgical Procedure</asp:ListItem>
          <asp:ListItem>Biopsy</asp:ListItem>
          <asp:ListItem>Paediatric Procedure (child)</asp:ListItem>
        </asp:DropDownList>
      </div>
      <div class="form-group">
        <label for="TxtConcerns">What are you most worried about? <small class="text-muted">(optional)</small></label>
        <asp:TextBox ID="TxtConcerns" runat="server" TextMode="MultiLine" Rows="3"
            CssClass="form-control"
            placeholder="e.g. I'm worried about the anaesthetic, recovery time, being in pain..." />
      </div>

      <asp:Button ID="BtnGenerate" runat="server" Text="Get Support &amp; Guidance"
          CssClass="btn btn-danger" OnClick="BtnGenerate_Click" />
    </div>
  </div>

  <!-- Result panel -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-danger">
      <div class="panel-heading">
        <h4 class="panel-title">
          <span class="glyphicon glyphicon-heart"></span>
          Your Pre-Surgery Support Guide
          <button type="button" class="btn btn-xs btn-default pull-right" onclick="window.print()">
            <span class="glyphicon glyphicon-print"></span> Print
          </button>
        </h4>
      </div>
      <div class="panel-body" style="white-space:pre-wrap;font-size:14px;">
        <asp:Literal ID="LitSupport" runat="server" />
      </div>
    </div>
    <div class="alert alert-success">
      <span class="glyphicon glyphicon-phone"></span>
      <strong>Need to speak to someone?</strong> Contact our patient support team at
      <strong>+1 (876) 555-0100</strong> or speak to your surgeon directly.
    </div>
  </asp:Panel>

</div>
</asp:Content>
