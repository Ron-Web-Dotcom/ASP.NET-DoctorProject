<%@ Page Title="Health Goal Planner" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="HealthGoalPlanner.aspx.cs" Inherits="HealthGoalPlanner" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .goal-hero{background:linear-gradient(135deg,#d35400,#922b21);color:#fff;padding:30px 20px 22px;margin-bottom:28px;border-radius:0 0 6px 6px;}
    .goal-hero h2{margin:0 0 4px;font-size:26px;} .goal-hero p{margin:0;opacity:.85;font-size:14px;}
    .card{background:#fff;border:1px solid #dde3ea;border-radius:6px;padding:24px;margin-bottom:24px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
    .card h4{margin-top:0;color:#d35400;border-bottom:1px solid #eef1f5;padding-bottom:10px;}
    .plan-output{white-space:pre-wrap;font-size:14px;line-height:1.8;color:#333;}
    .week-block{background:#fff8f4;border-left:4px solid #d35400;padding:12px 16px;margin-bottom:10px;border-radius:0 4px 4px 0;}
    .disclaimer{font-size:12px;color:#888;margin-top:14px;border-top:1px solid #eee;padding-top:10px;}
</style>
<div class="goal-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-flag"></span>&nbsp;Health Goal Planner</h2>
        <p>Tell us your health goal and GPT-4 will build you a personalised 4-week action plan to get you started.</p>
    </div>
</div>
<div class="container">
    <asp:Panel ID="PanelForm" runat="server">
        <div class="card">
            <h4><span class="glyphicon glyphicon-list-alt"></span> Tell Us Your Goal</h4>
            <div class="form-group">
                <label>What is your health goal?</label>
                <asp:TextBox ID="TxtGoal" runat="server" CssClass="form-control" placeholder="e.g. Lose 10kg, lower my blood pressure, get more active, improve my sleep..." />
            </div>
            <div class="row">
                <div class="col-sm-6">
                    <div class="form-group">
                        <label>Current Activity Level</label>
                        <asp:DropDownList ID="DdlActivity" runat="server" CssClass="form-control">
                            <asp:ListItem Value="sedentary">Sedentary (little to no exercise)</asp:ListItem>
                            <asp:ListItem Value="light">Light (1-2 days/week)</asp:ListItem>
                            <asp:ListItem Value="moderate">Moderate (3-4 days/week)</asp:ListItem>
                            <asp:ListItem Value="active">Active (5+ days/week)</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-sm-6">
                    <div class="form-group">
                        <label>Existing Conditions <span style="color:#aaa;font-weight:normal;">(optional)</span></label>
                        <asp:TextBox ID="TxtConditions" runat="server" CssClass="form-control" placeholder="e.g. bad knees, asthma, diabetes..." />
                    </div>
                </div>
            </div>
            <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" /><br />
            <asp:Button ID="BtnGenerate" runat="server" Text="Build My 4-Week Plan"
                CssClass="btn btn-lg" style="background:#d35400;color:#fff;border:none;" OnClick="BtnGenerate_Click" />
        </div>
    </asp:Panel>
    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-flag"></span> Your 4-Week Plan
                <small style="color:#777;font-weight:normal;"> for: <asp:Literal ID="LitGoal" runat="server" /></small>
            </h4>
            <div class="plan-output"><asp:Literal ID="LitPlan" runat="server" /></div>
            <p class="disclaimer"><span class="glyphicon glyphicon-exclamation-sign"></span> Always consult your doctor before making significant changes to your exercise or diet routine, especially if you have existing health conditions.</p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <asp:Button ID="BtnReset" runat="server" Text="Plan a Different Goal" CssClass="btn btn-default" OnClick="BtnReset_Click" />
            <a href="LifestylePlanner.aspx" class="btn btn-success" style="margin-left:8px;">Diet &amp; Lifestyle Planner</a>
            <a href="AppointmentForm.aspx" class="btn btn-primary" style="margin-left:8px;">Book an Appointment</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
