<%@ Page Title="Second Opinion Question Generator" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="SecondOpinionPrompts.aspx.cs" Inherits="SecondOpinionPrompts" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .sop-hero{background:linear-gradient(135deg,#1a5276,#0d2b45);color:#fff;padding:30px 20px 22px;margin-bottom:28px;border-radius:0 0 6px 6px;}
    .sop-hero h2{margin:0 0 4px;font-size:26px;} .sop-hero p{margin:0;opacity:.85;font-size:14px;}
    .card{background:#fff;border:1px solid #dde3ea;border-radius:6px;padding:24px;margin-bottom:24px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
    .card h4{margin-top:0;color:#1a5276;border-bottom:1px solid #eef1f5;padding-bottom:10px;}
    .qs-output{white-space:pre-wrap;font-size:14px;line-height:1.9;color:#333;}
    .disclaimer{font-size:12px;color:#888;margin-top:14px;border-top:1px solid #eee;padding-top:10px;}
</style>
<div class="sop-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-question-sign"></span>&nbsp;Second Opinion Question Generator</h2>
        <p>Paste your diagnosis or treatment plan — GPT-4 will generate smart questions to ask your doctor at your next visit.</p>
    </div>
</div>
<div class="container">
    <asp:Panel ID="PanelForm" runat="server">
        <div class="card">
            <h4><span class="glyphicon glyphicon-paste"></span> Your Diagnosis or Treatment Plan</h4>
            <div class="form-group">
                <label>Paste or type the diagnosis / treatment plan you have received:</label>
                <asp:TextBox ID="TxtDiagnosis" runat="server" TextMode="MultiLine" Rows="7"
                    CssClass="form-control"
                    placeholder="e.g. You have been diagnosed with Type 2 Diabetes. Your doctor recommends starting Metformin 500mg twice daily and making dietary changes. A follow-up is scheduled in 3 months..." />
            </div>
            <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" /><br />
            <asp:Button ID="BtnGenerate" runat="server" Text="Generate My Questions"
                CssClass="btn btn-primary btn-lg" OnClick="BtnGenerate_Click" />
        </div>
    </asp:Panel>
    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-question-sign"></span> Questions to Ask Your Doctor
                <button onclick="window.print()" class="btn btn-default btn-sm pull-right">
                    <span class="glyphicon glyphicon-print"></span> Print
                </button>
            </h4>
            <div class="alert alert-info" style="margin-bottom:14px;">
                <span class="glyphicon glyphicon-info-sign"></span>
                &nbsp;Bring this list to your next appointment. Don't be afraid to ask every question — your doctor is there to help.
            </div>
            <div class="qs-output"><asp:Literal ID="LitQuestions" runat="server" /></div>
            <p class="disclaimer"><span class="glyphicon glyphicon-exclamation-sign"></span> These questions are AI-generated based on the text you provided. They are a starting point for conversation, not medical advice.</p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <asp:Button ID="BtnReset" runat="server" Text="Try Different Diagnosis" CssClass="btn btn-default" OnClick="BtnReset_Click" />
            <a href="AppointmentForm.aspx" class="btn btn-primary" style="margin-left:8px;">Book an Appointment</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
