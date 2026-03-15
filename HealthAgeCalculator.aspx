<%@ Page Title="Health Age Calculator" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="HealthAgeCalculator.aspx.cs" Inherits="HealthAgeCalculator" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="container" style="max-width:760px;margin-top:30px;margin-bottom:40px;">

  <h2><span class="glyphicon glyphicon-dashboard"></span> Health Age Calculator</h2>
  <p class="text-muted">
    Answer a few simple lifestyle questions and GPT-4 will estimate your "health age" —
    how old your body behaves relative to your actual age — along with personalised improvement tips.
  </p>

  <div class="alert alert-warning">
    <span class="glyphicon glyphicon-warning-sign"></span>
    <strong>Disclaimer:</strong> This is a general wellness estimate for educational purposes only
    and is not a medical assessment or clinical diagnosis.
    Please speak to your GP for personalised health advice.
  </div>

  <div class="panel panel-default">
    <div class="panel-body">
      <div class="row">
        <div class="col-sm-4">
          <div class="form-group">
            <label for="TxtAge">Your Actual Age</label>
            <asp:TextBox ID="TxtAge" runat="server" CssClass="form-control"
                placeholder="e.g. 42" TextMode="Number" />
          </div>
        </div>
        <div class="col-sm-4">
          <div class="form-group">
            <label for="DdlSmoking">Smoking Status</label>
            <asp:DropDownList ID="DdlSmoking" runat="server" CssClass="form-control">
              <asp:ListItem>Non-smoker</asp:ListItem>
              <asp:ListItem>Ex-smoker</asp:ListItem>
              <asp:ListItem>Occasional smoker</asp:ListItem>
              <asp:ListItem>Regular smoker</asp:ListItem>
              <asp:ListItem>Heavy smoker</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
        <div class="col-sm-4">
          <div class="form-group">
            <label for="DdlExercise">Exercise Frequency</label>
            <asp:DropDownList ID="DdlExercise" runat="server" CssClass="form-control">
              <asp:ListItem>Daily</asp:ListItem>
              <asp:ListItem>4–5 times a week</asp:ListItem>
              <asp:ListItem>2–3 times a week</asp:ListItem>
              <asp:ListItem>Once a week</asp:ListItem>
              <asp:ListItem>Rarely</asp:ListItem>
              <asp:ListItem>Never</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
      </div>
      <div class="row">
        <div class="col-sm-4">
          <div class="form-group">
            <label for="DdlDiet">Diet Quality</label>
            <asp:DropDownList ID="DdlDiet" runat="server" CssClass="form-control">
              <asp:ListItem>Excellent</asp:ListItem>
              <asp:ListItem>Good</asp:ListItem>
              <asp:ListItem>Average</asp:ListItem>
              <asp:ListItem>Poor</asp:ListItem>
              <asp:ListItem>Very poor</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
        <div class="col-sm-4">
          <div class="form-group">
            <label for="DdlSleep">Average Sleep per Night</label>
            <asp:DropDownList ID="DdlSleep" runat="server" CssClass="form-control">
              <asp:ListItem>9+ hours</asp:ListItem>
              <asp:ListItem>7–8 hours</asp:ListItem>
              <asp:ListItem>6 hours</asp:ListItem>
              <asp:ListItem>5 hours</asp:ListItem>
              <asp:ListItem>Less than 5 hours</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
        <div class="col-sm-4">
          <div class="form-group">
            <label for="DdlStress">Stress Level</label>
            <asp:DropDownList ID="DdlStress" runat="server" CssClass="form-control">
              <asp:ListItem>Low</asp:ListItem>
              <asp:ListItem>Moderate</asp:ListItem>
              <asp:ListItem>High</asp:ListItem>
              <asp:ListItem>Very high</asp:ListItem>
            </asp:DropDownList>
          </div>
        </div>
      </div>

      <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />

      <asp:Button ID="BtnCalculate" runat="server" Text="Calculate My Health Age"
          CssClass="btn btn-success" OnClick="BtnCalculate_Click" />
    </div>
  </div>

  <!-- Result panel -->
  <asp:Panel ID="PanelResult" runat="server" Visible="false">
    <div class="panel panel-success">
      <div class="panel-heading">
        <h4 class="panel-title">
          <span class="glyphicon glyphicon-dashboard"></span>
          Your Health Age Result
        </h4>
      </div>
      <div class="panel-body" style="white-space:pre-wrap;font-size:14px;">
        <asp:Literal ID="LitResult" runat="server" />
      </div>
    </div>
    <div class="alert alert-info">
      <span class="glyphicon glyphicon-info-sign"></span>
      Ready to make a change?
      <a href="HealthGoalPlanner.aspx" class="btn btn-info btn-sm pull-right">Open Health Goal Planner</a>
    </div>
  </asp:Panel>

</div>
</asp:Content>
