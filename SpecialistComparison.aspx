<%@ Page Title="Specialist Comparison Tool" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="SpecialistComparison.aspx.cs" Inherits="SpecialistComparison" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:760px;margin-top:30px;margin-bottom:40px;">

  <h2><span class="glyphicon glyphicon-transfer"></span> Specialist Comparison Tool</h2>
  <p class="text-muted">
    Not sure which specialist you need? Select two specialties and GPT-4 will explain the difference
    in plain English to help you decide which is most appropriate.
  </p>

  <div class="panel panel-default">
    <div class="panel-body">
      <div class="row">
        <div class="col-sm-5">
          <div class="form-group">
            <label for="DdlSpecialty1">First Specialty</label>
            <asp:DropDownList ID="DdlSpecialty1" runat="server" CssClass="form-control">
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
        <div class="col-sm-2 text-center" style="padding-top:28px;">
          <span class="glyphicon glyphicon-transfer" style="font-size:24px;color:#999;"></span>
          <br /><small class="text-muted">vs</small>
        </div>
        <div class="col-sm-5">
          <div class="form-group">
            <label for="DdlSpecialty2">Second Specialty</label>
            <asp:DropDownList ID="DdlSpecialty2" runat="server" CssClass="form-control">
              <asp:ListItem>General Practitioner</asp:ListItem>
              <asp:ListItem>Cardiology</asp:ListItem>
              <asp:ListItem>Gynaecology</asp:ListItem>
              <asp:ListItem>Opticology</asp:ListItem>
              <asp:ListItem>Paediatrician</asp:ListItem>
              <asp:ListItem>Radiology</asp:ListItem>
              <asp:ListItem>Surgeon</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
      </div>

      <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />

      <asp:Button ID="BtnCompare" runat="server" Text="Compare Specialists"
          CssClass="btn btn-primary" OnClick="BtnCompare_Click" />
    </div>
  </div>

  <!-- Result panel -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-primary">
      <div class="panel-heading">
        <h4 class="panel-title">
          <span class="glyphicon glyphicon-transfer"></span>
          Specialist Comparison
        </h4>
      </div>
      <div class="panel-body" style="white-space:pre-wrap;font-size:14px;">
        <asp:Literal ID="LitComparison" runat="server" />
      </div>
    </div>
    <div class="alert alert-info">
      <span class="glyphicon glyphicon-info-sign"></span>
      Your GP can refer you to the most appropriate specialist based on your individual circumstances.
      <a href="BookAppointment.aspx" class="btn btn-info btn-sm pull-right">Book an Appointment</a>
    </div>
  </asp:Panel>

</div>
</asp:Content>
