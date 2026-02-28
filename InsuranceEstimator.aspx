<%@ Page Title="AI Insurance & Cost Estimator" Language="C#" MasterPageFile="~/MasterPage.master"
    AutoEventWireup="true" CodeFile="InsuranceEstimator.aspx.cs" Inherits="InsuranceEstimator" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<style>
    .ins-hero { background:linear-gradient(135deg,#8e44ad,#5b2c6f); color:#fff; padding:30px 20px 22px; margin-bottom:28px; border-radius:0 0 6px 6px; }
    .ins-hero h2 { margin:0 0 4px; font-size:26px; }
    .ins-hero p  { margin:0; opacity:.85; font-size:14px; }
    .card { background:#fff; border:1px solid #dde3ea; border-radius:6px; padding:24px; margin-bottom:24px; box-shadow:0 1px 4px rgba(0,0,0,.06); }
    .card h4 { margin-top:0; color:#8e44ad; border-bottom:1px solid #eef1f5; padding-bottom:10px; }
    .guide-output { white-space:pre-wrap; font-size:14px; line-height:1.8; color:#333; }
    .disclaimer { font-size:12px; color:#888; margin-top:14px; border-top:1px solid #eee; padding-top:10px; }
</style>

<div class="ins-hero">
    <div class="container">
        <h2><span class="glyphicon glyphicon-usd"></span>&nbsp;AI Insurance &amp; Cost Estimator</h2>
        <p>Select your service and insurance type — GPT-4 will explain what is typically covered, questions to ask your insurer, and general cost factors.</p>
    </div>
</div>

<div class="container">
    <div class="card">
        <h4><span class="glyphicon glyphicon-list-alt"></span> Your Details</h4>
        <div class="row">
            <div class="col-sm-6">
                <div class="form-group">
                    <label>Service / Specialty</label>
                    <asp:DropDownList ID="DdlService" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">-- Select a service --</asp:ListItem>
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
            <div class="col-sm-6">
                <div class="form-group">
                    <label>Insurance Type <span style="color:#aaa;font-weight:normal;">(optional)</span></label>
                    <asp:DropDownList ID="DdlInsurance" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">-- Not sure / prefer not to say --</asp:ListItem>
                        <asp:ListItem>Private Health Insurance</asp:ListItem>
                        <asp:ListItem>National Health Insurance (NHI)</asp:ListItem>
                        <asp:ListItem>Employer Health Plan</asp:ListItem>
                        <asp:ListItem>Self-Pay / No Insurance</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
        </div>
        <asp:Label ID="LblError" runat="server" CssClass="text-danger" Visible="false" />
        <br />
        <asp:Button ID="BtnEstimate" runat="server" Text="Get Insurance Guide"
            CssClass="btn btn-lg" style="background:#8e44ad;color:#fff;border:none;"
            OnClick="BtnEstimate_Click" />
    </div>

    <asp:Panel ID="PanelResult" runat="server" Visible="false">
        <div class="card">
            <h4><span class="glyphicon glyphicon-flash"></span> Insurance &amp; Cost Guide</h4>
            <div class="alert alert-warning" style="margin-bottom:12px;">
                <span class="glyphicon glyphicon-info-sign"></span>
                &nbsp;Guide for: <strong><asp:Literal ID="LitService" runat="server" /></strong>
                &nbsp;|&nbsp; Insurance: <strong><asp:Literal ID="LitInsurance" runat="server" /></strong>
            </div>
            <div class="guide-output">
                <asp:Literal ID="LitGuide" runat="server" />
            </div>
            <p class="disclaimer">
                <span class="glyphicon glyphicon-exclamation-sign"></span>
                This is general guidance only and not financial or legal advice.
                Contact your insurance provider and our billing team for exact details specific to your plan.
            </p>
        </div>
        <div class="text-center" style="margin-bottom:30px;">
            <a href="AppointmentForm.aspx" class="btn btn-primary">Book an Appointment</a>
            <a href="ContactForm.aspx" class="btn btn-default" style="margin-left:8px;">Contact Our Billing Team</a>
        </div>
    </asp:Panel>
</div>
</asp:Content>
