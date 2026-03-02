<%@ Page Title="Mental Health Check-In" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="MentalHealthCheckIn.aspx.cs" Inherits="MentalHealthCheckIn" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .mh-hero{background:linear-gradient(135deg,#6c3483,#4a235a);color:#fff;padding:30px 20px 22px;margin-bottom:28px;border-radius:0 0 6px 6px;}
    .mh-hero h2{margin:0 0 4px;font-size:26px;} .mh-hero p{margin:0;opacity:.85;font-size:14px;}
    .card{background:#fff;border:1px solid #dde3ea;border-radius:6px;padding:24px;margin-bottom:24px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
    .card h4{margin-top:0;color:#6c3483;border-bottom:1px solid #eef1f5;padding-bottom:10px;}
    .score-row{display:flex;align-items:center;margin-bottom:14px;gap:16px;}
    .score-label{width:160px;font-weight:bold;font-size:13px;flex-shrink:0;}
    .score-row .form-control{width:140px;}
    .result-output{white-space:pre-wrap;font-size:14px;line-height:1.8;color:#333;}
    .disclaimer{font-size:12px;color:#888;margin-top:14px;border-top:1px solid #eee;padding-top:10px;}
</style>
<div class="mh-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-heart"></span>&nbsp;Mental Health Check-In</h2>
        <p>Take a moment to check in on your wellbeing. Rate each area and GPT-4 will provide personalised support suggestions.</p>
    </div>
</div>
<div class="container">
    <asp:Panel ID="PanelForm" runat="server">
        <div class="card">
            <h4><span class="glyphicon glyphicon-list-alt"></span> How Are You Feeling? <small style="color:#aaa;font-weight:normal;">(1 = very poor, 5 = excellent)</small></h4>
            <div class="score-row">
                <span class="score-label"><span class="glyphicon glyphicon-sunglasses"></span> Mood</span>
                <asp:DropDownList ID="DdlMood" runat="server" CssClass="form-control">
                    <asp:ListItem Value="1">1 — Very low</asp:ListItem>
                    <asp:ListItem Value="2">2 — Low</asp:ListItem>
                    <asp:ListItem Value="3" Selected="True">3 — Okay</asp:ListItem>
                    <asp:ListItem Value="4">4 — Good</asp:ListItem>
                    <asp:ListItem Value="5">5 — Excellent</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="score-row">
                <span class="score-label"><span class="glyphicon glyphicon-bed"></span> Sleep Quality</span>
                <asp:DropDownList ID="DdlSleep" runat="server" CssClass="form-control">
                    <asp:ListItem Value="1">1 — Very poor</asp:ListItem>
                    <asp:ListItem Value="2">2 — Poor</asp:ListItem>
                    <asp:ListItem Value="3" Selected="True">3 — Okay</asp:ListItem>
                    <asp:ListItem Value="4">4 — Good</asp:ListItem>
                    <asp:ListItem Value="5">5 — Excellent</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="score-row">
                <span class="score-label"><span class="glyphicon glyphicon-alert"></span> Anxiety <small>(1=very anxious)</small></span>
                <asp:DropDownList ID="DdlAnxiety" runat="server" CssClass="form-control">
                    <asp:ListItem Value="1">1 — Very anxious</asp:ListItem>
                    <asp:ListItem Value="2">2 — Quite anxious</asp:ListItem>
                    <asp:ListItem Value="3" Selected="True">3 — Some anxiety</asp:ListItem>
                    <asp:ListItem Value="4">4 — Mostly calm</asp:ListItem>
                    <asp:ListItem Value="5">5 — Very calm</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="score-row">
                <span class="score-label"><span class="glyphicon glyphicon-flash"></span> Energy Levels</span>
                <asp:DropDownList ID="DdlEnergy" runat="server" CssClass="form-control">
                    <asp:ListItem Value="1">1 — Exhausted</asp:ListItem>
                    <asp:ListItem Value="2">2 — Tired</asp:ListItem>
                    <asp:ListItem Value="3" Selected="True">3 — Okay</asp:ListItem>
                    <asp:ListItem Value="4">4 — Energetic</asp:ListItem>
                    <asp:ListItem Value="5">5 — Very energetic</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="score-row">
                <span class="score-label"><span class="glyphicon glyphicon-user"></span> Social Connection</span>
                <asp:DropDownList ID="DdlSocial" runat="server" CssClass="form-control">
                    <asp:ListItem Value="1">1 — Very isolated</asp:ListItem>
                    <asp:ListItem Value="2">2 — Mostly alone</asp:ListItem>
                    <asp:ListItem Value="3" Selected="True">3 — Some connection</asp:ListItem>
                    <asp:ListItem Value="4">4 — Well connected</asp:ListItem>
                    <asp:ListItem Value="5">5 — Very connected</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-group" style="margin-top:8px;">
                <label>Anything else you'd like to mention? <span style="color:#aaa;font-weight:normal;">(optional)</span></label>
                <asp:TextBox ID="TxtNotes" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="e.g. I've been under a lot of stress at work recently..." />
            </div>
            <asp:Button ID="BtnCheckIn" runat="server" Text="Get My Wellbeing Summary"
                CssClass="btn btn-lg" style="background:#6c3483;color:#fff;border:none;" OnClick="BtnCheckIn_Click" />
        </div>
    </asp:Panel>
    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-heart"></span> Your Wellbeing Summary</h4>
            <div class="result-output"><asp:Literal ID="LitResult" runat="server" /></div>
            <p class="disclaimer"><span class="glyphicon glyphicon-exclamation-sign"></span> This is not a clinical assessment. If you are experiencing a mental health crisis, please contact your GP, call NHS 111, or the Samaritans on 116 123.</p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <asp:Button ID="BtnReset" runat="server" Text="Check In Again" CssClass="btn btn-default" OnClick="BtnReset_Click" />
            <a href="AppointmentForm.aspx" class="btn btn-primary" style="margin-left:8px;">Book a GP Appointment</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
