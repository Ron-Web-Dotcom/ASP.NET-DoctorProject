<%@ Page Title="Emergency Symptom Triage" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="EmergencyTriage.aspx.cs" Inherits="EmergencyTriage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .triage-hero{background:linear-gradient(135deg,#c0392b,#7b241c);color:#fff;padding:30px 20px 22px;margin-bottom:28px;border-radius:0 0 6px 6px;}
    .triage-hero h2{margin:0 0 4px;font-size:26px;} .triage-hero p{margin:0;opacity:.85;font-size:14px;}
    .card{background:#fff;border:1px solid #dde3ea;border-radius:6px;padding:24px;margin-bottom:24px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
    .card h4{margin-top:0;color:#c0392b;border-bottom:1px solid #eef1f5;padding-bottom:10px;}
    .result-output{white-space:pre-wrap;font-size:14px;line-height:1.8;color:#333;}
    .level-emergency{background:#F2DEDE;border-left:5px solid #A94442;padding:14px 18px;border-radius:4px;font-size:15px;font-weight:bold;color:#A94442;}
    .level-urgent   {background:#FCF8E3;border-left:5px solid #8A6D3B;padding:14px 18px;border-radius:4px;font-size:15px;font-weight:bold;color:#8A6D3B;}
    .level-appointment{background:#D9EDF7;border-left:5px solid #31708F;padding:14px 18px;border-radius:4px;font-size:15px;font-weight:bold;color:#31708F;}
    .level-selfcare {background:#DFF0D8;border-left:5px solid #3C763D;padding:14px 18px;border-radius:4px;font-size:15px;font-weight:bold;color:#3C763D;}
    .disclaimer{font-size:12px;color:#888;margin-top:14px;border-top:1px solid #eee;padding-top:10px;}
</style>
<div class="triage-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-warning-sign"></span>&nbsp;Emergency Symptom Triage</h2>
        <p>Describe your symptoms and GPT-4 will classify the urgency level and tell you exactly what to do next.</p>
    </div>
</div>
<div class="container">
    <div class="alert alert-danger">
        <strong><span class="glyphicon glyphicon-exclamation-sign"></span> If you are having a medical emergency, call 999 immediately. Do not wait for this tool.</strong>
    </div>
    <asp:Panel ID="PanelForm" runat="server">
        <div class="card">
            <h4><span class="glyphicon glyphicon-list-alt"></span> Describe Your Symptoms</h4>
            <div class="form-group">
                <label>What symptoms are you experiencing? Include when they started and how severe they are.</label>
                <asp:TextBox ID="TxtSymptoms" runat="server" TextMode="MultiLine" Rows="6"
                    CssClass="form-control" placeholder="e.g. Severe chest pain for the last 20 minutes, pain radiating to my left arm, feeling short of breath and sweaty..." />
            </div>
            <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" /><br />
            <asp:Button ID="BtnTriage" runat="server" Text="Assess My Symptoms"
                CssClass="btn btn-danger btn-lg" OnClick="BtnTriage_Click" />
        </div>
    </asp:Panel>
    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-flash"></span> Triage Assessment</h4>
            <asp:Literal ID="LitLevelBadge" runat="server" />
            <div class="result-output" style="margin-top:16px;">
                <asp:Literal ID="LitResult" runat="server" />
            </div>
            <p class="disclaimer"><span class="glyphicon glyphicon-exclamation-sign"></span> This tool provides general guidance only and does not replace professional medical assessment. If in doubt, always seek immediate help.</p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <asp:Button ID="BtnReset" runat="server" Text="Assess Another Symptom" CssClass="btn btn-default" OnClick="BtnReset_Click" />
            <a href="AppointmentForm.aspx" class="btn btn-primary" style="margin-left:8px;">Book an Appointment</a>
            <a href="AIChatAssistant.aspx" class="btn btn-info" style="margin-left:8px;">Chat with AI Assistant</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
