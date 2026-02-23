<%@ Page Title="Staff Bio Generator" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="StaffBioGenerator.aspx.cs" Inherits="StaffBioGenerator" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:700px; margin-top:30px;">

  <div class="well">
    <center>
      <h2><span class="glyphicon glyphicon-user"></span> AI Staff Bio Generator</h2>
      <p class="text-muted">Enter a doctor's details and GPT-4 will write a polished biography ready for the website.</p>
    </center>
  </div>

  <asp:Panel ID="PanelForm" runat="server" Visible="true">
    <div class="panel panel-primary">
      <div class="panel-heading"><h3 class="panel-title">Doctor Details</h3></div>
      <div class="panel-body">

        <div class="form-group">
          <label>Full Name <span class="text-danger">*</span></label>
          <asp:TextBox ID="TxtName" runat="server" CssClass="form-control" placeholder="e.g. Jane Smith" />
        </div>

        <div class="form-group">
          <label>Medical Specialty <span class="text-danger">*</span></label>
          <asp:DropDownList ID="DdlSpecialty" runat="server" CssClass="form-control">
            <asp:ListItem>Select specialty</asp:ListItem>
            <asp:ListItem>Cardiology</asp:ListItem>
            <asp:ListItem>General Practice</asp:ListItem>
            <asp:ListItem>Gynaecology</asp:ListItem>
            <asp:ListItem>Opticology</asp:ListItem>
            <asp:ListItem>Paediatrics</asp:ListItem>
            <asp:ListItem>Radiology</asp:ListItem>
            <asp:ListItem>Surgery</asp:ListItem>
          </asp:DropDownList>
        </div>

        <div class="form-group">
          <label>Qualifications <span class="text-danger">*</span></label>
          <asp:TextBox ID="TxtQualifications" runat="server" CssClass="form-control"
                       placeholder="e.g. MBBS, MRCP, PhD" />
        </div>

        <div class="form-group">
          <label>Years of Experience <span class="text-danger">*</span></label>
          <asp:TextBox ID="TxtYears" runat="server" CssClass="form-control" style="max-width:150px;"
                       placeholder="e.g. 15" />
        </div>

        <div class="form-group">
          <label>Additional Details <span class="text-muted">(optional)</span></label>
          <asp:TextBox ID="TxtExtra" runat="server" TextMode="MultiLine" Rows="3"
                       CssClass="form-control"
                       placeholder="e.g. Research interests, languages spoken, awards, community work..." />
        </div>

        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
        <asp:Button ID="BtnGenerate" runat="server" Text="Generate Biography"
                    CssClass="btn btn-success btn-lg btn-block" OnClick="BtnGenerate_Click" />
      </div>
    </div>
  </asp:Panel>

  <!-- Generated bio -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-success">
      <div class="panel-heading">
        <h3 class="panel-title"><span class="glyphicon glyphicon-ok"></span> Generated Biography</h3>
      </div>
      <div class="panel-body">
        <div class="well" style="white-space:pre-wrap; font-family:Georgia, serif; font-size:15px; line-height:1.7;">
          <asp:Literal ID="LitBio" runat="server" />
        </div>
        <asp:Button ID="BtnGenerateAnother" runat="server" Text="Generate Another"
                    CssClass="btn btn-default" OnClick="BtnGenerateAnother_Click" />
      </div>
    </div>
  </asp:Panel>

</div>
</asp:Content>
