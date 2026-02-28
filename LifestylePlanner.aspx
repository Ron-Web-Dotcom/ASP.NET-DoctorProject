<%@ Page Title="AI Diet & Lifestyle Planner" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="LifestylePlanner.aspx.cs" Inherits="LifestylePlanner" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .planner-hero { background:linear-gradient(135deg,#27ae60,#1a7a44); color:#fff; padding:30px 20px 22px; margin-bottom:28px; border-radius:0 0 6px 6px; }
    .planner-hero h2 { margin:0 0 4px; font-size:26px; }
    .planner-hero p  { margin:0; opacity:.85; font-size:14px; }
    .card { background:#fff; border:1px solid #dde3ea; border-radius:6px; padding:24px; margin-bottom:24px; box-shadow:0 1px 4px rgba(0,0,0,.06); }
    .card h4 { margin-top:0; color:#27ae60; border-bottom:1px solid #eef1f5; padding-bottom:10px; }
    .plan-output { white-space:pre-wrap; font-size:14px; line-height:1.75; color:#333; }
    .disclaimer { font-size:12px; color:#888; margin-top:14px; border-top:1px solid #eee; padding-top:10px; }
</style>

<div class="planner-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-leaf"></span>&nbsp;AI Diet &amp; Lifestyle Planner</h2>
        <p>Tell us your upcoming specialty and any relevant details — GPT-4 will generate a personalised diet, exercise, and lifestyle plan to help you prepare.</p>
    </div>
</div>

<div class="container">
    <div class="card">
        <h4><span class="glyphicon glyphicon-list-alt"></span> Your Details</h4>
        <div class="row">
            <div class="col-sm-4">
                <div class="form-group">
                    <label>Specialty / Service Booked</label>
                    <asp:DropDownList ID="DdlSpecialty" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">-- Select a specialty --</asp:ListItem>
                        <asp:ListItem>Cardiology</asp:ListItem>
                        <asp:ListItem>General Practitioner</asp:ListItem>
                        <asp:ListItem>Gynaecology</asp:ListItem>
                        <asp:ListItem>Opticology</asp:ListItem>
                        <asp:ListItem>Paediatrics</asp:ListItem>
                        <asp:ListItem>Radiology</asp:ListItem>
                        <asp:ListItem>Surgery</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="col-sm-4">
                <div class="form-group">
                    <label>Your Age <span style="color:#aaa;font-weight:normal;">(optional)</span></label>
                    <asp:TextBox ID="TxtAge" runat="server" CssClass="form-control" placeholder="e.g. 45" />
                </div>
            </div>
            <div class="col-sm-4">
                <div class="form-group">
                    <label>Other Conditions / Notes <span style="color:#aaa;font-weight:normal;">(optional)</span></label>
                    <asp:TextBox ID="TxtConditions" runat="server" CssClass="form-control"
                        placeholder="e.g. diabetes, high blood pressure" />
                </div>
            </div>
        </div>
        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
        <br />
        <asp:Button ID="BtnGenerate" runat="server" Text="Generate My Plan"
            CssClass="btn btn-success btn-lg" OnClick="BtnGenerate_Click" />
    </div>

    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-flash"></span> Your Personalised Plan</h4>
            <div class="alert alert-success" style="margin-bottom:12px;">
                <span class="glyphicon glyphicon-info-sign"></span>
                &nbsp;This plan is tailored to your booked specialty:
                <strong><asp:Literal ID="LitSpecialty" runat="server" /></strong>
            </div>
            <div class="plan-output">
                <asp:Literal ID="LitPlan" runat="server" />
            </div>
            <p class="disclaimer">
                <span class="glyphicon glyphicon-exclamation-sign"></span>
                This is general wellness guidance only and does not replace advice from your doctor.
                Always follow your clinician's specific recommendations.
            </p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <a href="AppointmentForm.aspx" class="btn btn-primary">Book an Appointment</a>
            <a href="MedicationChecker.aspx" class="btn btn-default" style="margin-left:8px;">Check Medication Interactions</a>
            <a href="SymptomDiary.aspx" class="btn btn-default" style="margin-left:8px;">Open Symptom Diary</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
