<%@ Page Title="Symptom Diary" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="SymptomDiary.aspx.cs" Inherits="SymptomDiary" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:720px; margin-top:30px;">

  <div class="well">
    <center>
      <h2><span class="glyphicon glyphicon-pencil"></span> Symptom Diary</h2>
      <p class="text-muted">Log your daily symptoms and let AI track your progression — share the analysis with your doctor.</p>
    </center>
  </div>

  <!-- Add new entry -->
  <div class="panel panel-primary">
    <div class="panel-heading"><h3 class="panel-title">Add Today's Entry</h3></div>
    <div class="panel-body">
      <div class="form-group">
        <label>Date</label>
        <asp:TextBox ID="TxtDate" runat="server" CssClass="form-control" style="max-width:200px;" />
      </div>
      <div class="form-group">
        <label>How are you feeling today? Describe your symptoms.</label>
        <asp:TextBox ID="TxtEntry" runat="server" TextMode="MultiLine" Rows="3"
                     CssClass="form-control"
                     placeholder="e.g. Moderate chest pain, 6/10. Shortness of breath when climbing stairs. No fever." />
      </div>
      <asp:Label ID="LblEntryError" runat="server" CssClass="text-danger" Visible="false" />
      <asp:Button ID="BtnAddEntry" runat="server" Text="Add Entry" CssClass="btn btn-primary"
                  OnClick="BtnAddEntry_Click" CausesValidation="false" />
    </div>
  </div>

  <!-- Diary log -->
  <asp:Panel ID="PanelDiary" runat="server" Visible="false">
    <div class="panel panel-default">
      <div class="panel-heading"><h3 class="panel-title">Your Diary</h3></div>
      <div class="panel-body">
        <asp:Repeater ID="RptEntries" runat="server">
          <HeaderTemplate><ul class="list-group"></HeaderTemplate>
          <ItemTemplate>
            <li class="list-group-item">
              <strong><%# Eval("Date") %></strong> — <%# Eval("Entry") %>
            </li>
          </ItemTemplate>
          <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
        <asp:Button ID="BtnAnalyse" runat="server" Text="Analyse Diary with AI"
                    CssClass="btn btn-success btn-lg" OnClick="BtnAnalyse_Click"
                    CausesValidation="false" />
        &nbsp;
        <asp:Button ID="BtnClearDiary" runat="server" Text="Clear Diary"
                    CssClass="btn btn-danger btn-sm" OnClick="BtnClearDiary_Click"
                    CausesValidation="false"
                    OnClientClick="return confirm('Clear all diary entries?');" />
      </div>
    </div>
  </asp:Panel>

  <!-- AI Analysis result -->
  <asp:Panel ID="PanelAnalysis" runat="server" Visible="false">
    <div class="panel panel-success">
      <div class="panel-heading"><h3 class="panel-title"><span class="glyphicon glyphicon-stats"></span> AI Analysis</h3></div>
      <div class="panel-body">
        <div class="alert alert-info">
          <strong>Trend Summary:</strong><br />
          <asp:Literal ID="LitSummary" runat="server" />
        </div>
        <div class="alert alert-warning">
          <strong>Recommendation:</strong><br />
          <asp:Literal ID="LitRecommendation" runat="server" />
        </div>
        <p class="text-muted" style="font-size:12px;">
          This AI analysis is for informational purposes only. Always consult a doctor for medical advice.
        </p>
        <a href="AppointmentForm.aspx" class="btn btn-primary">Book an Appointment</a>
      </div>
    </div>
  </asp:Panel>

</div>
</asp:Content>
