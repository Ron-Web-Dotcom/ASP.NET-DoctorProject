<%@ Page Title="Medical History Summariser" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="MedicalHistory.aspx.cs" Inherits="MedicalHistory" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .hist-hero{background:linear-gradient(135deg,#16a085,#0e6655);color:#fff;padding:30px 20px 22px;margin-bottom:28px;border-radius:0 0 6px 6px;}
    .hist-hero h2{margin:0 0 4px;font-size:26px;} .hist-hero p{margin:0;opacity:.85;font-size:14px;}
    .card{background:#fff;border:1px solid #dde3ea;border-radius:6px;padding:24px;margin-bottom:24px;box-shadow:0 1px 4px rgba(0,0,0,.06);}
    .card h4{margin-top:0;color:#16a085;border-bottom:1px solid #eef1f5;padding-bottom:10px;}
    .summary-output{white-space:pre-wrap;font-size:14px;line-height:1.8;color:#333;background:#f8fffd;border:1px solid #b2dfdb;border-radius:4px;padding:16px;}
    .disclaimer{font-size:12px;color:#888;margin-top:14px;border-top:1px solid #eee;padding-top:10px;}
</style>
<div class="hist-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-file"></span>&nbsp;Medical History Summariser</h2>
        <p>Enter your medical history and GPT-4 will generate a concise clinical summary you can print and share with any healthcare provider.</p>
    </div>
</div>
<div class="container">
    <asp:Panel ID="PanelForm" runat="server">
        <div class="card">
            <h4><span class="glyphicon glyphicon-list-alt"></span> Your Medical History</h4>
            <div class="row">
                <div class="col-sm-6">
                    <div class="form-group">
                        <label>Current Medical Conditions</label>
                        <asp:TextBox ID="TxtConditions" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="e.g. Type 2 diabetes, hypertension, asthma..." />
                    </div>
                    <div class="form-group">
                        <label>Current Medications</label>
                        <asp:TextBox ID="TxtMedications" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="e.g. Metformin 500mg twice daily, Lisinopril 10mg once daily..." />
                    </div>
                    <div class="form-group">
                        <label>Known Allergies</label>
                        <asp:TextBox ID="TxtAllergies" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" placeholder="e.g. Penicillin — rash, latex — hives..." />
                    </div>
                </div>
                <div class="col-sm-6">
                    <div class="form-group">
                        <label>Past Surgeries &amp; Procedures</label>
                        <asp:TextBox ID="TxtSurgeries" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="e.g. Appendectomy 2010, knee arthroscopy 2018..." />
                    </div>
                    <div class="form-group">
                        <label>Family Medical History</label>
                        <asp:TextBox ID="TxtFamilyHistory" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" placeholder="e.g. Father — heart disease, Mother — breast cancer..." />
                    </div>
                </div>
            </div>
            <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" /><br />
            <asp:Button ID="BtnGenerate" runat="server" Text="Generate Clinical Summary"
                CssClass="btn btn-lg" style="background:#16a085;color:#fff;border:none;" OnClick="BtnGenerate_Click" />
        </div>
    </asp:Panel>
    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-print"></span> Clinical Summary
                <button onclick="window.print()" class="btn btn-default btn-sm pull-right">
                    <span class="glyphicon glyphicon-print"></span> Print
                </button>
            </h4>
            <div class="summary-output"><asp:Literal ID="LitSummary" runat="server" /></div>
            <p class="disclaimer"><span class="glyphicon glyphicon-exclamation-sign"></span> This summary is generated from information you provided. Always verify its accuracy with your healthcare provider before sharing.</p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <asp:Button ID="BtnReset" runat="server" Text="Edit &amp; Regenerate" CssClass="btn btn-default" OnClick="BtnReset_Click" />
            <a href="AppointmentForm.aspx" class="btn btn-primary" style="margin-left:8px;">Book an Appointment</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
